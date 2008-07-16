// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers.
  /// Provides <see cref="TypeCompiler"/>s for <see cref="CompilableProvider"/>s.
  /// </summary>
  public abstract class Compiler : AssociateProvider,
    ICompiler
  {
    private readonly object _lock = new object();
    private readonly ThreadSafeDictionary<Type, TypeCompiler> cache = ThreadSafeDictionary<Type, TypeCompiler>.Create();

    /// <summary>
    /// Gets the compiler responsible for compilation of specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Compilable provider to get the compiler for.</param>
    /// <returns>The compiler.</returns>
    protected TypeCompiler GetCompiler(CompilableProvider provider)
    {
      if (provider == null)
        return null;
      Type type = provider.GetType();
      return GetCompiler(type);
    }

    /// <summary>
    /// Gets the compiler responsible for compilation of provider of specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of provider to get the compiler for.</param>
    /// <returns>The compiler.</returns>
    protected TypeCompiler GetCompiler(Type type)
    {
      TypeCompiler result = cache.GetValue(type);
      if (result!=null)
        return result;
      lock (_lock) {
        result = cache.GetValue(type);
        if (result!=null)
          return result;
        MethodInfo innerGetCompiler = GetType()
          .GetMethod("GetCompiler", BindingFlags.Public | BindingFlags.Instance, null, ArrayUtils<Type>.EmptyArray, null);
        innerGetCompiler = innerGetCompiler
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {type});
        result = innerGetCompiler.Invoke(this, null) as TypeCompiler;
        cache.SetValue(type, result);
        return result;
      }
    }

    /// <summary>
    /// Gets the compiler responsible for compilation of provider of specified <typeparamref name="TProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The type of provider to get the compiler for.</typeparam>
    /// <returns>The compiler.</returns>
    protected TypeCompiler<TProvider> GetCompiler<TProvider>() 
      where TProvider : CompilableProvider
    {
      return GetAssociate<TProvider, TypeCompiler<TProvider>, TypeCompiler<TProvider>>();
    }

    /// <summary>
    /// Ensures the specified provider is compiled 
    /// by the compiler provided by this provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider; 
    /// the original <paramref name="provider"/>, if it is already compiled.</returns>
    public Provider Compile(Provider provider)
    {
      if (provider==null)
        return null;
      var cp = provider as CompilableProvider;
      if (cp!=null)
        return Compile(GetCompiler(cp).Compile(cp));
      if (IsCompiled(provider))
        return provider;
      return Wrap(provider);
    }

    /// <summary>
    /// Determines whether the specified provider can be considered as compiled by this compiler.
    /// </summary>
    /// <param name="provider">The provider to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified provider is compiled; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected abstract bool IsCompiled(Provider provider);

    /// <summary>
    /// Wraps the specified provider to a provider that "appears" as the 
    /// result of compilation by this provider (i.e. call of <see cref="IsCompiled"/>
    /// on the result of this method should always return <see langword="true" />).
    /// </summary>
    /// <param name="provider">The provider to wrap.</param>
    /// <returns>Wrapping provider.</returns>
    protected abstract Provider Wrap(Provider provider);

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult)(object) associate;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected Compiler()
    {
      TypeSuffixes = new[] { "Compiler" };
      Type t = GetType();
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}