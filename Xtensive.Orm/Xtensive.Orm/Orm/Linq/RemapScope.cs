// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.03

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class RemapScope : Scope<RemapContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static RemapContext CurrentContext
    {
      get { return Scope<RemapContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new RemapContext Context
    {
      get { return base.Context; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public RemapScope(RemapContext context)
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RemapScope()
      :base(new RemapContext())
    {
      
    }
  }
}