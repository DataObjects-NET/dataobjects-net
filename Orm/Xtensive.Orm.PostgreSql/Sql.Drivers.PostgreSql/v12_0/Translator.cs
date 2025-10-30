// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;

namespace Xtensive.Sql.Drivers.PostgreSql.v12_0
{
  internal class Translator : v10_0.Translator
  {
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      if (!index.IsFullText) {
        var output = context.Output;
        switch (section) {
          case CreateIndexSection.NonkeyColumnsEnter:
            _ = output.AppendOpeningPunctuation("INCLUDE (");
            break;
          case CreateIndexSection.NonkeyColumnsExit:
            _ = output.AppendClosingPunctuation(")");
            break;
          default:
            base.Translate(context, node, section);
            break;
        }
      }
      else {
        base.Translate(context, node, section);
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}