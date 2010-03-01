// Copyright (C) 2003-2010 Xtensive LLC.
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
  /// Base class for any exception thrown as result of concurrency conflict
  /// (e.g. deadlock or version conflict).
  /// </summary>
  [Serializable]
  public abstract class ConcurrencyConflictException : Exception,
    IConcurrencyConflictException
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public ConcurrencyConflictException()
      : base(Strings.ExConcurrencyConflict)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="text">Text of message.</param>
    public ConcurrencyConflictException(string text)
      : base(text)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ConcurrencyConflictException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}