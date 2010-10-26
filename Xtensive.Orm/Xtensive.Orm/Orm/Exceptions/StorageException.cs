// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for any storage-level exception.
  /// </summary>
  [Serializable]
  public class StorageException : Exception
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public StorageException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StorageException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected StorageException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}