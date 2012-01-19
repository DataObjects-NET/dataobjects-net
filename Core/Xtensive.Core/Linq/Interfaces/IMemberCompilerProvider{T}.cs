// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Collections.Generic;
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
    /// Registers compilers for the corresponding members.
    /// Conflicts are resolved as <see cref="ConflictHandlingMethod.ReportError"/> were specified.
    /// </summary>
    /// <param name="compilerDefinitions">Compiler definitions,
    /// <see cref="KeyValuePair{TKey,TValue}.Key"/> is a target member,
    /// <see cref="KeyValuePair{TKey,TValue}.Value"/> is a corresponding compiler.</param>
    void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions);

    /// <summary>
    /// Registers compilers for the corresponding members.
    /// In case of multiple compilers exist for the same <see cref="MethodInfo"/>
    /// implementor should use specified <see cref="ConflictHandlingMethod"/>.
    /// </summary>
    /// <param name="compilerDefinitions">Compiler definitions,
    /// <see cref="KeyValuePair{TKey,TValue}.Key"/> is a target member,
    /// <see cref="KeyValuePair{TKey,TValue}.Value"/> is a corresponding compiler.</param>
    /// <param name="conflictHandlingMethod">Conflict handling method.</param>
    void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions, ConflictHandlingMethod conflictHandlingMethod);

    /// <summary>
    /// Finds compiler for specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="target"><see cref="MemberInfo"/> to search compiler for.</param>
    /// <returns>compiler associated with <see cref="MethodInfo"/>
    /// or <see langword="null"/> if compiler is not found.</returns>
    Func<T, T[], T> GetCompiler(MemberInfo target);
  }
}
