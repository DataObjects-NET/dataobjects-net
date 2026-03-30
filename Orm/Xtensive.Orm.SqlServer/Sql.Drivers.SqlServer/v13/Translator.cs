// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.SqlServer.Resources;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Translator : SqlTranslator
  {
    /// <inheritdoc/>
    public override string DateTimeFormatString => @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fffffff\'' as datetime2)'";

    /// <inheritdoc/>
    public override string DateOnlyFormatString => @"'cast ('\'yyyy\-MM\-dd\'' as date)'";

    /// <inheritdoc/>
    public override string TimeOnlyFormatString => @"'cast ('\'HH\:mm\:ss\.fffffff\'' as time)'";

    /// <inheritdoc/>
    public override string DateTimeOffsetFormatString => @"'cast ('\'yyyy\-MM\-dd HH\:mm\:ss\.fffffff\ zzz\'' as datetimeoffset)'";

    /// <inheritdoc/>
    public override string TimeSpanFormatString => string.Empty;

    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberDecimalSeparator = ".";
    }

    #region Translate methods for nodes and expressions that write to output directly

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.ArgumentEntry:
          break;
        case FunctionCallSection.ArgumentDelimiter:
          _ = context.Output.Append(ArgumentDelimiter);
          break;
        default:
          base.Translate(context, node, section, position);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case TableColumnSection.Type:
          if (column.Domain is null) {
            _ = output.Append(Translate(column.DataType));
          }
          else {
            TranslateIdentifier(output, column.Domain.Schema.DbName, column.Domain.DbName);
          }
          break;
        case TableColumnSection.GenerationExpressionEntry:
          _ = output.Append("AS (");
          break;
        case TableColumnSection.GeneratedEntry:
        case TableColumnSection.GeneratedExit:
        case TableColumnSection.SetIdentityInfoElement:
        case TableColumnSection.Exit:
          break;
        default:
          base.Translate(context, column, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.AddColumn:
          _ = context.Output.Append("ADD");
          break;
        case AlterTableSection.DropBehavior:
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (descriptor.Owner) {
        case Sequence _:
          TranslateSequenceDescriptorDefault(context, descriptor, section);
          break;
        default:
          throw new NotSupportedException();
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      var output = context.Output;
      switch (section) {
        case ConstraintSection.Unique:
          _ = Driver.ServerInfo.UniqueConstraint.Features.Supports(UniqueConstraintFeatures.Clustered)
            ? output.Append(((UniqueConstraint) constraint).IsClustered ? "UNIQUE CLUSTERED (" : "UNIQUE NONCLUSTERED (")
            : output.Append("UNIQUE (");
          break;
        case ConstraintSection.PrimaryKey:
          _ = Driver.ServerInfo.PrimaryKey.Features.Supports(PrimaryKeyConstraintFeatures.Clustered)
            ? output.Append(((PrimaryKey) constraint).IsClustered ? "PRIMARY KEY CLUSTERED (" : "PRIMARY KEY NONCLUSTERED (")
            : output.Append("PRIMARY KEY (");
          break;
        case ConstraintSection.Exit:
          if (constraint is ForeignKey fk) {
            _ = output.Append(")");
            if (fk.OnUpdate != ReferentialAction.Restrict && fk.OnUpdate != ReferentialAction.NoAction) {
              _ = output.Append(" ON UPDATE ");
              Translate(output, fk.OnUpdate);
            }
            if (fk.OnDelete != ReferentialAction.Restrict && fk.OnDelete != ReferentialAction.NoAction) {
              _ = output.Append(" ON DELETE ");
              Translate(output, fk.OnDelete);
            }
          }
          else {
            _ = output.Append(")");
          }
          break;
        default:
          base.Translate(context, constraint, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateTableSection.Entry:
          _ = output.Append("CREATE ");
          if (node.Table is TemporaryTable temporaryTable) {
            temporaryTable.DbName = temporaryTable.IsGlobal
              ? "##" + temporaryTable.Name
              : "#" + temporaryTable.Name;
          }
          _ = output.Append("TABLE ");
          Translate(context, node.Table);
          return;
        case CreateTableSection.Exit:
          if (!string.IsNullOrEmpty(node.Table.Filegroup)) {
            _ = output.Append(" ON ");
            TranslateIdentifier(output, node.Table.Filegroup);
          }
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          if (node.View.CheckOptions == CheckOptions.Cascaded) {
            _ = context.Output.Append("WITH CHECK OPTION");
          }
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateDomain node, CreateDomainSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateDomainSection.Entry:
          _ = output.Append("CREATE TYPE ");
          Translate(context, node.Domain);
          _ = output.Append(" FROM ")
            .Append(Translate(node.Domain.DataType));
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropDomain node)
    {
      _ = context.Output.Append("DROP TYPE ");
      Translate(context, node.Domain);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterDomain node, AlterDomainSection section)
    {
      throw SqlHelper.NotSupported("ALTER DOMAIN"); // NOTE: Do not localize, it's an SQL keyword
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      if (section == DeclareCursorSection.Holdability || section == DeclareCursorSection.Returnability) {
        return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      var output = context.Output;
      switch (section) {
        case JoinSection.Specification:
          if (node.Expression is null)
            switch (node.JoinType) {
              case SqlJoinType.InnerJoin:
              case SqlJoinType.LeftOuterJoin:
              case SqlJoinType.RightOuterJoin:
              case SqlJoinType.FullOuterJoin:
                throw new NotSupportedException();
              case SqlJoinType.CrossApply:
                _ = output.Append("CROSS APPLY");
                return;
              case SqlJoinType.LeftOuterApply:
                _ = output.Append("OUTER APPLY");
                return;
            }
          var joinHint = TryFindJoinHint(context, node);

          Translate(output, node.JoinType);
          if (joinHint is not null) {
            _ = output.AppendSpace().Append(Translate(joinHint.Method));
          }
          _ = output.Append(" JOIN");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }

      static SqlJoinHint TryFindJoinHint(SqlCompilerContext context, SqlJoinExpression node)
      {
        SqlQueryStatement statement = null;
        for (int i = 0, count = context.GetTraversalPath().Length; i < count; i++) {
          if (context.GetTraversalPath()[i] is SqlQueryStatement) {
            statement = context.GetTraversalPath()[i] as SqlQueryStatement;
          }
        }
        if (statement is null || statement.Hints.Count == 0) {
          return null;
        }

        var candidate = statement.Hints
          .OfType<SqlJoinHint>()
          .FirstOrDefault(hint => hint.Table == node.Right);
        return candidate;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      if (node.All && section == QueryExpressionSection.All && (node.NodeType == SqlNodeType.Except || node.NodeType == SqlNodeType.Intersect))
        return;
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      var output = context.Output;
      switch (section) {
        case SelectSection.Entry:
          base.Translate(context, node, section);
          break;
        case SelectSection.Limit:
          _ = output.Append("FETCH NEXT");
          break;
        case SelectSection.LimitEnd:
          _ = output.Append("ROWS ONLY");
          break;
        case SelectSection.Offset:
          _ = output.Append("OFFSET");
          break;
        case SelectSection.OffsetEnd:
          _ = output.Append("ROWS");
          break;
        case SelectSection.Exit:
          var hasHints = false;
          foreach (var hint in node.Hints) {
            switch (hint) {
              case SqlForceJoinOrderHint:
                AppendHint(output, "FORCE ORDER", ref hasHints);
                break;
              case SqlFastFirstRowsHint sqlFastFirstRowsHint:
                AppendHint(output, "FAST ", ref hasHints);
                _ = output.Append(sqlFastFirstRowsHint.Amount);
                break;
              case SqlNativeHint sqlNativeHint:
                AppendHint(output, sqlNativeHint.HintText, ref hasHints);
                break;
            }
          }
          if (hasHints) {
            _ = output.Append(")");
          }
          break;
        default:
          base.Translate(context, node, section);
          break;
      }

      static void AppendHint(IOutput output, string hint, ref bool hasHints)
      {
        if (hasHints) {
          _ = output.Append(", ");
        }
        else {
          _ = output.Append(" OPTION (");
          hasHints = true;
        }
        _ = output.Append(hint);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      switch (section) {
        case UpdateSection.Limit:
          _ = context.Output.Append("TOP");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      switch (section) {
        case DeleteSection.Entry:
          _ = context.Output.Append("DELETE");
          break;
        case DeleteSection.Limit:
          _ = context.Output.Append("TOP");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlRenameTable node)
    {
      TranslateExecSpRename(context, node.Table, null, node.NewName);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      if (section == CreateIndexSection.ColumnsExit && node.Index.IsSpatial) {
        var table = node.Index.DataTable as Table;
        var column = table.TableColumns[node.Index.Columns[0].Name];
        _ = context.Output.Append(column.DataType.Type == CustomSqlType.Geometry
          ? ") USING GEOMETRY_GRID WITH ( BOUNDING_BOX = ( 0, 0, 500, 200))"
          : ") USING GEOGRAPHY_GRID"
        );
        return;
      }

      base.Translate(context, node, section);
      if (section != CreateIndexSection.Exit) {
        return;
      }
      var index = node.Index;
      if (index is not FullTextIndex ftIndex) {
        return;
      }
      if (ftIndex.FullTextCatalog != null) {
        _ = context.Output.Append(" ON ");
        TranslateIdentifier(context.Output, ftIndex.FullTextCatalog);
      }
      _ = context.Output.Append(" WITH CHANGE_TRACKING ")
        .Append(TranslateChangeTrackingMode(ftIndex.ChangeTrackingMode));
    }

    /// <inheritdoc/>
    public virtual void Translate(SqlCompilerContext context, SqlRenameColumn action)
    {
      var table = action.Column.Table;
      TranslateExecSpRename(context, table, action.Column, action.NewName);
    }

    /// <inheritdoc/>
    public virtual void Translate(SqlCompilerContext context, SqlAlterTable node, DefaultConstraint constraint)
    {
      TranslateExecDropDefaultConstraint(context, node, constraint);
    }

    protected void TranslateExecDropDefaultConstraint(SqlCompilerContext context, SqlAlterTable node, DefaultConstraint defaultConstraint)
    {
      var column = defaultConstraint.Column;
      var table = defaultConstraint.Table;
      var schema = defaultConstraint.Column.DataTable.Schema;

      var sqlVariableName = RemoveFromStringInvalidCharacters($"var_{schema.DbName}_{table.DbName}_{column.DbName}");

      var gettingNameOfDefaultConstraintScript = GetCurentNameOfDefaultConstraintScript(sqlVariableName,
        schema.Catalog.DbName, schema.DbName, table.DbName, column.DbName);

      _ = context.Output.Append(gettingNameOfDefaultConstraintScript)
        .Append(" ")
        .Append("Exec(N'");
      Translate(context, node, AlterTableSection.Entry);
      Translate(context, node, AlterTableSection.DropConstraint);
      _ = context.Output.Append(" CONSTRAINT[' + @")
        .Append(sqlVariableName)
        .Append(" + N']')");
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      switch (section) {
        case ExtractSection.Entry:
          _ = context.Output.Append("DATEPART(");
          break;
        case ExtractSection.From:
          _ = context.Output.Append(ArgumentDelimiter);
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      base.Translate(context, node, section);
      if (section != TableSection.AliasDeclaration) {
        return;
      }

      var select = context.GetTraversalPath()
        .OfType<SqlSelect>()
        .Where(s => s.Lock != SqlLockType.Empty)
        .FirstOrDefault();
      if (select is not null) {
        _ = context.Output.Append(" WITH (");
        Translate(context.Output, select.Lock);
        _ = context.Output.Append(")");
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      var output = context.Output;
      _ = section switch {
        TrimSection.Entry => node.TrimType switch {
          SqlTrimType.Leading => output.AppendOpeningPunctuation("LTRIM("),
          SqlTrimType.Trailing => output.AppendOpeningPunctuation("RTRIM("),
          SqlTrimType.Both => output.AppendOpeningPunctuation("LTRIM(RTRIM("),
          _ => throw new ArgumentOutOfRangeException("node.TrimType"),
        },
        TrimSection.Exit => node.TrimType switch {
          SqlTrimType.Leading or SqlTrimType.Trailing => output.Append(")"),
          SqlTrimType.Both => output.Append("))"),
          _ => throw new ArgumentOutOfRangeException("node.TrimType"),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropSchema node)
    {
      _ = context.Output.Append("DROP SCHEMA ");
      TranslateIdentifier(context.Output, node.Schema.DbName);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      _ = context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropView node)
    {
      _ = context.Output.Append("DROP VIEW ");
      Translate(context, node.View);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case char:
        case string:
          TranslateString(output, literalValue.ToString());
          break;
        case TimeSpan v:
          _ = output.Append(v.Ticks * 100);
          break;
        case bool v:
          _ = output.Append(v ? "cast(1 as bit)" : "cast(0 as bit)");
          break;
        case DateTime dateTime:
          var dateTimeRange = (ValueRange<DateTime>) Driver.ServerInfo.DataTypes.DateTime.ValueRange;
          var newDateTime = ValueRangeValidator.Correct(dateTime, dateTimeRange);
          _ = output.Append(newDateTime.ToString(DateTimeFormatString));
          break;
        case byte[] array:
          var builder = output.StringBuilder;
          _ = builder.EnsureCapacity(builder.Length + 2 * (array.Length + 1));
          _ = builder.Append("0x")
            .AppendHexArray(array);
          break;
        case Guid guid:
          TranslateString(output, guid.ToString());
          break;
        case long v:
          _ = output.Append($"CAST({v} as BIGINT)");
          break;
        case DateOnly dateOnly:
          var dateOnlyRange = (ValueRange<DateTime>) Driver.ServerInfo.DataTypes.DateTime.ValueRange;
          var newDateOnly = ValueRangeValidator.Correct(dateOnly.ToDateTime(TimeOnly.MinValue), dateOnlyRange).Date;
          _ = output.Append(newDateOnly.ToString(DateOnlyFormatString, DateTimeFormat));
          break;
        case DateTimeOffset dateTimeOffset:
          var dateTimeOffsetRange = (ValueRange<DateTimeOffset>) Driver.ServerInfo.DataTypes.DateTimeOffset.ValueRange;
          var newDateTimeOffset = ValueRangeValidator.Correct(dateTimeOffset, dateTimeOffsetRange);
          _ = context.Output.Append(newDateTimeOffset.ToString(DateTimeOffsetFormatString));
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Collation collation)
    {
      _ = context.Output.Append(collation.DbName);
    }

    #endregion

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropSequence node) =>
      TranslateSequenceStatement(context, node.Sequence, "DROP");

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      if (section == NodeSection.Entry) {
        TranslateSequenceStatement(context, node.Sequence, "ALTER");
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      if (section == NodeSection.Entry) {
        TranslateSequenceStatement(context, node.Sequence, "CREATE");
      }
    }

    private void TranslateSequenceStatement(SqlCompilerContext context, Sequence sequence, string action)
    {
      // SQL Server does not support database qualification in create/drop/alter sequence.
      // We add explicit "use" statement before such statements.
      // This changes current database as side effect,
      // but it's OK because we always use database qualified objects in all other statements.

      AddUseStatement(context, sequence.Schema.Catalog);
      _ = context.Output.Append(action)
        .Append(" SEQUENCE ");
      TranslateIdentifier(context.Output, sequence.Schema.DbName, sequence.DbName);
    }

    #region Enums and other types that require translation to string

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.Count:
          _ = output.Append("COUNT_BIG");
          break;
        case SqlNodeType.Concat:
          _ = output.Append("+");
          break;
        case SqlNodeType.Overlaps:
          throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type));
        default:
          base.Translate(output, type);
          break;
      }
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlNodeType type)
    {
      return type switch {
        SqlNodeType.Overlaps => throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type)),
        _ => base.TranslateToString(type),
      };
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType functionType)
    {
      switch (functionType) {
        case SqlFunctionType.IntervalAbs:
          _ = output.Append("ABS");
          break;
        case SqlFunctionType.IntervalNegate:
          _ = output.Append("-");
          break;
        case SqlFunctionType.CurrentDate:
          _ = output.Append("GETDATE");
          break;
        case SqlFunctionType.CharLength:
          _ = output.Append("LEN");
          break;
        case SqlFunctionType.BinaryLength:
          _ = output.Append("DATALENGTH");
          break;
        case SqlFunctionType.Position:
          _ = output.Append("CHARINDEX");
          break;
        case SqlFunctionType.Atan2:
          _ = output.Append("ATN2");
          break;
        case SqlFunctionType.LastAutoGeneratedId:
          _ = output.Append("SCOPE_IDENTITY");
          break;
        case SqlFunctionType.CurrentDateTimeOffset:
          _ = output.Append("SYSDATETIMEOFFSET");
          break;
        default:
          base.Translate(output, functionType);
          break;
      }
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlFunctionType functionType) =>
      functionType switch {
        SqlFunctionType.CharLength => "LEN",
        _ => base.TranslateToString(functionType)
      };

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTrimType type) { }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      _ = output.Append("ROWLOCK");

      if (lockType.Supports(SqlLockType.Update)) {
        _ = output.Append(", UPDLOCK");
      }
      else if (lockType.Supports(SqlLockType.Exclusive)) {
        _ = output.Append(", XLOCK");
      }

      if (lockType.Supports(SqlLockType.ThrowIfLocked)) {
        _ = output.Append(", NOWAIT");
      }
      else if (lockType.Supports(SqlLockType.SkipLocked)) {
        _ = output.Append(", READPAST");
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlJoinMethod method)
    {
      return method switch {
        SqlJoinMethod.Hash => "HASH",
        SqlJoinMethod.Merge => "MERGE",
        SqlJoinMethod.Loop => "LOOP",
        SqlJoinMethod.Remote => "REMOTE",
        _ => string.Empty,
      };
    }

    protected virtual string TranslateChangeTrackingMode(ChangeTrackingMode mode)
    {
      switch (mode) {
        case ChangeTrackingMode.Auto:
          return "AUTO";
        case ChangeTrackingMode.Manual:
          return "MANUAL";
        case ChangeTrackingMode.Off:
          return "OFF";
        case ChangeTrackingMode.OffWithNoPopulation:
          return "OFF, NO POPULATION";
        default:
          return "AUTO";
      }
    }

    #endregion

    #region Other types that can be written directly to output

    /// <inheritdoc/>
    public override void TranslateString(IOutput output, string str)
    {
      _ = output.Append("N");
      base.TranslateString(output, str);
    }

    #endregion

    #region Provider-specific heplers

    protected static string RemoveFromStringInvalidCharacters(string name)
    {
      unsafe {
        Span<char> chars = stackalloc char[name.Length];
        for (int i = 0, l = name.Length; i < name.Length; i++) {
          chars[i] = char.IsLetterOrDigit(name[i]) ? name[i] : '_';
        }
        return new string(chars);
      }
    }

    protected static void AddUseStatement(SqlCompilerContext context, Catalog catalog)
    {
      if (context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects)) {
        _ = context.Output.Append($"USE [{catalog.DbName}]; ");
      }
    }

    protected void TranslateExecSpRename(SqlCompilerContext context, SchemaNode affectedNode, DataTableNode tableObject, string newName)
    {
      var objectType = tableObject switch {
        null => "'OBJECT'",
        TableColumn c => "'COLUMN'",
        Index i when !i.IsFullText => "'INDEX'",
        _ => throw new NotSupportedException()
      };

      var output = context.Output;
      _ = output.Append("EXEC ");
      if (context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects)) {
        TranslateIdentifier(output, affectedNode.Schema.Catalog.DbName);
        _ = output.Append("..");
      }
      _ = output.Append("sp_rename '");
      if (tableObject is null) {
        Translate(context, affectedNode);
      }
      else {
        var table = tableObject.DataTable;
        var schema = table.Schema;
        TranslateIdentifier(context.Output, schema.Catalog.DbName, schema.DbName, table.DbName, tableObject.DbName);
      }
      _ = output.Append("', '")
        .Append(newName)
        .Append('\'');
      if (objectType is not null) {
        _ = output.Append($", {objectType}");
      }
    }

    protected static string GetCurentNameOfDefaultConstraintScript(string parameterName,
      string databaseName, string schemaName, string tableName, string columnName)
    {
      return 
        $"DECLARE @{parameterName} VARCHAR(256)" + Environment.NewLine +
        $"SELECT @{parameterName} = [{databaseName}].sys.default_constraints.name " +
        $"FROM [{databaseName}].sys.all_columns " +
        $"INNER JOIN [{databaseName}].sys.tables ON all_columns.object_id = tables.object_id " +
        $"INNER JOIN [{databaseName}].sys.schemas ON tables.schema_id = schemas.schema_id " +
        $"INNER JOIN [{databaseName}].sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id " +
        $"WHERE schemas.name = '{schemaName}' AND tables.name = '{tableName}' AND all_columns.name = '{columnName}'";
    }

    #endregion


    public Translator(SqlDriver driver)
      : base(driver)
    {
      FloatFormatString = "'cast('" + base.FloatFormatString + "'e0 as real')";
      DoubleFormatString = "'cast('" + base.DoubleFormatString + "'e0 as float')";
    }
  }
}