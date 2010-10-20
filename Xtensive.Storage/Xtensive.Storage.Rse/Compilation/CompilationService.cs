// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Caching;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="CompilableProvider"/> compilation service.
  /// </summary>
  public abstract class CompilationService
  {
    /// <summary>
    /// Gets the size of compilation cache.
    /// Currently it is 256 (compilation results).
    /// </summary>
    public const int DefaultCacheSize = 256;
    
    private static Func<CompilationService> resolver;
    private readonly ICache<CompilableProvider, CacheEntry> cache;
    private readonly object _lock = new object();
    private readonly Func<ICompiler> compilerProvider;
    private readonly Func<IPreCompiler> preCompilerProvider;
    private readonly Func<ICompiler, IPostCompiler> postCompilerProvider;

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public readonly static DefaultCompilationService Default = 
      new DefaultCompilationService();

    #region Nested type: CacheEntry

    /// <summary>
    /// Describes RSE compilation cache entry.
    /// </summary>
    protected class CacheEntry 
    {
      /// <summary>
      /// Entry key.
      /// </summary>
      public readonly CompilableProvider Key;

      /// <summary>
      /// Entry value.
      /// </summary>
      public readonly ExecutableProvider Value;


      // Constructors

      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
      /// </summary>
      /// <param name="key">The key.</param>
      /// <param name="value">The value.</param>
      public CacheEntry(CompilableProvider key, ExecutableProvider value)
      {
        Key = key;
        Value = value;
      }
    }

    #endregion


    /// <summary>
    /// Compiles the specified provider by passing it to <see cref="ICompiler"/>.
    /// <see cref="ICompiler.Compile"/> method.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="InvalidOperationException">Can't compile the specified 
    /// <paramref name="provider"/>.</exception>
    public ExecutableProvider Compile(CompilableProvider provider)
    {
      if (provider == null)
        return null;
      lock (_lock) {
        var entry = cache[provider, true];
        if (entry!=null)
          return entry.Value;
      }

      var preCompiler = preCompilerProvider();
      var compiler = compilerProvider();
      var postCompiler = postCompilerProvider(compiler);
      if (compiler == null)
        throw new InvalidOperationException(Strings.ExCanNotCompileNoCompiler);
      
      var preCompiledProvider = preCompiler.Process(provider);
      var result = compiler.Compile(preCompiledProvider);
      result = postCompiler.Process(result);
      
      if (result==null)
        throw new InvalidOperationException(string.Format(Strings.ExCantCompileProviderX, provider));

      if (result.IsCacheable)
        lock (_lock)
          cache.Add(new CacheEntry(provider, result));

      return result;
    }

    /// <summary>
    /// Creates RSE compilation cache.
    /// </summary>
    /// <param name="cacheSize">Size of the cache.</param>
    /// <returns>RSE compilation cache.</returns>
    protected virtual ICache<CompilableProvider, CacheEntry> CreateCache(int cacheSize)
    {
      return 
        new LruCache<CompilableProvider, CacheEntry>(cacheSize, i => i.Key,
          new WeakestCache<CompilableProvider, CacheEntry>(false, false, i => i.Key));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compilerProvider">The compiler provider.</param>
    /// <param name="preCompilerProvider">The pre-compiler provider.</param>
    /// <param name="postCompilerProvider">The post-compiler provider.</param>
    protected CompilationService(
      Func<ICompiler> compilerProvider,
      Func<IPreCompiler> preCompilerProvider,
      Func<ICompiler, IPostCompiler> postCompilerProvider)
      : this(compilerProvider, preCompilerProvider, postCompilerProvider, DefaultCacheSize)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compilerProvider">The compiler provider.</param>
    /// <param name="preCompilerProvider">The pre-compiler provider.</param>
    /// <param name="postCompilerProvider">The post-compiler provider.</param>
    /// <param name="cacheSize">Size of the cache.</param>
    protected CompilationService(
      Func<ICompiler> compilerProvider,
      Func<IPreCompiler> preCompilerProvider,
      Func<ICompiler, IPostCompiler> postCompilerProvider,
      int cacheSize)
    {
      this.compilerProvider = compilerProvider;
      this.preCompilerProvider = preCompilerProvider;
      this.postCompilerProvider = postCompilerProvider;
      cache = CreateCache(cacheSize);
    }
  }
}