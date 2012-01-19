// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_2
{
  internal class Translator : v8_1.Translator
  {
    [DebuggerStepThrough]
    public override string QuoteString(string str)
    {
      return "E'" + str.Replace("'", "''").Replace(@"\", @"\\").Replace("\0", string.Empty) + "'";
    }

    protected override void AppendIndexStorageParameters(StringBuilder builder, Index index)
    {
      if (index.FillFactor!=null)
        builder.AppendFormat("WITH(FILLFACTOR={0})", index.FillFactor);
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
        //date
        case SqlFunctionType.CurrentDate:
          return "date_trunc('day', clock_timestamp())";
        case SqlFunctionType.CurrentTimeStamp:
          return "clock_timestamp()";
        default:
          return base.Translate(type);
      }
    }
    
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}