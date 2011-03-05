// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when RDBMS can not serialize concurrent access
  /// It's nearly the same as <see cref="DeadlockException"/>, but this one is normally
  /// thrown for RDBMS that implement multi-version concurrency control (MVCC) isolation.
  /// This exception is unrelated to .NET serialization!
  /// </summary>
  [Serializable]
  public class TransactionSerializationFailureException : ReprocessableException
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public TransactionSerializationFailureException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TransactionSerializationFailureException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected TransactionSerializationFailureException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }     
  }
}