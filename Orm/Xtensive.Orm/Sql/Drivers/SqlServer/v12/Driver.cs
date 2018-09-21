// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.09.21

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v12
{
  internal class Driver : v11.Driver
  {
    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override SqlTranslator CreateTranslator()
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

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser messageParser)
      : base(coreServerInfo, messageParser)
    {
    }
  }
}
