// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class Translator : v8_4.Translator
  {
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case byte[] array:
          var builder = output.StringBuilder;
          builder.EnsureCapacity(builder.Length + 2 * (array.Length + 6));
          builder.Append(@"E'\\x");
          builder.AppendHexArray(array);
          builder.Append("'");
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}