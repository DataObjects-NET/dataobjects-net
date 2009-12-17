// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for any storage-level error,
  /// that can be recovered by rolling back active transaction
  /// and reprocessing all actions in a new one.
  /// </summary>
  [Serializable]
  public class ReprocessableStorageException : StorageException
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public ReprocessableStorageException(string message)
      : base(message)
    {
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ReprocessableStorageException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}