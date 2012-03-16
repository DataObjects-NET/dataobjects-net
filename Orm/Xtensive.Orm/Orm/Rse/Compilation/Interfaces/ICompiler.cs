// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.08.31

using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse.Compilation
{
  /// <summary>
  /// Provider compiler contract.
  /// </summary>
  public interface ICompiler
  {
    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider, if compiler can handle the compilation of specified provider;
    /// otherwise, <see langword="null"/>.</returns>
    ExecutableProvider Compile(CompilableProvider provider);
  }
}