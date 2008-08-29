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
    private const string CompiledKey = "Compiled";

    /// <summary>
    /// Gets the compiled provider for this provider.
    /// </summary>
    public Provider Compiled {
      get {
        if (EnumerationContext.Current == null)
          throw new InvalidOperationException(
            Strings.ExCanNotCompileNoEnumerationContext);
        var compiled = EnumerationContext.Current.GetValue<ExecutableProvider>(new Pair<object, string>(this,CompiledKey));
        if (compiled == null) 
          lock (this) 
            if (EnumerationContext.Current.GetValue<ExecutableProvider>(new Pair<object, string>(this,CompiledKey)) == null) {
              if (CompilationScope.CurrentContext == null)
                using (new CompilationContext(new DefaultCompiler()).Activate())
                  compiled = this.Compile();
              else
                compiled = this.Compile();
              EnumerationContext.Current.SetValue(new Pair<object, string>(this,CompiledKey), compiled);
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