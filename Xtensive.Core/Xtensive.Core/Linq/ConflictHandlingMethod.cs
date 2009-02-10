// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Enum for specifying how <see cref="IMemberCompilerProvider{T}"/> should handle conflicts
  /// (i.e. multiple compilers for same method)
  /// </summary>
  public enum ConflictHandlingMethod
  {
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should keep existing compiler for specified method.
    /// </summary>
    KeepOld,
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should overwrite existing compiler with newly found compiler.
    /// </summary>
    Overwrite,
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should throw exception if conflict is found.
    /// </summary>
    ReportError
  }
}