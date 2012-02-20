// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.20

using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlSelectCorrector : IPostCompiler
  {
    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var sqlProvider = rootProvider as SqlProvider;
      if (sqlProvider!=null)
        SqlSelectProcessor.Process(sqlProvider.Request.SelectStatement);
      return rootProvider;
    }
  }
}