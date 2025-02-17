// Copyright (C) 2012-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class Driver : v8_4.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper() => new TypeMapper(this);

    protected override SqlCompiler CreateCompiler() => new Compiler(this);

    protected override Model.Extractor CreateExtractor() => new Extractor(this);

    protected override SqlTranslator CreateTranslator() => new Translator(this);

    protected override Info.ServerInfoProvider CreateServerInfoProvider() => new ServerInfoProvider(this);

    // Constructors

    public Driver(CoreServerInfo coreServerInfo, PostgreServerInfo pgServerInfo)
      : base(coreServerInfo, pgServerInfo)
    {
    }
  }
}