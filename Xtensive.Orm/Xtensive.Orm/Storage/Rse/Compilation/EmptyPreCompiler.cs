// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Empty <see cref="IPreCompiler"/> implementation.
  /// </summary>
  public sealed class EmptyPreCompiler : IPreCompiler
  {
    /// <inheritdoc/>
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      return rootProvider;
    }
  }
}