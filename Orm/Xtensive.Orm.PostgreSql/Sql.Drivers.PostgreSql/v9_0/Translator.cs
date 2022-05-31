// Copyright (C) 2012-2022 Xtensive LLC.
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
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case byte[] array:
          var builder = output.StringBuilder;
          _ = builder.EnsureCapacity(builder.Length + 2 * (array.Length + 6));
          _ = builder.Append(@"E'\\x")
            .AppendHexArray(array)
            .Append('\'');
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