// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// RecordSet provider compilation scope. 
  /// </summary>
  public class CompilationScope : Scope<CompilationContext>
  {
    /// <inheritdoc/>
    public new static CompilationContext CurrentContext
    {
      get { return Scope<CompilationContext>.CurrentContext; }
    }

    /// <inheritdoc/>
    public new CompilationContext Context
    {
      get { return base.Context; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CompilationScope()
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    public CompilationScope(CompilationContext context)
      : base(context)
    {
    }
  }
}