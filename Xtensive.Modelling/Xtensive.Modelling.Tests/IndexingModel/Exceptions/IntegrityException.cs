// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Describes errors detected during <see cref="StorageInfo"/>.<see cref="Node.Validate"/> execution.
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="nodePath">The path of the invalid node.</param>
    public IntegrityException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    /// <inheritdoc/>
    public IntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}