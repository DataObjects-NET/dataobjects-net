// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.SQLite.Resources;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.SQLite.v3
{
    internal class Translator : SqlTranslator
    {
        /// <inheritdoc/>
        public override string DateTimeFormatString
        {
            get { return @"\'yyyy\-MM\-dd HH\:mm\:ss\.ffffff\'"; }
        }

        /// <inheritdoc/>
        public override string TimeSpanFormatString
        {
            get { return string.Empty; }
        }

        public override string FloatFormatString
        {
            get { return base.FloatFormatString; }
        }

        public override string DoubleFormatString
        {
            get { return base.DoubleFormatString; }
        }

        /// <inheritdoc/>
        public override string DdlStatementDelimiter
        {
            get { return ";"; }
        }

        /// <inheritdoc/>
        public override string BatchItemDelimiter
        {
            get { return ";\r\n"; }
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            FloatNumberFormat.NumberDecimalSeparator = ".";
            DoubleNumberFormat.NumberDecimalSeparator = ".";
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
        {
            if (node.FunctionType == SqlFunctionType.LastAutoGeneratedId)
            {
                if (section == FunctionCallSection.Entry)
                    return Translate(node.FunctionType);
                if (section == FunctionCallSection.Exit)
                    return string.Empty;
            }
            switch (section)
            {
                case FunctionCallSection.ArgumentEntry:
                    return string.Empty;
                case FunctionCallSection.ArgumentDelimiter:
                    return ArgumentDelimiter;
                default:
                    return base.Translate(context, node, section, position);
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlFunctionType functionType)
        {
            switch (functionType)
            {
                case SqlFunctionType.IntervalAbs:
                    return "ABS";
                case SqlFunctionType.Substring:
                    return "SUBSTR";
                case SqlFunctionType.IntervalNegate:
                    return "-";
                case SqlFunctionType.CurrentDate:
                    return "DATE('NOW')";
                case SqlFunctionType.BinaryLength:
                    return "LENGTH";
                case SqlFunctionType.LastAutoGeneratedId:
                    return "LAST_INSERT_ROWID()";
            }
            return base.Translate(functionType);
        }

        /// <inheritdoc/>
        public override string Translate(SqlNodeType type)
        {
            switch (type)
            {
                case SqlNodeType.Count:
                    return "COUNT";
                case SqlNodeType.Concat:
                    return "+";
                case SqlNodeType.Overlaps:
                    throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type));
            }
            return base.Translate(type);
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
        {
            switch (section)
            {
                case AlterTableSection.AddColumn:
                    return "ADD";
                case AlterTableSection.DropBehavior:
                    return string.Empty;
                default:
                    return base.Translate(context, node, section);
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
        {
            switch (section)
            {
                case TableColumnSection.Exit:
                    return string.Empty;
                case TableColumnSection.GeneratedExit:
                    return "AUTOINCREMENT"; //Workaround based on fake sequence.
                default:
                    return base.Translate(context, column, section);
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
        {
            switch (section)
            {
                case ConstraintSection.Exit:
                    ForeignKey fk = constraint as ForeignKey;
                    if (fk != null)
                    {
                        if (fk.OnUpdate == ReferentialAction.Cascade)
                            return ") ON UPDATE CASCADE";
                        if (fk.OnDelete == ReferentialAction.Cascade)
                            return ") ON DELETE CASCADE";
                    }
                    return ")";
                default:
                    return base.Translate(context, constraint, section);
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
        {
            switch (section)
            {
                case CreateTableSection.Entry:
                    var builder = new StringBuilder();
                    builder.Append("CREATE ");
                    var temporaryTable = node.Table as TemporaryTable;
                    if (temporaryTable != null)
                    {
                        builder.Append("TEMPORARY TABLE " + Translate(temporaryTable));
                    }
                    else
                    {
                        builder.Append("TABLE " + Translate(node.Table));
                    }
                    return builder.ToString();
                case CreateTableSection.Exit:
                    return string.Empty;
            }
            return base.Translate(context, node, section);
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlDropIndex node)
        {
            return string.Format("DROP INDEX {0}.{1}", QuoteIdentifier(node.Index.DataTable.DbName), QuoteIdentifier(node.Index.DbName));
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
        {
            if (section == DeclareCursorSection.Holdability || section == DeclareCursorSection.Returnability)
                return string.Empty;
            return base.Translate(context, node, section);
        }

        public override string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
        {
            switch (section)
            {
                case JoinSection.Specification:
                    if (node.Expression == null)
                        switch (node.JoinType)
                        {
                            case SqlJoinType.RightOuterJoin:
                                throw new NotSupportedException();
                            case SqlJoinType.FullOuterJoin:
                                throw new NotSupportedException();
                            case SqlJoinType.CrossApply:
                                throw new NotSupportedException();
                            case SqlJoinType.LeftOuterApply:
                                throw new NotSupportedException();
                        }
                    var joinHint = TryFindJoinHint(context, node);
                    return Translate(node.JoinType)
                      + (joinHint != null ? " " + Translate(joinHint.Method) : string.Empty) + " JOIN";
            }
            return base.Translate(context, node, section);
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
        {
            if (node.All && section == QueryExpressionSection.All && (node.NodeType == SqlNodeType.Except || node.NodeType == SqlNodeType.Intersect))
                return string.Empty;
            return base.Translate(context, node, section);
        }

        private static SqlJoinHint TryFindJoinHint(SqlCompilerContext context, SqlJoinExpression node)
        {
            SqlQueryStatement statement = null;
            for (int i = 0, count = context.GetTraversalPath().Length; i < count; i++)
            {
                if (context.GetTraversalPath()[i] is SqlQueryStatement)
                    statement = context.GetTraversalPath()[i] as SqlQueryStatement;
            }
            if (statement == null || statement.Hints.Count == 0)
                return null;
            var candidate = statement.Hints
              .OfType<SqlJoinHint>()
              .FirstOrDefault(hint => hint.Table == node.Right);
            return candidate;
        }

        /// <inheritdoc/>
        public override string Translate(SqlJoinMethod method)
        {
            switch (method)
            {
                case SqlJoinMethod.Hash:
                    return "HASH";
                case SqlJoinMethod.Merge:
                    return "MERGE";
                case SqlJoinMethod.Loop:
                    return "LOOP";
                case SqlJoinMethod.Remote:
                    return "REMOTE";
                default:
                    return string.Empty;
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
        {
            switch (section)
            {
                case SelectSection.Limit:
                    return "TOP";
                case SelectSection.Offset:
                    throw new NotSupportedException();
                case SelectSection.Exit:
                    if (node.Hints.Count == 0)
                        return string.Empty;
                    var hints = new List<string>(node.Hints.Count);
                    foreach (var hint in node.Hints)
                    {
                        if (hint is SqlForceJoinOrderHint)
                            hints.Add("FORCE ORDER");
                        else if (hint is SqlFastFirstRowsHint)
                            hints.Add("FAST " + (hint as SqlFastFirstRowsHint).Amount);
                        else if (hint is SqlNativeHint)
                            hints.Add((hint as SqlNativeHint).HintText);
                    }
                    return hints.Count > 0 ? "OPTION (" + string.Join(", ", hints.ToArray()) + ")" : string.Empty;
            }

            return base.Translate(context, node, section);
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
        {
            //http://www.sqlite.org/lang_expr.html
            switch (node.Type.Type)
            {
                case SqlType.Binary:
                case SqlType.Char:
                case SqlType.Interval:
                case SqlType.DateTime:
                    switch (section)
                    {
                        case NodeSection.Entry:
                            return "CAST(";
                        case NodeSection.Exit:
                            return "AS " + Translate(node.Type) + ")";
                        default:
                            throw new ArgumentOutOfRangeException("section");
                    }
                case SqlType.Int16:
                case SqlType.Int32:
                    switch (section)
                    {
                        case NodeSection.Entry:
                            return "CAST(";
                        case NodeSection.Exit:
                            return "AS " + Translate(node.Type) + ")";
                        default:
                            throw new ArgumentOutOfRangeException("section");
                    }
                case SqlType.Decimal:
                case SqlType.Double:
                case SqlType.Float:
                    switch (section)
                    {
                        case NodeSection.Entry:
                            return string.Empty;
                        case NodeSection.Exit:
                            return "+ 0.0";
                        default:
                            throw new ArgumentOutOfRangeException("section");
                    }
            }
            return string.Empty;
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlRenameTable node)
        {
            //http://stackoverflow.com/questions/805363/how-do-i-rename-a-column-in-a-sqlite-database-table
            return string.Format("ALTER TABLE {0} RENAME TO {1}", Translate(node.Table), node.NewName);
        }

        /// <inheritdoc/>
        public virtual string Translate(SqlCompilerContext context, SqlRenameColumn action)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
        {
            switch (section)
            {
                case TrimSection.Entry:
                    switch (node.TrimType)
                    {
                        case SqlTrimType.Leading:
                            return "LTRIM(";
                        case SqlTrimType.Trailing:
                            return "RTRIM(";
                        case SqlTrimType.Both:
                            return "TRIM(";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case TrimSection.Exit:
                    switch (node.TrimType)
                    {
                        case SqlTrimType.Leading:
                        case SqlTrimType.Trailing:
                            return ")";
                        case SqlTrimType.Both:
                            return ")";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlDropSchema node)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlDropTable node)
        {
            return "DROP TABLE IF EXISTS " + Translate(node.Table);
        }

        /// <inheritdoc/>
        public override string Translate(SqlCompilerContext context, SqlDropView node)
        {
            return "DROP VIEW IF EXISTS " + Translate(node.View);
        }

        /// <inheritdoc/>
        public override string Translate(SqlTrimType type)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public override string Translate(Collation collation)
        {
            return collation.DbName;
        }

        /// <inheritdoc/>
        public override string Translate(SqlLockType lockType)
        {
            if (lockType.Supports(SqlLockType.Shared))
                return "SHARED";
            if (lockType.Supports(SqlLockType.Exclusive))
                return "EXCLUSIVE";
            if (lockType.Supports(SqlLockType.SkipLocked) || lockType.Supports(SqlLockType.ThrowIfLocked))
                return base.Translate(lockType);
            return "PENDING"; //http://www.sqlite.org/lockingv3.html Not sure whether this is the best alternative.
        }

        protected virtual string TranslateClrType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "int";
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "bigint";
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return "numeric";
                case TypeCode.Char:
                case TypeCode.String:
                    return "text";
                case TypeCode.DateTime:
                    return "timestamp";
                default:
                    if (type == typeof(TimeSpan))
                        return "bigint";
                    if (type == typeof(Guid))
                        return "guid";
                    return "text";
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
        }
    }
}