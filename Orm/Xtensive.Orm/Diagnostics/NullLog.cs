// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Null log implementation. Does nothing.
  /// </summary>
  public sealed class NullLog : RealLogImplementationBase
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="name">Log name.</param>
    internal NullLog(string name)
      : base(name)
    {
    }
  }
}