#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Messages.Messages
File: BaseConnectionMessage.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License
namespace StockSharp.Messages
{
	using System;
	using System.Runtime.Serialization;
	using System.Xml.Serialization;

	/// <summary>
	/// Base connect/disconnect message.
	/// </summary>
	[DataContract]
	[Serializable]
	public abstract class BaseConnectionMessage : Message
	{
		/// <summary>
		/// Initialize <see cref="BaseConnectionMessage"/>.
		/// </summary>
		/// <param name="type">Message type.</param>
		protected BaseConnectionMessage(MessageTypes type)
			: base(type)
		{
		}

		/// <summary>
		/// Information about the error connection or disconnection.
		/// </summary>
		[DataMember]
		[XmlIgnore]
		public Exception Error { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return base.ToString() + (Error == null ? null : $",Error={Error.Message}");
		}
	}
}