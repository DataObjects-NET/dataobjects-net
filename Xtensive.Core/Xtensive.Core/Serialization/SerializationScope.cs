// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Serialization scope.
  /// </summary>
  public abstract class SerializationScope : Scope<SerializationContext>
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    protected SerializationScope(SerializationContext context) 
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    protected SerializationScope() 
    {
    }
  }
}