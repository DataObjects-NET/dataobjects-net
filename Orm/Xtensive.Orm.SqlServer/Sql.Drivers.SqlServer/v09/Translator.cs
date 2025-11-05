// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.SqlServer.Resources;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class Translator : SqlTranslator
  {
    public override string DateTimeFormatString => @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fff\'' as datetime)'";
    public override string DateOnlyFormatString => @"'cast ('\'yyyy\-MM\-dd\'' as date)'";
    public override string TimeOnlyFormatString => @"'cast ('\'HH\:mm\:ss\.fff\'' as time)'";
    public override string TimeSpanFormatString => string.Empty;

    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberDecimalSeparator = ".";
    }

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
    public override void Translate(IOutput output, SqlNodeType type)
    {
      switch(type) {
        case SqlNodeType.Count: _ = output.Append("COUNT_BIG"); break;
        case SqlNodeType.Concat: _ = output.Append("+"); break;
        case SqlNodeType.Overlaps: throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type));
        default: base.Translate(output, type); break;
      };
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
    public override void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case TableColumnSection.Type:
          if (column.Domain == null) {
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
      TranslateIdentityDescriptor(context, descriptor, section);
    }

    protected void TranslateIdentityDescriptor(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.StartValue:
        case SequenceDescriptorSection.RestartValue:
          if (descriptor.StartValue.HasValue) {
            _ = context.Output.Append("IDENTITY (").Append(descriptor.StartValue.Value).Append(RowItemDelimiter);
          }
          break;
        case SequenceDescriptorSection.Increment when descriptor.Increment.HasValue:
          _ = context.Output.Append(descriptor.Increment.Value).Append(")");
          break;
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
          if (node.Expression == null)
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
          if (joinHint != null) {
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
        if (statement == null || statement.Hints.Count == 0) {
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

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      var output = context.Output;
      switch (section) {
        case SelectSection.Entry:
          base.Translate(context, node, section);
          break;
        case SelectSection.Limit:
          _ = output.Append("TOP");
          break;
        case SelectSection.Offset:
          throw new NotSupportedException();
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
      TranslateExecSpRename(context, node.Table, () => Translate(context, node.Table), node.NewName, null);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
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
      var schema = table.Schema;
      TranslateExecSpRename(context,
        table,
        () => TranslateIdentifier(context.Output, schema.Catalog.DbName, schema.DbName, table.DbName, action.Column.DbName),
        action.NewName,
        "COLUMN");
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
      var gettingNameOfDefaultConstraintScript = GetCurentNameOfDefaultConstraintScript();

      var sqlVariableName = RemoveFromStringInvalidCharacters($"var_{schema.DbName}_{table.DbName}_{column.DbName}");

      _ = context.Output.Append(string.Format(gettingNameOfDefaultConstraintScript,
          sqlVariableName,
          QuoteIdentifier(schema.Catalog.DbName),
          schema.DbName,
          table.DbName,
          column.DbName))
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
          _ = context.Output.Append(",");
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
      if (select != null) {
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
          var newValue = ValueRangeValidator.Correct(dateTime, dateTimeRange);
          _ = output.Append(newValue.ToString(DateTimeFormatString));
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
          output.Append(newDateOnly.ToString(DateOnlyFormatString, DateTimeFormat));
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

    /// <inheritdoc/>
    public override void TranslateString(IOutput output, string str)
    {
      _ = output.Append("N");
      base.TranslateString(output, str);
    }

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
    public override void Translate(IOutput output, SqlFunctionType functionType) {
      switch (functionType) {
        case SqlFunctionType.IntervalAbs: _ = output.Append("ABS"); break;
        case SqlFunctionType.IntervalNegate: _ = output.Append("-"); break;
        case SqlFunctionType.CurrentDate: _ = output.Append("GETDATE"); break;
        case SqlFunctionType.CharLength: _ = output.Append("LEN"); break;
        case SqlFunctionType.BinaryLength: _ = output.Append("DATALENGTH"); break;
        case SqlFunctionType.Position: _ = output.Append("CHARINDEX"); break;
        case SqlFunctionType.Atan2: _ = output.Append("ATN2"); break;
        case SqlFunctionType.LastAutoGeneratedId: _ = output.Append("SCOPE_IDENTITY"); break;
        default: base.Translate(output, functionType); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlFunctionType functionType) =>
      functionType switch {
        SqlFunctionType.CharLength => "LEN",
        _ => base.TranslateToString(functionType)
      };

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTrimType type) { }

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

    protected void TranslateExecSpRename(SqlCompilerContext context, SchemaNode affectedNode, System.Action printObjectName, string newName, string type)
    {
      var output = context.Output;
      _ = output.Append("EXEC ");
      if (context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects)) {
        TranslateIdentifier(output, affectedNode.Schema.Catalog.DbName);
        _ = output.Append("..");
      }
      _ = output.Append("sp_rename '");
      printObjectName();
      _ = output.Append("', '")
        .Append(newName)
        .Append('\'');
      if (type != null) {
        _ = output.Append($", '{type}'");
      }
    }


    protected virtual string GetCurentNameOfDefaultConstraintScript()
    {
      return @"DECLARE @{0} VARCHAR(256)
        SELECT @{0} = {1}.sys.default_constraints.name
      FROM 
        {1}.sys.all_columns
      INNER JOIN
        {1}.sys.tables
      ON all_columns.object_id = tables.object_id
      INNER JOIN 
        {1}.sys.schemas
      ON tables.schema_id = schemas.schema_id  
      INNER JOIN
        {1}.sys.default_constraints
      ON all_columns.default_object_id = default_constraints.object_id

      WHERE 
        schemas.name = '{2}'
        AND tables.name = '{3}'
        AND all_columns.name = '{4}'";
    }

    protected static string RemoveFromStringInvalidCharacters(string name)
    {
      var normalizedName = name.Aggregate(string.Empty,
        (current, character) => current.Insert(current.Length, !char.IsLetterOrDigit(character)
          ? Convert.ToString('_', CultureInfo.InvariantCulture)
          : Convert.ToString(character, CultureInfo.InvariantCulture)));
      return normalizedName;
    }

    protected static void AddUseStatement(SqlCompilerContext context, Catalog catalog)
    {
      if (context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects)) {
        _ = context.Output.Append($"USE [{catalog.DbName}]; ");
      }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Translator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal Translator(SqlDriver driver)
      : base(driver)
    {
      FloatFormatString = "'cast('" + base.FloatFormatString + "'e0 as real')";
      DoubleFormatString = "'cast('" + base.DoubleFormatString + "'e0 as float')";
    }
  }
}