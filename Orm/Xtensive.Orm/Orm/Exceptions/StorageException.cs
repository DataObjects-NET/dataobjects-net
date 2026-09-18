// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for any storage-level exception.
  /// </summary>
  public class StorageException : Exception
  {
    /// <summary>
    /// Context information about occurred error.
    /// </summary>
    public StorageExceptionInfo Info { get; init; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    public StorageException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StorageException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}