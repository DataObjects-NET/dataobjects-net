// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Describes connection that receives data.
  /// </summary>
  public interface IReceivingConnection: IConnection
  {
    /// <summary>
    /// Thrown than connection receives data.
    /// </summary>
    event EventHandler<DataReceivedEventArgs> DataReceived;
  }
}