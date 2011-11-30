// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.27

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Linq
{
  /// <summary>
  /// Base interface for compiler providers.
  /// See <see cref="IMemberCompilerProvider{T}"/>.
  /// </summary>
  public interface IMemberCompilerProvider : ILockable
  {
    /// <summary>
    /// Gets the type of expression.
    /// </summary>
    Type ExpressionType { get; }

    /// <summary>
    /// Gets untyped compiler for <see paramref="target"/>.
    /// </summary>
    /// <param name="target">Member to get compiler for.</param>
    /// <returns>Compiler for <see cref="target"/></returns>
    Delegate GetUntypedCompiler(MemberInfo target);

    /// <summary>
    /// Registers compilers found in specified type.
    /// Conflicts are resolved as <see cref="ConflictHandlingMethod.ReportError"/> were specified.
    /// </summary>
    /// <param name="compilerContainer">Type to search for compiler methods.</param>
    void RegisterCompilers(Type compilerContainer);

    /// <summary>
    /// Registers compilers found in specified type.
    /// In case of multiple compilers exist for the same <see cref="MethodInfo"/>
    /// implementor should use specified <see cref="ConflictHandlingMethod"/>.
    /// </summary>
    /// <param name="compilerContainer">Type to search for compiler methods.</param>
    /// <param name="conflictHandlingMethod">Determines how provider would resolve conflicts.</param>
    void RegisterCompilers(Type compilerContainer, ConflictHandlingMethod conflictHandlingMethod);
  }
}