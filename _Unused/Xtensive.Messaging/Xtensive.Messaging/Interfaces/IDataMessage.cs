// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.08

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes a message that has <see cref="Data"/> property.
  /// </summary>
  public interface IDataMessage : IMessage
  {
    /// <summary>
    /// Gets or sets message data.
    /// </summary>
    object Data { get; set; }
  }
}