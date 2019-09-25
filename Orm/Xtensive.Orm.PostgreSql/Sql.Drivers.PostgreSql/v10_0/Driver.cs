// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Driver : v9_0.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    public Driver(CoreServerInfo coreServerInfo) : base(coreServerInfo)
    {
    }
  }
}
