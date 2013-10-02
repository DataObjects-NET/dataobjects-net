// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;


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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public InstanceIsLockedException()
      : base(Strings.ExInstanceIsLocked)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
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
    protected InstanceIsLockedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}