// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Driver : v10.Driver
  {
    protected override Sql.Compiler.SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Sql.Compiler.SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser)
      : base(coreServerInfo, errorMessageParser)
    {
    }
  }
}