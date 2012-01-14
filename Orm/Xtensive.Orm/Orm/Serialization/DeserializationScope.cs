// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.30

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Orm.Serialization
{
  /// <summary>
  /// Deserialization scope.
  /// </summary>
  public sealed class DeserializationScope : Scope<DeserializationContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static DeserializationContext CurrentContext
    {
      get { return Scope<DeserializationContext>.CurrentContext; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public DeserializationScope(DeserializationContext context) 
      : base(context)
    {
    }
  }
}