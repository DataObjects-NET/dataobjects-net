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
  /// An exception that is thrown when generated RDBMS query contains syntax error.
  /// When you've got this exception, most probably you've found an error in DataObjects.
  /// </summary>
  [Serializable]
  public class SyntaxErrorException : StorageException
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public SyntaxErrorException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SyntaxErrorException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}