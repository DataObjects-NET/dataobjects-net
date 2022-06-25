// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.10.15

using System;


namespace Xtensive.Collections
{
  /// <summary>
  /// Represents a set of information describing <see cref="IChangeNotifier"/> change.
  /// </summary>
  [Serializable]
  public readonly struct ChangeNotifierEventArgs
  {
    /// <summary>
    /// Gets the object representing some additional change information.
    /// </summary>
    /// <value>The info.</value>
    public object ChangeInfo { get; }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="changeInfo">The info.</param>
    public ChangeNotifierEventArgs(object changeInfo)
    {
      ChangeInfo = changeInfo;
    }
  }
}
