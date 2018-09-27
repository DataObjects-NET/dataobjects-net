// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.09.21

using System.Collections.Generic;
using Xtensive.Sql.Drivers.SqlServer.v10;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Driver : v12.Driver
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