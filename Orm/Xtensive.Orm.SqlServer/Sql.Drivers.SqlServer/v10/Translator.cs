// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class Translator : v09.Translator
  {
    /// <inheritdoc/>
    public override string DateTimeFormatString => @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fffffff\'' as datetime2)'";

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override string TimeOnlyFormatString => @"'cast ('\'HH\:mm\:ss\.fffffff\'' as time)'";
#endif

    public string DateTimeOffsetFormatString => @"'cast ('\'yyyy\-MM\-dd HH\:mm\:ss\.fffffff\ zzz\'' as datetimeoffset)'";

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Ddl.SqlCreateIndex node, CreateIndexSection section)
    {
      switch (section) {
        case CreateIndexSection.ColumnsExit when node.Index.IsSpatial:
          var table = node.Index.DataTable as Table;
          var column = table.TableColumns[node.Index.Columns[0].Name];
          _ = context.Output.Append(column.DataType.Type == CustomSqlType.Geometry
            ? ") USING GEOMETRY_GRID WITH ( BOUNDING_BOX = ( 0, 0, 500, 200))"
            : ") USING GEOGRAPHY_GRID"
          );
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType functionType)
    {
      if (functionType == SqlFunctionType.CurrentDateTimeOffset) {
        _ = output.Append("SYSDATETIMEOFFSET");
      }
      else {
        base.Translate(output, functionType);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      switch (literalValue) {
        case DateTimeOffset dateTimeOffset:
          var dateTimeOffsetRange = (ValueRange<DateTimeOffset>) Driver.ServerInfo.DataTypes.DateTimeOffset.ValueRange;
          var newValue = ValueRangeValidator.Correct(dateTimeOffset, dateTimeOffsetRange);
          _ = context.Output.Append(newValue.ToString(DateTimeOffsetFormatString));
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