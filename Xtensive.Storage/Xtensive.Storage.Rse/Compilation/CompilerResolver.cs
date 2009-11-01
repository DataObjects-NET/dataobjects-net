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
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Resolves compiler for specified <see cref="CompilableProvider"/>.
  /// </summary>
  public abstract class CompilerResolver : AssociateProvider
  {
    private readonly object _lock = new object();
    private readonly ThreadSafeDictionary<Type, ProviderCompiler> cache = ThreadSafeDictionary<Type, ProviderCompiler>.Create();

    /// <summary>
    /// Gets a <see cref="ProviderCompiler"/> for specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Compilable provider.</param>
    public virtual ProviderCompiler GetCompiler(CompilableProvider provider)
    {
      if (provider == null)
        return null;
      Type type = provider.GetType();
      ProviderCompiler result = cache.GetValue(type);
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
        result = innerGetCompiler.Invoke(this, null) as ProviderCompiler;
        cache.SetValue(type, result);
        return result;
      }
    }

    /// <summary>
    /// Gets a <see cref="ProviderCompiler{TProvider}"/> for specified <typeparamref name="TProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">Type of compilable provider.</typeparam>
    public ProviderCompiler<TProvider> GetCompiler<TProvider>() where TProvider : CompilableProvider
    {
      return GetAssociate<TProvider, ProviderCompiler<TProvider>, ProviderCompiler<TProvider>>();
    }

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
    protected CompilerResolver()
    {
      TypeSuffixes = new[] { "Compiler" };
      ConstructorParams = ArrayUtils<object>.EmptyArray;
      Type t = GetType();
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}