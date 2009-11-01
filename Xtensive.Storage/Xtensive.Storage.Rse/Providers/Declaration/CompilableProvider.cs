// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Compilation.DefaultCompilers;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public abstract class CompilableProvider : Provider
  {
    private Provider compiled;
    private CompilationContext context;

    public RecordSet Result
    {
      get
      {
        return new RecordSet(this);
      }
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      EnsureIsCompiled();
      return compiled.GetService<T>();
    }

    private void EnsureIsCompiled()
    {
      if (compiled == null) lock (this) if (compiled == null) {
        context = CompilationScope.CurrentContext;
        if (context == null) {
          using (new CompilationContext(new[]{new DefaultCompilerResolver()}).Activate()) {
            context = CompilationScope.CurrentContext;
            compiled = context.Compile(this);
          }
        }
        else
          compiled = context.Compile(this);
      }
      if (!context.IsActive) {
        var currentContext = CompilationScope.CurrentContext;
        if (currentContext != null)
          context = currentContext;
        using (context.Activate())
          compiled = context.Compile(this);
      }
    }

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      EnsureIsCompiled();
      return compiled.GetEnumerator();
    }


    // Constructor

    protected CompilableProvider(params CompilableProvider[] sourceProviders)
      : base(sourceProviders)
    {
    }
  }
}