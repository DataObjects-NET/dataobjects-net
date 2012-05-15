// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.20

using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  public class SqlSelectCorrector : IPostCompiler
  {
    private readonly ProviderInfo providerInfo;

    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var sqlProvider = rootProvider as SqlProvider;
      if (sqlProvider!=null)
        SqlSelectProcessor.Process(sqlProvider.Request.Statement, providerInfo);
      return rootProvider;
    }

    // Constructors
    
    public SqlSelectCorrector(ProviderInfo providerInfo)
    {
      this.providerInfo = providerInfo;
    }
  }
}