// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.19

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Container to send responses asked by <see cref="Sender"/>
  /// </summary>
  [Serializable]
  internal class DataResponseMessage: IResponseMessage, ISerializable, IDataMessage
  {
    private long queryId;
    private long responseQueryId;
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
    /// Gets or sets object to send.
    /// </summary>
    public object Data
    {
      get { return data; }
      set { data = value; }
    }

    /// <summary>
    /// Gets or sets response query identifier.
    /// </summary>
    public long ResponseQueryId
    {
      get { return responseQueryId; }
      set { responseQueryId = value; }
    }

    // Constructors

    public DataResponseMessage()
    {
    }

    public DataResponseMessage(object data)
      : this()
    {
      this.data = data;
    }

    #region ISerializable Members

    protected DataResponseMessage(SerializationInfo info, StreamingContext context)
    {
      queryId = info.GetInt64("QueryId");
      responseQueryId = info.GetInt64("ResponseQueryId");
      data = info.GetValue("Data", typeof(object));
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("QueryId", queryId);
      info.AddValue("ResponseQueryId", responseQueryId);
      info.AddValue("Data", data);
    }

    #endregion
  }
}