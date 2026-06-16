using System;
// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;

namespace Xtensive.Sql.Drivers.PostgreSql.v12_0
{
  internal class Translator : v10_0.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      if (!index.IsFullText) {
        switch (section) {
          case CreateIndexSection.NonkeyColumnsEnter:
            return "INCLUDE (";
          case CreateIndexSection.NonkeyColumnsExit:
            return")";
          default:
            return base.Translate(context, node, section);
        }
      }
      else {
        return base.Translate(context, node, section);
      }
    }


    // Constructors

    public Translator(PostgreSql.Driver driver)
      : base(driver)
    {
    }
  }
}
