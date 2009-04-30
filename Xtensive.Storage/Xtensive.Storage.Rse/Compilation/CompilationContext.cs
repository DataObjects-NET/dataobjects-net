// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="CompilableProvider"/> compilation context.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  public abstract class CompilationContext : Context<CompilationScope>
  {
    private readonly Func<ICompiler> compilerProvider;
    private readonly Func<IPreCompiler> optimizerProvider;

    #region Nested type: CacheEntry

    private class CacheEntry 
    {
      public readonly CompilableProvider Key;
      public readonly ExecutableProvider Value;


      // Constructors
      
      public CacheEntry(CompilableProvider key, ExecutableProvider value)
      {
        Key = key;
        Value = value;
      }
    }

    #endregion

    /// <summary>
    /// Gets the size of compilation cache.
    /// Currently it is 1024 (compilation results).
    /// </summary>
    public readonly static int CacheSize = 1024;

    private readonly ICache<CompilableProvider, CacheEntry> cache;
    private readonly object _lock = new object();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public readonly static DefaultCompilationContext Default = new DefaultCompilationContext();

    /// <summary>
    /// Gets the current compilation context.
    /// </summary>
    public static CompilationContext Current {
      [DebuggerStepThrough]
      get { return CompilationScope.CurrentContext ?? Default; }
    }


    /// <summary>
    /// Compiles the specified provider by passing it to <see cref="Compiler"/>.<see cref="ICompiler.Compile"/> method.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="InvalidOperationException">Can't compile the specified <paramref name="provider"/>.</exception>
    public ExecutableProvider Compile(CompilableProvider provider)
    {
      if (provider == null)
        return null;
      lock (_lock) {
        var entry = cache[provider, true];
        if (entry!=null)
          return entry.Value;
      }

      var optimizer = optimizerProvider();
      var compiler = compilerProvider();
      if (compiler == null)
        throw new InvalidOperationException(
          Strings.ExCanNotCompileNoCompiler);
      
      var optimizedProvider = optimizer.Process(provider);
      var result = compiler.Compile(optimizedProvider);
      
      if (result!=null && result.IsCacheable)
        lock (_lock)
          cache.Add(new CacheEntry(provider, result));
      if (result==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantCompileProviderX, provider));
      return result;
    }

    /// <summary>
    /// Creates the enumeration context suitable 
    /// for compilation results produced by this
    /// <see cref="CompilationContext"/>.
    /// </summary>
    /// <returns>Newly created <see cref="EnumerationContext"/> object.</returns>
    public abstract EnumerationContext CreateEnumerationContext();

    #region IContext<...> members

    /// <inheritdoc/>
    protected override CompilationScope CreateActiveScope()
    {
      return new CompilationScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return CompilationScope.CurrentContext == this; }
    }

    #endregion


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compilerProvider">The compiler provider.</param>
    /// <param name="optimizerProvider">The optimizer provider.</param>
    protected CompilationContext(Func<ICompiler> compilerProvider, Func<IPreCompiler> optimizerProvider)
    {
      this.compilerProvider = compilerProvider;
      this.optimizerProvider = optimizerProvider;
      cache = new LruCache<CompilableProvider, CacheEntry>(CacheSize, i => i.Key,
        new WeakestCache<CompilableProvider, CacheEntry>(false, false, i => i.Key));
    }
  }
}