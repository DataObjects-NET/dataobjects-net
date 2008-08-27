// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// <see cref="EnumerationContext"/> activation scope.
  /// </summary>
  public class EnumerationScope : Scope<EnumerationContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static EnumerationContext CurrentContext
    {
      get { return Scope<EnumerationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new EnumerationContext Context
    {
      get { return base.Context; }
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public EnumerationScope()
      : base (new EnumerationContext())
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public EnumerationScope(EnumerationContext context)
      : base(context)
    {}
  }
}