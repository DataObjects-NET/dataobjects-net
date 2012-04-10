// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Runtime.Serialization;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when RDBMS detects a violation of a constraint.
  /// <seealso cref="ReferentialConstraintViolationException"/>
  /// <seealso cref="UniqueConstraintViolationException"/>
  /// <seealso cref="CheckConstraintViolationException"/>
  /// </summary>
  [Serializable]
  public abstract class ConstraintViolationException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConstraintViolationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConstraintViolationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ConstraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }     
  }
}