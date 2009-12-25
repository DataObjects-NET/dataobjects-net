// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.18

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Container for objects to send through <see cref="Sender"/> and receive through <see cref="Receiver"/>
  /// </summary>
  [Serializable]
  internal class DataQueryMessage: IQueryMessage, ISerializable, IDataMessage
  {
    private long queryId;
    private string receiverUrl;
    private TimeSpan timeout;
    private object data;


    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId
    {
      get { return queryId; }
      set { queryId = value; }
    }

    /// <summary>
    /// Gets or sets url of <see cref="Receiver"/> to send response to/
    /// </summary>
    public string ReceiverUrl
    {
      get { return receiverUrl; }
      set { receiverUrl = value; }
    }

    /// <summary>
    /// Gets or sets object to send.
    /// </summary>
    public object Data
    {
      get { return data; }
      set { data = value; }
    }

    /// <summary>
    /// Gets or sets <see cref="TimeSpan"/> indicating how long <see cref="Sender"/> will be waiting for the response.
    /// </summary>
    public TimeSpan Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }

    /// <summary>
    /// Calculates <see cref="Timeout"/> value for the message.
    /// </summary>
    /// <returns></returns>
    public virtual TimeSpan? CalculateTimeout()
    {
      return null;
    }


    // Constructors

    public DataQueryMessage()
    {
    }

    public DataQueryMessage(object data)
      : this()
    {
      this.data = data;
    }

    #region ISerializable Members

    protected DataQueryMessage(SerializationInfo info, StreamingContext context)
    {
      queryId = info.GetInt64("QueryId");
      receiverUrl = info.GetString("ReceiverUrl");
      timeout = (TimeSpan)info.GetValue("Timeout", typeof (TimeSpan));
      data = info.GetValue("Data", typeof (object));
    }

    /// <inheritdoc/>
    [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("QueryId", queryId);
      info.AddValue("ReceiverUrl", receiverUrl);
      info.AddValue("Timeout", timeout);
      info.AddValue("Data", data);
    }

    #endregion
  }
}