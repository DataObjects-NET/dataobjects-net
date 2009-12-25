// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using Xtensive.Core;
using System;

namespace Xtensive.Messaging.Providers
{

  /// <summary>
  /// Base connection interface.
  /// </summary>
  public interface IConnection : IResource
  {
    // Events

    /// <summary>
    /// Occurs than connection closed.
    /// </summary>
    event EventHandler Closed;
  }
}