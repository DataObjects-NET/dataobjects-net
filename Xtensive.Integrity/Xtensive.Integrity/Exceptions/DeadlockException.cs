// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity
{
  /// <summary>
  /// Thrown as the result of a deadlock.
  /// </summary>
  [Serializable]
  public class DeadlockException : ConcurrencyConflictException
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public DeadlockException()
      : base(Strings.ExConcurrencyConflict)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="text">Text of message.</param>
    public DeadlockException(string text)
      : base(text)
    {
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected DeadlockException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}