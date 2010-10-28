// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.12

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Provider's tree post-compiler contract.
  /// </summary>
  public interface IPostCompiler
  {
    /// <summary>
    /// Processes the specified provider's tree.
    /// </summary>
    /// <param name="rootProvider">The root provider.</param>
    ExecutableProvider Process(ExecutableProvider rootProvider);
  }
}