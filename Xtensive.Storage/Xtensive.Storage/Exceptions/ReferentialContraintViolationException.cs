// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// An exception that is thrown when referential constaint (aka foreign key) is violated.
  /// This differs from <see cref="ReferentialIntegrityException"/>.
  /// <see cref="ReferentialContraintViolationException"/> is thrown when RDBMS detects a violation.
  /// <see cref="Xtensive.Storage.ReferentialIntegrity"/> is thrown when internal referential integrity
  /// mechanism detects a violation.
  /// </summary>
  [Serializable]
  public class ReferentialContraintViolationException : ConstraintViolationException
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public ReferentialContraintViolationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ReferentialContraintViolationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ReferentialContraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }    
  }
}