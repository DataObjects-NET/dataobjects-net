// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class Translator : v8_4.Translator
  {
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      var array = literalValue as byte[];
      if (array == null) {
        return base.Translate(context, literalValue);
      }

      var result = new StringBuilder((array.Length * 2) + 6)
        .Append(@"E'\\x")
        .AppendHexArray(array)
        .Append("'");

      return result.ToString();
    }

    public override string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          return (node.Ascending) ? "ASC NULLS FIRST" : "DESC NULLS LAST";
      }
      return string.Empty;
    }

    public override string TranslateSortOrder(bool ascending) => ascending ? "ASC NULLS FIRST" : "DESC NULLS LAST";

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}