// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Orm.Rse.Compilation;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// <see cref="EnumerationContext"/> activation scope.
  /// </summary>
  public class EnumerationScope : InheritableScope<EnumerationContext, EnumerationScope>
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public EnumerationScope(EnumerationContext context)
      : base(context)
    {
    }
  }
}