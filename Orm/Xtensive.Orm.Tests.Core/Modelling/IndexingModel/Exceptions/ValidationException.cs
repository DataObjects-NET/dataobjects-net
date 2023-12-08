// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// Describes errors detected during 
  /// <see cref="StorageInfo"/>.<see cref="Node.Validate"/> execution.
  /// </summary>
  [Serializable]
  public class ValidationException : Exception
  {
    /// <summary>
    /// Gets the path of the node which validation has failed.
    /// </summary>
    public string NodePath { get; private set; }


    // Constructors

    /// <inheritdoc/>
    protected ValidationException()
    {
    }

    /// <inheritdoc/>
    public ValidationException(string message)
      : base(message)
    {
    }

    /// <inheritdoc/>
    public ValidationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public ValidationException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    #region Serializing members

    /// <inheritdoc/>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    protected ValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      NodePath = info.GetString("NodePath");
    }

    /// <inheritdoc/>
    [SecurityCritical]
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("NodePath", NodePath);
    }

    #endregion
  }
}