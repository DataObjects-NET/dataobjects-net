// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_2
{
  internal class Translator : v8_1.Translator
  {
    protected override void InitFunctionTypeTranslations()
    {
      base.InitFunctionTypeTranslations();

      FunctionTypeTranslations.AddOrOverride(SqlFunctionType.CurrentDate, "date_trunc('day', clock_timestamp())");
      FunctionTypeTranslations.AddOrOverride(SqlFunctionType.CurrentTimeStamp, "clock_timestamp()");
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string QuoteString(string str) =>
      "E'" + str.Replace("'", "''").Replace(@"\", @"\\").Replace("\0", string.Empty) + "'";

    /// <inheritdoc/>
    public override void TranslateString(IOutput output, string str)
    {
      _ = output.Append('E');
      base.TranslateString(output, str);
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType type)
    {
      switch(type) {
        //date
        case SqlFunctionType.CurrentDate: _ = output.Append("date_trunc('day', clock_timestamp())"); break;
        case SqlFunctionType.CurrentTimeStamp: _ = output.Append("clock_timestamp()"); break;
        default: base.Translate(output, type); break;
      };
    }

    protected override void AppendIndexStorageParameters(IOutput output, Index index)
    {
      if (index.FillFactor != null) {
        _ = output.Append($"WITH(FILLFACTOR={index.FillFactor})");
      }
    }


    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}