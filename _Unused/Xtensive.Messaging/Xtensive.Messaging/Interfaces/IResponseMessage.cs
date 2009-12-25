// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08


namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes message interface for responses received by <see cref="Sender.Ask(IMessage)"/> method.
  /// </summary>
  public interface IResponseMessage: IMessage
  {
    /// <summary>
    /// Gets or sets response query identifier.
    /// </summary>
    long ResponseQueryId { get; set; }

  }
}