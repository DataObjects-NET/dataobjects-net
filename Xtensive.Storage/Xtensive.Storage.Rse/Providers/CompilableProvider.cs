// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>,
  /// that can be compiled.
  /// </summary>
  [Serializable]
  public abstract class CompilableProvider : Provider
  {
    private Provider compiled;
    private CompilationContext context;

    /// <summary>
    /// Gets the compiled provider for this provider.
    /// </summary>
    public Provider Compiled {
      get {
        EnsureIsCompiled();
        return compiled;
      }
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Compiled.GetService<T>();
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public override IEnumerator<Tuple> GetEnumerator()
    {
      return Compiled.GetEnumerator();
    }

    #endregion

    #region Private \ internal methods

    private void EnsureIsCompiled()
    {
      if (compiled == null) lock (this) if (compiled == null) {
        context = CompilationScope.CurrentContext;
        if (context == null) {
          using (new CompilationContext(new [] {new DefaultCompiler()}).Activate()) {
            context = CompilationScope.CurrentContext;
            do {
              compiled = context.Compile(this);
            } while (compiled is CompilableProvider);
          }
        }
        else
          do {
            compiled = context.Compile(this);
          } while (compiled is CompilableProvider);
      }
    }

    #endregion


    // Constructor

    /// <inheritdoc/>
    protected CompilableProvider(params Provider[] sources)
      : base(sources)
    {
    }
  }
}