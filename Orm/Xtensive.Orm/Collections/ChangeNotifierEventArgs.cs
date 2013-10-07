// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.15

using System;


namespace Xtensive.Collections
{
  /// <summary>
  /// Represents a set of information describing <see cref="IChangeNotifier"/> change.
  /// </summary>
  [Serializable]
  public sealed class ChangeNotifierEventArgs: EventArgs
  {
    private readonly object changeInfo;

    /// <summary>
    /// Gets the object representing some additional change information.
    /// </summary>
    /// <value>The info.</value>
    public object ChangeInfo
    {
      get { return changeInfo; }
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="changeInfo">The info.</param>
    public ChangeNotifierEventArgs(object changeInfo)
    {
      this.changeInfo = changeInfo;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public ChangeNotifierEventArgs()
    {
    }
  }
}