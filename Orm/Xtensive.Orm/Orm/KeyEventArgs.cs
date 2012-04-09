// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Diagnostics;


namespace Xtensive.Orm
{
  /// <summary>
  /// Arguments for <see cref="Key"/>-related events.
  /// </summary>
  [Serializable]
  public sealed class KeyEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="key">The key.</param>
    public KeyEventArgs(Key key)
    {
      Key = key;
    }
  }
}