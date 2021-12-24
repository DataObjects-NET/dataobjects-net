// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Info;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal class Driver : v2_5.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper() => new TypeMapper(this);

    protected override SqlCompiler CreateCompiler() => new Compiler(this);

    protected override SqlTranslator CreateTranslator() => new Translator(this);

    protected override Model.Extractor CreateExtractor() => new Extractor(this);

    protected override Info.ServerInfoProvider CreateServerInfoProvider() => new ServerInfoProvider(this);

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}