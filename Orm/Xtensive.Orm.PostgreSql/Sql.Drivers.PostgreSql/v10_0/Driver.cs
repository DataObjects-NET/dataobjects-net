// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Driver : v9_0.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper() => new TypeMapper(this);

    protected override SqlCompiler CreateCompiler() => new Compiler(this);

    protected override Model.Extractor CreateExtractor() => new Extractor(this);

    protected override SqlTranslator CreateTranslator() => new Translator(this);

    protected override Info.ServerInfoProvider CreateServerInfoProvider() => new ServerInfoProvider(this);

    public Driver(CoreServerInfo coreServerInfo) : base(coreServerInfo)
    {
    }
  }
}
