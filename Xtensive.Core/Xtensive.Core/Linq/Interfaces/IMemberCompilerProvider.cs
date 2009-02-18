// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Reflection;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Interface for compiler providers.
  /// </summary>
  /// <typeparam name="T">Type which compiler should produce.</typeparam>
  public interface IMemberCompilerProvider<T>
  {
    /// <summary>
    /// Finds compiler for specified <see cref="MethodInfo"/>
    /// </summary>
    /// <param name="methodInfo"><see cref="MethodInfo"/> to search compiler for.</param>
    /// <returns>compiler associated with <see cref="MethodInfo"/> or null if compiler is not found.</returns>
    Func<T, T[], T> GetCompiler(MethodInfo methodInfo);

    /// <summary>
    /// Finds compiler for specified <see cref="MethodInfo"/>
    /// </summary>
    /// <param name="methodInfo"><see cref="MethodInfo"/> to search compiler for.</param>
    /// <param name="compilerMethodInfo"><see cref="MethodInfo"/> of method which is called by returned delegate</param>
    /// <returns>compiler associated with <see cref="MethodInfo"/> or null if compiler is not found.</returns>
    Func<T, T[], T> GetCompiler(MethodInfo methodInfo, out MethodInfo compilerMethodInfo);

    /// <summary>
    /// Registers compilers found in specified type.
    /// Conflicts are resolved as <see cref="ConflictHandlingMethod.ReportError"/> were specified.
    /// </summary>
    /// <param name="t">Type to search for compiler methods.</param>
    void RegisterCompilers(Type t);

    /// <summary>
    /// Registers compilers found in specified type.
    /// In case of multiple compilers exist for the same <see cref="MethodInfo"/>
    /// implementor should use specified <see cref="ConflictHandlingMethod"/>.
    /// </summary>
    /// <param name="type">Type to search for compiler methods.</param>
    /// <param name="conflictHandlingMethod">Determines how providers would resolve conflicts.</param>
    void RegisterCompilers(Type type, ConflictHandlingMethod conflictHandlingMethod);
  }
}
