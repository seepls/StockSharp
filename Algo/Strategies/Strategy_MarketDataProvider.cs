namespace StockSharp.Algo.Strategies
{
	using System;
	using System.Collections.Generic;

	using StockSharp.BusinessEntities;
	using StockSharp.Messages;

	partial class Strategy
	{
		/// <inheritdoc />
		public event Action<Security, IEnumerable<KeyValuePair<Level1Fields, object>>, DateTimeOffset, DateTimeOffset> ValuesChanged;

		/// <inheritdoc />
		public MarketDepth GetMarketDepth(Security security)
		{
			return SafeGetConnector().GetMarketDepth(security);
		}

		/// <inheritdoc />
		public object GetSecurityValue(Security security, Level1Fields field)
		{
			if (security == null)
				throw new ArgumentNullException(nameof(security));

			return SafeGetConnector().GetSecurityValue(security, field);
		}

		/// <inheritdoc />
		public IEnumerable<Level1Fields> GetLevel1Fields(Security security)
		{
			if (security == null)
				throw new ArgumentNullException(nameof(security));

			return SafeGetConnector().GetLevel1Fields(security);
		}

		/// <inheritdoc />
		public event Action<Trade> NewTrade
		{
			add => SafeGetConnector().NewTrade += value;
			remove => SafeGetConnector().NewTrade -= value;
		}

		/// <inheritdoc />
		public event Action<Security> NewSecurity
		{
			add => SafeGetConnector().NewSecurity += value;
			remove => SafeGetConnector().NewSecurity -= value;
		}

		/// <inheritdoc />
		public event Action<Security> SecurityChanged
		{
			add => SafeGetConnector().SecurityChanged += value;
			remove => SafeGetConnector().SecurityChanged -= value;
		}

		/// <inheritdoc />
		public event Action<MarketDepth> NewMarketDepth
		{
			add => SafeGetConnector().NewMarketDepth += value;
			remove => SafeGetConnector().NewMarketDepth -= value;
		}

		/// <inheritdoc />
		public event Action<MarketDepth> MarketDepthChanged
		{
			add => SafeGetConnector().MarketDepthChanged += value;
			remove => SafeGetConnector().MarketDepthChanged -= value;
		}

		/// <inheritdoc />
		public event Action<OrderLogItem> NewOrderLogItem
		{
			add => SafeGetConnector().NewOrderLogItem += value;
			remove => SafeGetConnector().NewOrderLogItem -= value;
		}

		/// <inheritdoc />
		public event Action<News> NewNews
		{
			add => SafeGetConnector().NewNews += value;
			remove => SafeGetConnector().NewNews -= value;
		}

		/// <inheritdoc />
		public event Action<News> NewsChanged
		{
			add => SafeGetConnector().NewsChanged += value;
			remove => SafeGetConnector().NewsChanged -= value;
		}

		/// <inheritdoc />
		public event Action<SecurityLookupMessage, IEnumerable<Security>, Exception> LookupSecuritiesResult
		{
			add => SafeGetConnector().LookupSecuritiesResult += value;
			remove => SafeGetConnector().LookupSecuritiesResult -= value;
		}

		/// <inheritdoc />
		public event Action<SecurityLookupMessage, IEnumerable<Security>, IEnumerable<Security>, Exception> LookupSecuritiesResult2
		{
			add => SafeGetConnector().LookupSecuritiesResult2 += value;
			remove => SafeGetConnector().LookupSecuritiesResult2 -= value;
		}

		/// <inheritdoc />
		public event Action<BoardLookupMessage, IEnumerable<ExchangeBoard>, Exception> LookupBoardsResult
		{
			add => SafeGetConnector().LookupBoardsResult += value;
			remove => SafeGetConnector().LookupBoardsResult -= value;
		}

		/// <inheritdoc />
		public event Action<BoardLookupMessage, IEnumerable<ExchangeBoard>, IEnumerable<ExchangeBoard>, Exception> LookupBoardsResult2
		{
			add => SafeGetConnector().LookupBoardsResult2 += value;
			remove => SafeGetConnector().LookupBoardsResult2 -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage> MarketDataSubscriptionSucceeded
		{
			add => SafeGetConnector().MarketDataSubscriptionSucceeded += value;
			remove => SafeGetConnector().MarketDataSubscriptionSucceeded -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage, Exception> MarketDataSubscriptionFailed
		{
			add => SafeGetConnector().MarketDataSubscriptionFailed += value;
			remove => SafeGetConnector().MarketDataSubscriptionFailed -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage, MarketDataMessage> MarketDataSubscriptionFailed2
		{
			add => SafeGetConnector().MarketDataSubscriptionFailed2 += value;
			remove => SafeGetConnector().MarketDataSubscriptionFailed2 -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage> MarketDataUnSubscriptionSucceeded
		{
			add => SafeGetConnector().MarketDataUnSubscriptionSucceeded += value;
			remove => SafeGetConnector().MarketDataUnSubscriptionSucceeded -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage, Exception> MarketDataUnSubscriptionFailed
		{
			add => SafeGetConnector().MarketDataUnSubscriptionFailed += value;
			remove => SafeGetConnector().MarketDataUnSubscriptionFailed -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage, MarketDataMessage> MarketDataUnSubscriptionFailed2
		{
			add => SafeGetConnector().MarketDataUnSubscriptionFailed2 += value;
			remove => SafeGetConnector().MarketDataUnSubscriptionFailed2 -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataFinishedMessage> MarketDataSubscriptionFinished
		{
			add => SafeGetConnector().MarketDataSubscriptionFinished += value;
			remove => SafeGetConnector().MarketDataSubscriptionFinished -= value;
		}

		/// <inheritdoc />
		public event Action<Security, MarketDataMessage, Exception> MarketDataUnexpectedCancelled
		{
			add => SafeGetConnector().MarketDataUnexpectedCancelled += value;
			remove => SafeGetConnector().MarketDataUnexpectedCancelled -= value;
		}

		/// <inheritdoc />
		public void LookupSecurities(SecurityLookupMessage criteria)
		{
			SafeGetConnector().LookupSecurities(criteria);
		}

		/// <inheritdoc />
		public void LookupBoards(BoardLookupMessage criteria)
		{
			SafeGetConnector().LookupBoards(criteria);
		}

		/// <inheritdoc />
		public MarketDepth GetFilteredMarketDepth(Security security)
		{
			return SafeGetConnector().GetFilteredMarketDepth(security);
		}

		/// <inheritdoc />
		public void SubscribeMarketData(Security security, MarketDataMessage message)
		{
			SafeGetConnector().SubscribeMarketData(security, message);
		}

		/// <inheritdoc />
		public void UnSubscribeMarketData(Security security, MarketDataMessage message)
		{
			SafeGetConnector().UnSubscribeMarketData(security, message);
		}

		/// <inheritdoc />
		public void SubscribeMarketData(MarketDataMessage message)
		{
			SafeGetConnector().SubscribeMarketData(message);
		}

		/// <inheritdoc />
		public void UnSubscribeMarketData(MarketDataMessage message)
		{
			SafeGetConnector().UnSubscribeMarketData(message);
		}

		/// <inheritdoc />
		public void RegisterMarketDepth(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, int? maxDepth = null, IMessageAdapter adapter = null)
		{
			SafeGetConnector().RegisterMarketDepth(security, from, to, count, buildMode, buildFrom, maxDepth, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterMarketDepth(Security security)
		{
			SafeGetConnector().UnRegisterMarketDepth(security);
		}

		/// <inheritdoc />
		public void RegisterFilteredMarketDepth(Security security)
		{
			SafeGetConnector().RegisterFilteredMarketDepth(security);
		}

		/// <inheritdoc />
		public void UnRegisterFilteredMarketDepth(Security security)
		{
			SafeGetConnector().UnRegisterFilteredMarketDepth(security);
		}

		/// <inheritdoc />
		public void RegisterTrades(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, IMessageAdapter adapter = null)
		{
			SafeGetConnector().RegisterTrades(security, from, to, count, buildMode, buildFrom, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterTrades(Security security)
		{
			SafeGetConnector().UnRegisterTrades(security);
		}

		/// <inheritdoc />
		public void RegisterSecurity(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, MarketDataBuildModes buildMode = MarketDataBuildModes.LoadAndBuild, MarketDataTypes? buildFrom = null, IMessageAdapter adapter = null)
		{
			SafeGetConnector().RegisterSecurity(security, from, to, count, buildMode, buildFrom, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterSecurity(Security security)
		{
			SafeGetConnector().UnRegisterSecurity(security);
		}

		/// <inheritdoc />
		public void RegisterOrderLog(Security security, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = null, IMessageAdapter adapter = null)
		{
			SafeGetConnector().RegisterOrderLog(security, from, to, count, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterOrderLog(Security security)
		{
			SafeGetConnector().UnRegisterOrderLog(security);
		}

		/// <inheritdoc />
		public void RegisterNews(Security security = null, IMessageAdapter adapter = null)
		{
			SafeGetConnector().RegisterNews(security, adapter);
		}

		/// <inheritdoc />
		public void UnRegisterNews(Security security = null)
		{
			SafeGetConnector().UnRegisterNews(security);
		}

		/// <inheritdoc />
		public void SubscribeBoard(ExchangeBoard board, IMessageAdapter adapter = null)
		{
			SafeGetConnector().SubscribeBoard(board, adapter);
		}

		/// <inheritdoc />
		public void UnSubscribeBoard(ExchangeBoard board)
		{
			SafeGetConnector().UnSubscribeBoard(board);
		}
	}
}