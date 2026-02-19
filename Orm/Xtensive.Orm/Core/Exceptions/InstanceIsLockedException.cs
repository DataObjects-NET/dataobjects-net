// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;


namespace Xtensive.Core
{
  /// <summary>
  /// Thrown by <see cref="ILockable"/> implementors on attempts 
  /// to change instance properties in the locked state.
  /// </summary>
  public sealed class InstanceIsLockedException : InvalidOperationException
  {
    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public InstanceIsLockedException()
      : base(Strings.ExInstanceIsLocked)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="text">Text of message.</param>
    public InstanceIsLockedException(string text)
      : base(text)
    {
    }
  }
}