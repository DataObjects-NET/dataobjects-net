// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

namespace Xtensive.Storage.Providers.Sql.Servers.Oracle
{
  internal class SqlCompiler : ManualPagingSqlCompiler
  {
    protected override string ProcessAliasedName(string name)
    {
      return Handlers.NameBuilder.ApplyNamingRules(name);
    }

    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}