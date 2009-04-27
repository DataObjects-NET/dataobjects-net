// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using Xtensive.Core;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Internals
{
  internal class CompiledQueryScope : Scope<ResultExpression>
  {
    private ResultExpression context;

    /// <summary>
    /// Gets the context.
    /// </summary>
    public static CompiledQueryScope Current
    {
      get { return (CompiledQueryScope)CurrentScope; }
    }

    public new ResultExpression Context
    {
      get { return context; }
      set
      {
        if (context != null)
          throw Exceptions.AlreadyInitialized("Context");
        context = value;
      }
    }


    // Constructors

    public CompiledQueryScope()
      : base (null)
    {
    }
  }
}