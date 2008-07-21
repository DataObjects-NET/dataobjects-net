// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default implementation of <see cref="Compiler"/>.
  /// </summary>
  public sealed class DefaultCompiler : Compiler
  {
    /// <inheritdoc/>
    public override bool IsCompatible(Provider provider)
    {
      return (provider is ExecutableProvider);
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(Provider provider)
    {
      throw new NotSupportedException();
    }
  }
}