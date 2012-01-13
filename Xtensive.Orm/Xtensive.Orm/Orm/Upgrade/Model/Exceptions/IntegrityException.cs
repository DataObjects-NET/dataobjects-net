// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
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

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    public IntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}