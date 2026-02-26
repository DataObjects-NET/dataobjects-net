// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtia
// Created:    2009.03.27

using System;
using Xtensive.Core;
using Xtensive.Reflection;
using Activator = System.Activator;

namespace Xtensive.Orm.Linq.MemberCompilation
{
  /// <summary>
  /// Factory for <see cref="IMemberCompilerProvider" /> implementation.
  /// </summary>
  public static class MemberCompilerProviderFactory
  {
    /// <summary>
    /// Creates new instance of <see cref="IMemberCompilerProvider"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    public static IMemberCompilerProvider Create(Type type)
    {
      ArgumentNullException.ThrowIfNull(type, "type");
      var concreteType = typeof (MemberCompilerProvider<>).CachedMakeGenericType(type);
      return (IMemberCompilerProvider) Activator.CreateInstance(concreteType);
    }

    /// <summary>
    /// Creates new instance of <see cref="IMemberCompilerProvider{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    public static IMemberCompilerProvider<T> Create<T>()
    {
      return new MemberCompilerProvider<T>();
    }
  }
}