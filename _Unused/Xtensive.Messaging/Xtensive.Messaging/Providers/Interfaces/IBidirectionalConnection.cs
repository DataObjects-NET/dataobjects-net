// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Describes bidirectional connection, that can be used to both send and receive data.
  /// </summary>
  public interface IBidirectionalConnection: ISendingConnection, IReceivingConnection
  {
  }
}