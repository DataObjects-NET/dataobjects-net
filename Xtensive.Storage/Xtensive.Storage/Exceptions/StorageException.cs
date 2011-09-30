// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for any storage-level exception.
  /// </summary>
  [Serializable]
  public class StorageException : Exception
  {
    private StorageExceptionInfo info;

    /// <summary>
    /// Context information about occured error.
    /// </summary>
    public StorageExceptionInfo Info {
      get { return info; }
      set {
        if (info!=null)
          throw new InvalidOperationException(Strings.ExValueIsAlreadyAssigned);
        info = value;
      }
    }

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