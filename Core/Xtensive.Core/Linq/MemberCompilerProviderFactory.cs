// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtia
// Created:    2009.03.27

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Xtensive.Linq
{
  [Serializable]
  public class MemberCompilerProviderFactory
  {
    /// <summary>
    /// Creates new instance of <see cref="IMemberCompilerProvider"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    public static IMemberCompilerProvider Create(Type type)
    {
      return (IMemberCompilerProvider) Activator.CreateInstance(typeof(MemberCompilerProvider<>)
             .MakeGenericType(type), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[0], null);
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