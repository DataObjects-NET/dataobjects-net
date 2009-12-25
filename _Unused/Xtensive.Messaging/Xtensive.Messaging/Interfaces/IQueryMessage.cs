// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes base interface for query messages.
  /// </summary>
  public interface IQueryMessage : IMessage
  {

    /// <summary>
    /// Gets or sets url of <see cref="Receiver"/> to send response to.
    /// </summary>
    string ReceiverUrl { get; set; }

    /// <summary>
    /// Gets or sets <see cref="TimeSpan"/> indicating how long <see cref="Sender"/> will be waiting for the response.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Calculates <see cref="Timeout"/> value for the message.
    /// </summary>
    /// <returns></returns>
    TimeSpan? CalculateTimeout();
  }
}