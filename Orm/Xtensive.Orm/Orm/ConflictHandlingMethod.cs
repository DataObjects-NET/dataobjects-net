// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

namespace Xtensive.Orm
{
  /// <summary>
  /// Possible ways of handling conflicts (i.e. multiple compilers for same method)
  /// for <see cref="IMemberCompilerProvider{T}"/> 
  /// </summary>
  public enum ConflictHandlingMethod
  {
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should throw exception if conflict is found.
    /// </summary>
    ReportError = 0,
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should keep existing compiler for specified method.
    /// </summary>
    KeepOld = 1,
    /// <summary>
    /// <see cref="IMemberCompilerProvider{T}"/> should overwrite existing compiler with newly found compiler.
    /// </summary>
    Overwrite = 2,
    /// <summary>
    /// Default action for <see cref="IMemberCompilerProvider{T}"/> if conflict is found.
    /// </summary>
    Default = ReportError
  }
}