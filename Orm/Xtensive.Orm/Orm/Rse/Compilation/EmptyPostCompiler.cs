// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.12

using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  public sealed class EmptyPostCompiler : IPostCompiler
  {
    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      return rootProvider;
    }
  }
}