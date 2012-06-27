// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Serialization scope.
  /// </summary>
  public sealed class SerializationScope : Scope<SerializationContext>
  {
    internal static new SerializationContext CurrentContext
    {
      get { return Scope<SerializationContext>.CurrentContext; }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    internal SerializationScope(SerializationContext context)
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    internal SerializationScope()
    {
    }
  }
}