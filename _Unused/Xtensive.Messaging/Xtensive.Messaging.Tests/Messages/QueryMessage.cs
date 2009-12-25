// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.11

using System;
using Xtensive.Messaging;

namespace Xtensive.Messaging.Tests
{
  /// <summary>
  /// Represents base class for requests.
  /// </summary>
  [Serializable]
  public abstract class QueryMessage : IQueryMessage
  {
    private long queryId;
    private string receiverUrl;
    private TimeSpan timeout;

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
    /// Gets or sets <see cref="TimeSpan"/> indicating how long <see cref="Sender"/> will be waiting for the response.
    /// </summary>
    public TimeSpan Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }

    /// <summary>
    /// Calculates <see cref="IQueryMessage.Timeout"/> value for the message.
    /// </summary>
    /// <returns></returns>
    public TimeSpan? CalculateTimeout()
    {
      return null;
    }
  }
}