#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Algo.Algo
File: Connector_Subscription.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License
namespace StockSharp.Algo
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Ecng.Collections;
	using Ecng.Common;

	using MoreLinq;

	using StockSharp.Algo.Candles;
	using StockSharp.BusinessEntities;
	using StockSharp.Localization;
	using StockSharp.Logging;
	using StockSharp.Messages;

	partial class Connector
	{
		private sealed class SubscriptionManager
		{
			private readonly SynchronizedDictionary<long, Tuple<MarketDataMessage, Security>> _requests = new SynchronizedDictionary<long, Tuple<MarketDataMessage, Security>>();
			private readonly SynchronizedDictionary<MarketDataTypes, CachedSynchronizedSet<Security>> _subscribers = new SynchronizedDictionary<MarketDataTypes, CachedSynchronizedSet<Security>>();
			private readonly Connector _connector;

			public SubscriptionManager(Connector connector)
			{
				_connector = connector ?? throw new ArgumentNullException(nameof(connector));
			}

			public void ClearCache()
			{
				_subscribers.Clear();
				_registeredPortfolios.Clear();
				_requests.Clear();
			}

			private IEnumerable<Security> GetSubscribers(MarketDataTypes type)
			{
				return _subscribers.TryGetValue(type)?.Cache ?? ArrayHelper.Empty<Security>();
			}

			public IEnumerable<Security> RegisteredSecurities => GetSubscribers(MarketDataTypes.Level1);

			public IEnumerable<Security> RegisteredMarketDepths => GetSubscribers(MarketDataTypes.MarketDepth);

			public IEnumerable<Security> RegisteredTrades => GetSubscribers(MarketDataTypes.Trades);

			public IEnumerable<Security> RegisteredOrderLogs => GetSubscribers(MarketDataTypes.OrderLog);

			private readonly CachedSynchronizedSet<Portfolio> _registeredPortfolios = new CachedSynchronizedSet<Portfolio>();

			public IEnumerable<Portfolio> RegisteredPortfolios => _registeredPortfolios.Cache;

			public void ProcessRequest(Security security, MarketDataMessage message, bool tryAdd)
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));

				if (!tryAdd)
				{
					var msg = (message.IsSubscribe ? LocalizedStrings.SubscriptionSent : LocalizedStrings.UnSubscriptionSent)
						.Put(security?.Id, message.ToDataTypeString());

					if (message.From != null && message.To != null)
						msg += LocalizedStrings.Str691Params.Put(message.From.Value, message.To.Value);

					_connector.AddDebugLog(msg + ".");
				}

				if (security == null)
				{
					if (!message.IsSubscribe)
					{
						if (message.OriginalTransactionId != 0)
							security = TryGetSecurity(message.OriginalTransactionId);
					}
				}

				if (security == null)
				{
					//if (message.DataType != MarketDataTypes.News)
					//{
						
					//}

					if (message.SecurityId != default)
					{
						security = _connector.LookupById(message.SecurityId);

						if (security == null)
							throw new ArgumentException(LocalizedStrings.Str704Params.Put(message.SecurityId));
					}
				}

				if (message.TransactionId == 0)
					message.TransactionId = _connector.TransactionIdGenerator.GetNextId();

				if (security != null)
					message.FillSecurityInfo(_connector, security);

				var value = Tuple.Create((MarketDataMessage)message.Clone(), security);

				if (tryAdd)
				{
					// if the message was looped back via IsBack=true
					_requests.TryAdd(message.TransactionId, value);
				}
				else
					_requests.Add(message.TransactionId, value);

				_connector.SendInMessage(message);
			}

			public void RegisterPortfolio(Portfolio portfolio)
			{
				if (portfolio == null)
					throw new ArgumentNullException(nameof(portfolio));

				if (portfolio is BasketPortfolio basketPortfolio)
					basketPortfolio.InnerPortfolios.ForEach(_connector.RegisterPortfolio);
				else
				{
					_registeredPortfolios.Add(portfolio);
					_connector.OnRegisterPortfolio(portfolio);
				}
			}

			public void UnRegisterPortfolio(Portfolio portfolio)
			{
				if (portfolio == null)
					throw new ArgumentNullException(nameof(portfolio));

				if (portfolio is BasketPortfolio basketPortfolio)
					basketPortfolio.InnerPortfolios.ForEach(_connector.UnRegisterPortfolio);
				else
				{
					_registeredPortfolios.Remove(portfolio);
					_connector.OnUnRegisterPortfolio(portfolio);
				}
			}

			public Security TryGetSecurity(long originalTransactionId)
			{
				return _requests.TryGetValue(originalTransactionId)?.Item2;
			}

			public Security ProcessResponse(MarketDataMessage response, out MarketDataMessage originalMsg, out bool unexpectedCancelled)
			{
				unexpectedCancelled = false;

				if (!_requests.TryGetValue(response.OriginalTransactionId, out var tuple))
				{
					originalMsg = null;
					return null;
				}

				//_requests.Remove(response.OriginalTransactionId);

				var subscriber = tuple.Item2;
				originalMsg = tuple.Item1;

				if (originalMsg.DataType != MarketDataTypes.News)
				{
					lock (_subscribers.SyncRoot)
					{
						if (originalMsg.IsSubscribe)
						{
							if (response.IsOk())
								_subscribers.SafeAdd(originalMsg.DataType).Add(subscriber);
							else
							{
								var set = _subscribers.TryGetValue(originalMsg.DataType);

								if (set != null && set.Remove(subscriber))
								{
									unexpectedCancelled = true;
								}
							}
						}
						else
						{
							var dict = _subscribers.TryGetValue(originalMsg.DataType);

							if (dict != null)
							{
								dict.Remove(subscriber);

								if (dict.Count == 0)
									_subscribers.Remove(originalMsg.DataType);
							}
						}
					}
				}
				
				return subscriber;
			}
		}

		/// <inheritdoc />
		public IEnumerable<Security> RegisteredSecurities => _subscriptionManager.RegisteredSecurities;

		/// <inheritdoc />
		public IEnumerable<Security> RegisteredMarketDepths => _subscriptionManager.RegisteredMarketDepths;

		/// <inheritdoc />
		public IEnumerable<Security> RegisteredTrades => _subscriptionManager.RegisteredTrades;

		/// <inheritdoc />
		public IEnumerable<Security> RegisteredOrderLogs => _subscriptionManager.RegisteredOrderLogs;

		/// <inheritdoc />
		public IEnumerable<Portfolio> RegisteredPortfolios => _subscriptionManager.RegisteredPortfolios;

		/// <summary>
		/// List of all candles series, subscribed via <see cref="SubscribeCandles"/>.
		/// </summary>
		public IEnumerable<CandleSeries> SubscribedCandleSeries => _entityCache.AllCandleSeries;

		/// <inheritdoc />
		public virtual void SubscribeMarketData(MarketDataMessage message)
		{
			SubscribeMarketData(null, message);
		}

		/// <inheritdoc />
		public virtual void SubscribeMarketData(Security security, MarketDataMessage message)
		{
			_subscriptionManager.ProcessRequest(security, message, false);
		}

		/// <inheritdoc />
		public virtual void UnSubscribeMarketData(MarketDataMessage message)
		{
			UnSubscribeMarketData(null, message);
		}

		/// <inheritdoc />
		public virtual void UnSubscribeMarketData(Security security, MarketDataMessage message)
		{
			_subscriptionManager.ProcessRequest(security, message, false);
		}

		private void SubscribeMarketData(Security security, MarketDataTypes type, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, Level1Fields? buildField = null, int? maxDepth = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, new MarketDataMessage
			{
				DataType = type,
				IsSubscribe = true,
				From = from,
				To = to,
				Count = count,
				BuildMode = buildMode,
				BuildFrom = buildFrom,
				BuildField = buildField,
				MaxDepth = maxDepth,
				Adapter = adapter
			});
		}

		private void UnSubscribeMarketData(Security security, MarketDataTypes type)
		{
			UnSubscribeMarketData(security, new MarketDataMessage
			{
				DataType = type,
				IsSubscribe = false,
			});
		}

		/// <inheritdoc />
		public void RegisterSecurity(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, MarketDataTypes.Level1, from, to, count, buildMode, buildFrom, adapter: adapter);
		}

		/// <inheritdoc />
		public void UnRegisterSecurity(Security security)
		{
			UnSubscribeMarketData(security, MarketDataTypes.Level1);
		}

		/// <inheritdoc />
		public void RegisterMarketDepth(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, int? maxDepth = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, MarketDataTypes.MarketDepth, from, to, count, buildMode, buildFrom, null, maxDepth, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterMarketDepth(Security security)
		{
			UnSubscribeMarketData(security, MarketDataTypes.MarketDepth);
		}

		/// <inheritdoc />
		public void RegisterFilteredMarketDepth(Security security)
		{
			if (security == null)
				throw new ArgumentNullException(nameof(security));

			var quotes = GetMarketDepth(security).ToMessage();
			var executions = _entityCache
				.GetOrders(security, OrderStates.Active)
				.Select(o => o.ToMessage())
				.ToArray();

			SubscribeMarketData(security, new MarketDataMessage
			{
				DataType = FilteredMarketDepthAdapter.FilteredMarketDepth,
				IsSubscribe = true,
				Arg = Tuple.Create(quotes, executions)
			});
		}

		/// <inheritdoc />
		public void UnRegisterFilteredMarketDepth(Security security)
		{
			UnSubscribeMarketData(security, FilteredMarketDepthAdapter.FilteredMarketDepth);
		}

		/// <inheritdoc />
		public void RegisterTrades(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, MarketDataTypes.Trades, from, to, count, buildMode, buildFrom, adapter: adapter);
		}

		/// <inheritdoc />
		public void UnRegisterTrades(Security security)
		{
			UnSubscribeMarketData(security, MarketDataTypes.Trades);
		}

		/// <inheritdoc />
		public void RegisterPortfolio(Portfolio portfolio)
		{
			_subscriptionManager.RegisterPortfolio(portfolio);
		}

		/// <summary>
		/// Subscribe on the portfolio changes.
		/// </summary>
		/// <param name="portfolio">Portfolio for subscription.</param>
		protected virtual void OnRegisterPortfolio(Portfolio portfolio)
		{
			SendInMessage(new PortfolioMessage
			{
				PortfolioName = portfolio.Name,
				TransactionId = TransactionIdGenerator.GetNextId(),
				IsSubscribe = true
			});
		}

		/// <inheritdoc />
		public void UnRegisterPortfolio(Portfolio portfolio)
		{
			_subscriptionManager.UnRegisterPortfolio(portfolio);
		}

		/// <summary>
		/// Unsubscribe from the portfolio changes.
		/// </summary>
		/// <param name="portfolio">Portfolio for unsubscription.</param>
		protected virtual void OnUnRegisterPortfolio(Portfolio portfolio)
		{
			SendInMessage(new PortfolioMessage
			{
				PortfolioName = portfolio.Name,
				TransactionId = TransactionIdGenerator.GetNextId(),
				IsSubscribe = false
			});
		}

		/// <inheritdoc />
		public void RegisterOrderLog(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, MarketDataTypes.OrderLog, from, to, count, adapter: adapter);
		}

		/// <inheritdoc />
		public void UnRegisterOrderLog(Security security)
		{
			UnSubscribeMarketData(security, MarketDataTypes.OrderLog);
		}

		/// <inheritdoc />
		public void RegisterNews(Security security = null, IMessageAdapter adapter = null)
		{
			OnRegisterNews(security, adapter);
		}

		/// <summary>
		/// Subscribe on news.
		/// </summary>
		/// <param name="security">Security for subscription.</param>
		/// <param name="adapter">Target adapter. Can be <see langword="null" />.</param>
		protected virtual void OnRegisterNews(Security security = null, IMessageAdapter adapter = null)
		{
			SubscribeMarketData(security, MarketDataTypes.News, adapter: adapter);
		}

		/// <inheritdoc />
		public void UnRegisterNews(Security security = null)
		{
			OnUnRegisterNews(security);
		}

		/// <inheritdoc />
		public void SubscribeBoard(ExchangeBoard board, IMessageAdapter adapter = null)
		{
			if (board == null)
				throw new ArgumentNullException(nameof(board));

			SendInMessage(new BoardRequestMessage
			{
				IsSubscribe = true,
				BoardCode = board.Code,
				TransactionId = TransactionIdGenerator.GetNextId(),
				Adapter = adapter,
			});
		}

		/// <inheritdoc />
		public void UnSubscribeBoard(ExchangeBoard board)
		{
			if (board == null)
				throw new ArgumentNullException(nameof(board));

			SendInMessage(new BoardRequestMessage
			{
				IsSubscribe = false,
				BoardCode = board.Code,
				TransactionId = TransactionIdGenerator.GetNextId(),
			});
		}

		/// <inheritdoc />
		public virtual void RequestNewsStory(News news, IMessageAdapter adapter = null)
		{
			if (news == null)
				throw new ArgumentNullException(nameof(news));

			SubscribeMarketData(null, new MarketDataMessage
			{
				TransactionId = TransactionIdGenerator.GetNextId(),
				DataType = MarketDataTypes.News,
				IsSubscribe = true,
				NewsId = news.Id.To<string>(),
				Adapter = adapter,
			});
		}

		/// <summary>
		/// Unsubscribe from news.
		/// </summary>
		/// <param name="security">Security for subscription.</param>
		protected virtual void OnUnRegisterNews(Security security = null)
		{
			UnSubscribeMarketData(security, MarketDataTypes.News);
		}

		/// <summary>
		/// Subscribe to receive new candles.
		/// </summary>
		/// <param name="series">Candles series.</param>
		/// <param name="from">The initial date from which you need to get data.</param>
		/// <param name="to">The final date by which you need to get data.</param>
		/// <param name="count">Candles count.</param>
		/// <param name="transactionId">Transaction ID.</param>
		/// <param name="extensionInfo">Extended information.</param>
		/// <param name="adapter">Target adapter. Can be <see langword="null" />.</param>
		public virtual void SubscribeCandles(CandleSeries series, DateTimeOffset? from = null, DateTimeOffset? to = null,
			long? count = null, long? transactionId = null, IDictionary<string, object> extensionInfo = null, IMessageAdapter adapter = null)
		{
			if (series == null)
				throw new ArgumentNullException(nameof(series));

			var mdMsg = series.ToMarketDataMessage(true, from, to, count);
			mdMsg.TransactionId = transactionId ?? TransactionIdGenerator.GetNextId();
			mdMsg.ExtensionInfo = extensionInfo;
			mdMsg.Adapter = adapter;

			_entityCache.CreateCandleSeries(mdMsg, series);

			SubscribeMarketData(series.Security, mdMsg);
		}

		/// <summary>
		/// To stop the candles receiving subscription, previously created by <see cref="SubscribeCandles"/>.
		/// </summary>
		/// <param name="series">Candles series.</param>
		public virtual void UnSubscribeCandles(CandleSeries series)
		{
			var originalTransId = _entityCache.TryGetTransactionId(series);

			if (originalTransId == 0)
				return;

			var mdMsg = series.ToMarketDataMessage(false);
			mdMsg.TransactionId = TransactionIdGenerator.GetNextId();
			mdMsg.OriginalTransactionId = originalTransId;
			UnSubscribeMarketData(series.Security, mdMsg);
		}
	}
}