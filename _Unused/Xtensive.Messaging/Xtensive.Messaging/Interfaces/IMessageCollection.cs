// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24


using System;
using System.Collections;
using Xtensive.Core.Threading;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Defines message collection. 
  /// </summary>
  public interface IMessageCollection: ICollection, IDisposable, IHasSyncRoot
  {
    /// <summary>
    /// Gets or sets timeout. Collection will wait for a specified period of time for data from remote side
    /// before throw <see cref="TimeoutException"/>.
    /// </summary>
    TimeSpan Timeout { get; set; }
  }
}