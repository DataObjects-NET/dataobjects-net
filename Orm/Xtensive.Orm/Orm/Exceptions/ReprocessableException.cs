// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;
using System.Runtime.Serialization;


namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for any storage-level error,
  /// that can be recovered by rolling back active transaction
  /// and reprocessing all actions in a new one.
  /// </summary>
  [Serializable]
  public abstract class ReprocessableException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ReprocessableException(string message)
      : base(message)
    {
    }

    /// <summary>
    ///	Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ReprocessableException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ReprocessableException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }    
  }
}