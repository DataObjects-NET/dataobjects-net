// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class FilterProviderCompiler : TypeCompiler<FilterProvider>
  {
    protected override ExecutableProvider Compile(FilterProvider provider)
    {
      return null;
      FilterVisitor visitor = new FilterVisitor();
      var expression = visitor.Visit(provider.Predicate);
      throw new System.NotImplementedException();
    }


    // Constructor

    public FilterProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}