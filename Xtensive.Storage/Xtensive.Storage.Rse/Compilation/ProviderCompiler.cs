// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation
{
  public abstract class ProviderCompiler
  {
    public abstract Provider Compile(CompilableProvider provider);
  }
}