// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.15

using System;
using Xtensive.Collections;

namespace Xtensive.Notifications
{
  /// <summary>
  /// An object exposing change related events contract.
  /// </summary>
  public interface IChangeNotifier
  {
    /// <summary>
    /// Occurs when this instance is about to be changed.
    /// </summary>
    event EventHandler<ChangeNotifierEventArgs> Changing;

    /// <summary>
    /// Occurs when this instance is changed.
    /// </summary>
    event EventHandler<ChangeNotifierEventArgs> Changed;
  }
}