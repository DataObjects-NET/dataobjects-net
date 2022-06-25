// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
  public readonly struct KeyEventArgs
  {
    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; }


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
