// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Runtime.Serialization;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Describes errors detected during <see cref="StorageModel"/>.<see cref="Node.Validate"/> execution.
  /// </summary>
  [Serializable]
  public class IntegrityException : Exception
  {
    /// <summary>
    /// Gets the path of the node which validation has failed.
    /// </summary>
    public string NodePath { get; private set; }


    // Constructors

    protected IntegrityException()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="nodePath">The path of the invalid node.</param>
    public IntegrityException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    // Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrityException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    ///   
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    public IntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}