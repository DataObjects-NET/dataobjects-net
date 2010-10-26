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
  /// Thrown by <see cref="VersionValidator.ValidateVersion(Key,VersionInfo,bool)"/>,
  /// <see cref="VersionSet.Validate(Entity,bool)"/> and similar methods
  /// to indicate version check didn't pass.
  /// </summary>
  [Serializable]
  public class VersionConflictException : StorageException
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public VersionConflictException(string message)
      : base(message)
    {
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public VersionConflictException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected VersionConflictException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }    
  }
}