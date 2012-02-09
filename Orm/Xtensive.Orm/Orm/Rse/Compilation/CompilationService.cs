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
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Rse.Compilation
{
  /// <summary>
  /// <see cref="CompilableProvider"/> compilation service.
  /// </summary>
  public sealed class CompilationService
  {
    private readonly Func<ICompiler> compilerProvider;
    private readonly Func<IPreCompiler> preCompilerProvider;
    private readonly Func<ICompiler, IPostCompiler> postCompilerProvider;

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

      return result;
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compilerProvider">The compiler provider.</param>
    /// <param name="preCompilerProvider">The pre-compiler provider.</param>
    /// <param name="postCompilerProvider">The post-compiler provider.</param>
    public CompilationService(
      Func<ICompiler> compilerProvider,
      Func<IPreCompiler> preCompilerProvider,
      Func<ICompiler, IPostCompiler> postCompilerProvider)
    {
      this.compilerProvider = compilerProvider;
      this.preCompilerProvider = preCompilerProvider;
      this.postCompilerProvider = postCompilerProvider;
    }
  }
}