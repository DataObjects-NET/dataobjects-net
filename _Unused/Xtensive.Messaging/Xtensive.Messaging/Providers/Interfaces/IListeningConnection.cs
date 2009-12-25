// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Waits and notifies subscribers about new incoming <see cref="IReceivingConnection"/>
  /// </summary>
  public interface IListeningConnection: IConnection
  {
    /// <summary>
    /// Listening connection accepts new incoming connection.
    /// </summary>
    event EventHandler Accepted;
  }
}