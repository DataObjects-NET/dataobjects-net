// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

namespace Xtensive.Messaging
{
  /// <summary>
  /// Base interface for messages.
  /// </summary>
  public interface IMessage
  {
    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    long QueryId { get; set; }

  }
}