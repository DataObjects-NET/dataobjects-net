// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Driver : SqlServer.Driver
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

    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser, bool checkConnectionIsAlive)
      : base(coreServerInfo, errorMessageParser, checkConnectionIsAlive)
    {
    }
  }
}