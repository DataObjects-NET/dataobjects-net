// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling
{
  /// <summary>
  /// <see cref="Node"/> cloning scope.
  /// </summary>
  public class CloningScope : Scope<CloningContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static CloningContext CurrentContext {
      get {
        return Scope<CloningContext>.CurrentContext;
      }
    }
    
    /// <summary>
    /// Gets the associated cloning context.
    /// </summary>
    public new CloningContext Context {
      get {
        return base.Context;
      }
    }
    
    /// <summary>
    /// Opens a cloning context and scope.
    /// </summary>
    /// <returns>A new <see cref="CloningScope"/>, if there is no 
    /// <see cref="CloningContext.Current"/> cloning context;
    /// otherwise, <see langword="null" />.</returns>
    public static CloningScope Open()
    {
      if (CurrentContext==null)
        return new CloningContext().Activate();
      else
        return null;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public CloningScope(CloningContext context)
      : base(context)
    {
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      Context.ApplyFixups();
      base.Dispose(disposing);
    }
  }
}