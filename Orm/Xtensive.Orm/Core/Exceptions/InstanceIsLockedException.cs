// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Runtime.Serialization;



namespace Xtensive.Core
{
  /// <summary>
  /// Thrown by <see cref="ILockable"/> implementors on attempts 
  /// to change instance properties in the locked state.
  /// </summary>
  [Serializable]
  public class InstanceIsLockedException : InvalidOperationException
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

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    /// <param name="info"><see cref="SerializationInfo"/> object.</param>
    /// <param name="context"><see cref="StreamingContext"/> object.</param>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    protected InstanceIsLockedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}