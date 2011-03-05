// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Reflection;

namespace Xtensive.Linq
{
  /// <summary>
  /// Interface for compiler providers.
  /// </summary>
  /// <typeparam name="T">Type which compiler should produce.</typeparam>
  public interface IMemberCompilerProvider<T> : IMemberCompilerProvider
  {
    /// <summary>
    /// Finds compiler for specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="source"><see cref="MemberInfo"/> to search compiler for.</param>
    /// <returns>compiler associated with <see cref="MethodInfo"/>
    /// or <see langword="null"/> if compiler is not found.</returns>
    Func<T, T[], T> GetCompiler(MemberInfo source);

    /// <summary>
    /// Finds compiler for specified <see cref="MemberInfo"/>
    /// </summary>
    /// <param name="source"><see cref="MemberInfo"/> to search compiler for.</param>
    /// <param name="compilerMethod"><see cref="MethodInfo"/> of method which is called by returned delegate</param>
    /// <returns>compiler associated with <see cref="MethodInfo"/>
    /// or <see langword="null"/> if compiler is not found.</returns>
    Func<T, T[], T> GetCompiler(MemberInfo source, out MethodInfo compilerMethod);
  }
}
