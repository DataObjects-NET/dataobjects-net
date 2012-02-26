// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  public sealed class EmptyPreCompiler : IPreCompiler
  {
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      return rootProvider;
    }
  }
}