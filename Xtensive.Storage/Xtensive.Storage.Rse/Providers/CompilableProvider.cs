// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>,
  /// that can be compiled.
  /// </summary>
  [Serializable]
  public abstract class CompilableProvider : Provider
  {
    #region Cached properties

    private const string CachedCompiledName = "CachedCompiled";

    private ExecutableProvider CachedCompiled
    {
      get { return EnumerationContext.Current.GetValue<ExecutableProvider>(new Pair<object, string>(this, CachedCompiledName)); }
      set { EnumerationContext.Current.SetValue(new Pair<object, string>(this, CachedCompiledName), value); }
    }

    #endregion

    /// <summary>
    /// Gets the compiled provider for this provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is no active <see cref="EnumerationContext"/> or <see cref="CompilationContext"/>.</exception>
    public Provider Compiled {
      get {
        if (EnumerationContext.Current == null)
          throw new InvalidOperationException(
            Strings.ExCanNotCompileNoEnumerationContext);
        var compiled = CachedCompiled;
        if (compiled==null) lock (this) if (CachedCompiled==null) {
          CachedCompiled = compiled = this.Compile();
        }
        return compiled;
      }
    }

    /// <summary>
    /// Creates the <see cref="RecordSet"/> wrapping this provider.
    /// </summary>
    public RecordSet Result
    {
      get { return new RecordSet(this); }
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

   
    // Constructor

    /// <inheritdoc/>
    protected CompilableProvider(params Provider[] sources)
      : base(sources)
    {
    }
  }
}