// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;

namespace Xtensive.Storage.Serialization
{
  /// <summary>
  /// Serialization scope.
  /// </summary>
  public sealed class SerializationScope : Scope<SerializationContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static SerializationContext CurrentContext
    {
      get { return Scope<SerializationContext>.CurrentContext; }
    }

    
    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public SerializationScope(SerializationContext context) 
      : base(context)
    {
    }
  }
}