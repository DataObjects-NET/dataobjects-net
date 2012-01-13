// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.27

using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  /// <summary>
  /// Provider's tree pre-compiler contract.
  /// </summary>
  public interface IPreCompiler
  {
    /// <summary>
    /// Processes the specified provider's tree.
    /// </summary>
    /// <param name="rootProvider">The root provider.</param>
    CompilableProvider Process(CompilableProvider rootProvider);
  }
}