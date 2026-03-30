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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintViolationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    protected ConstraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}