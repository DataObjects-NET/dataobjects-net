// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10

using System;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Drivers.Firebird.Resources;


namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    internal partial class Extractor : Model.Extractor
    {
        private const int DefaultNumericPrecision = 18;
        private const int DefaultNumericScale = 0;

        private Catalog theCatalog;
        private string targetSchema;

        private readonly Dictionary<string, string> replacementsRegistry = new Dictionary<string, string>();

        private readonly Dictionary<string, Xtensive.Sql.Model.DataTable> tables = new Dictionary<string, Xtensive.Sql.Model.DataTable>();
        private readonly Dictionary<string, Xtensive.Sql.Model.View> views = new Dictionary<string, Xtensive.Sql.Model.View>();

        private string GetSchema(IDataRecord p_row)
        {
            return p_row.GetString(1) ?? theCatalog.DefaultSchema.Name;
        }

        protected override void Initialize()
        {
            theCatalog = new Catalog(Driver.CoreServerInfo.DatabaseName);
        }

        public override Catalog ExtractCatalog()
        {
            targetSchema = null;
            ExtractSchemas();
            ExtractCatalogContents();
            return theCatalog;
        }

        public override Schema ExtractSchema(string schemaName)
        {
            targetSchema = schemaName.ToUpperInvariant();
            theCatalog.CreateSchema(targetSchema);
            ExtractCatalogContents();
            return theCatalog.Schemas[targetSchema];
        }

        private void ExtractCatalogContents()
        {
            ExtractTables();
            ExtractTableColumns();
            ExtractViews();
            ExtractViewColumns();
            ExtractIndexes();
            ExtractForeignKeys();
            ExtractCheckConstaints();
            ExtractUniqueAndPrimaryKeyConstraints();
            ExtractSequences();
        }

        private void ExtractSchemas()
        {
            var defaultSchemaName = Driver.CoreServerInfo.DefaultSchemaName.ToUpperInvariant();
            var defaultSchema = theCatalog.CreateSchema(defaultSchemaName);
            theCatalog.DefaultSchema = defaultSchema;
        }

        private System.Data.DataTable GetTable(string objtype, string[] restr)
        {
            ConnectionState ocs = Connection.State;
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            System.Data.DataTable dt = Connection.UnderlyingConnection.GetSchema(objtype, restr);
            if (ocs == ConnectionState.Closed)
                Connection.Close();
            return dt;
        }

        private void ExtractTables()
        {
            using (var reader = ExecuteReader(GetExtractTablesQuery()))
            {
                while (reader.Read())
                {
                    var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
                    string tableName = reader.GetString(1);
                    int tableType = reader.GetInt16(2);
                    bool isTemporary = tableType == 4 || tableType == 5;
                    if (isTemporary)
                    {
                        var table = schema.CreateTemporaryTable(tableName);
                        table.PreserveRows = tableType == 4;
                        table.IsGlobal = true;
                    }
                    else
                    {
                        schema.CreateTable(tableName);
                    }
                }
            }
        }

        private void ExtractTableColumns()
        {
            using (var reader = ExecuteReader(GetExtractTableColumnsQuery()))
            {
                Table table = null;
                int lastColumnId = int.MaxValue;
                while (reader.Read())
                {
                    int columnSeq = reader.GetInt16(2);
                    if (columnSeq <= lastColumnId)
                    {
                        var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
                        table = schema.Tables[reader.GetString(1)];
                    }
                    if (table.Name.ToUpper().StartsWith("RDB$") || table.Name.ToUpper().StartsWith("MON$"))
                        continue;
                    var column = table.CreateColumn(reader.GetString(3));
                    column.DataType = CreateValueType(reader, 4, 6, 7, 8);
                    column.IsNullable = ReadBool(reader, 9);
                    var defaultValue = ReadStringOrNull(reader, 10);
                    if (!string.IsNullOrEmpty(defaultValue))
                        column.DefaultValue = SqlDml.Native(defaultValue);
                    lastColumnId = columnSeq;
                }
            }
        }

        private void ExtractViews()
        {
            using (var reader = ExecuteReader(GetExtractViewsQuery()))
            {
                while (reader.Read())
                {
                    var schema = theCatalog.DefaultSchema;  // theCatalog.Schemas[reader.GetString(0)];
                    var view = reader.GetString(1);
                    var definition = ReadStringOrNull(reader, 2);
                    if (string.IsNullOrEmpty(definition))
                        schema.CreateView(view);
                    else
                        schema.CreateView(view, SqlDml.Native(definition));
                }
            }
        }

        private void ExtractViewColumns()
        {
            using (var reader = ExecuteReader(GetExtractViewColumnsQuery()))
            {
                int lastColumnId = int.MaxValue;
                View view = null;
                while (reader.Read())
                {
                    int columnId = reader.GetInt16(3);
                    if (columnId <= lastColumnId)
                    {
                        var schema = theCatalog.DefaultSchema;  // theCatalog.Schemas[reader.GetString(0)];
                        view = schema.Views[reader.GetString(1)];
                    }
                    view.CreateColumn(reader.GetString(2));
                    lastColumnId = columnId;
                }
            }
        }

        private void ExtractIndexes()
        {
            // it's possible to have table and index in different schemas in oracle.
            // we silently ignore this, indexes are always belong to the same schema as its table.
            using (var reader = ExecuteReader(GetExtractIndexesQuery()))
            {
                int lastColumnPosition = int.MaxValue;
                Table table = null;
                Index index = null;
                while (reader.Read())
                {
                    int columnPosition = reader.GetInt16(6);
                    if (columnPosition <= lastColumnPosition)
                    {
                        var schema = theCatalog.Schemas[reader.GetString(0)];
                        table = schema.Tables[reader.GetString(1)];
                        index = table.CreateIndex(reader.GetString(2));
                        index.IsUnique = ReadBool(reader, 3);
                        index.IsBitmap = reader.GetString(4) == "BITMAP";
                        if (!reader.IsDBNull(5))
                        {
                            int pctFree = reader.GetInt16(5);
                            index.FillFactor = (byte)(100 - pctFree);
                        }
                    }
                    var column = table.TableColumns[reader.GetString(7)];
                    bool isAscending = reader.GetString(8) == "ASC";
                    index.CreateIndexColumn(column, isAscending);
                    lastColumnPosition = columnPosition;
                }
            }
        }

        private void ExtractForeignKeys()
        {
            using (var reader = ExecuteReader(GetExtractForeignKeysQuery()))
            {
                int lastColumnPosition = int.MaxValue;
                ForeignKey constraint = null;
                Table referencingTable = null;
                Table referencedTable = null;
                while (reader.Read())
                {
                    int columnPosition = reader.GetInt16(7);
                    if (columnPosition <= lastColumnPosition)
                    {
                        var referencingSchema = theCatalog.Schemas[reader.GetString(0)];
                        referencingTable = referencingSchema.Tables[reader.GetString(1)];
                        constraint = referencingTable.CreateForeignKey(reader.GetString(2));
                        ReadConstraintProperties(constraint, reader, 3, 4);
                        ReadCascadeAction(constraint, reader, 5);
                        var referencedSchema = theCatalog.Schemas[reader.GetString(8)];
                        referencedTable = referencedSchema.Tables[reader.GetString(9)];
                        constraint.ReferencedTable = referencedTable;
                    }
                    var referencingColumn = referencingTable.TableColumns[reader.GetString(6)];
                    var referencedColumn = referencedTable.TableColumns[reader.GetString(10)];
                    constraint.Columns.Add(referencingColumn);
                    constraint.ReferencedColumns.Add(referencedColumn);
                    lastColumnPosition = columnPosition;
                }
            }
        }

        private void ExtractUniqueAndPrimaryKeyConstraints()
        {
            using (var reader = ExecuteReader(GetExtractUniqueAndPrimaryKeyConstraintsQuery()))
            {
                Table table = null;
                string constraintName = null;
                string constraintType = null;
                var columns = new List<TableColumn>();
                int lastColumnPosition = -1;
                while (reader.Read())
                {
                    int columnPosition = reader.GetInt16(5);
                    if (columnPosition <= lastColumnPosition)
                    {
                        CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
                        columns.Clear();
                    }
                    if (columns.Count == 0)
                    {
                        var schema = theCatalog.Schemas[reader.GetString(0)];
                        table = schema.Tables[reader.GetString(1)];
                        constraintName = reader.GetString(2);
                        constraintType = reader.GetString(3);
                    }
                    columns.Add(table.TableColumns[reader.GetString(4)]);
                    lastColumnPosition = columnPosition;
                }
                if (columns.Count > 0)
                    CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
            }
        }

        private void ExtractCheckConstaints()
        {
            using (var reader = ExecuteReader(GetExtractCheckConstraintsQuery()))
            {
                while (reader.Read())
                {
                    var schema = theCatalog.Schemas[reader.GetString(0)];
                    var table = schema.Tables[reader.GetString(1)];
                    var name = reader.GetString(2);
                    // It looks like ODP.NET sometimes fail to read a LONG column via GetString
                    // It returns empty string instead of the actual value.
                    var condition = string.IsNullOrEmpty(reader.GetString(3))
                      ? null
                      : SqlDml.Native(reader.GetString(3));
                    var constraint = table.CreateCheckConstraint(name, condition);
                    ReadConstraintProperties(constraint, reader, 4, 5);
                }
            }
        }

        private void ExtractSequences()
        {
            using (var reader = ExecuteReader(GetExtractSequencesQuery()))
            {
                while (reader.Read())
                {
                    var schema = theCatalog.Schemas[reader.GetString(0)];
                    var sequence = schema.CreateSequence(reader.GetString(1));
                    sequence.DataType = new SqlValueType(SqlType.Decimal, DefaultNumericPrecision, DefaultNumericScale);
                    var descriptor = sequence.SequenceDescriptor;
                    descriptor.MinValue = reader.GetInt32(2);
                    descriptor.MaxValue = reader.GetInt32(3);
                    descriptor.Increment = reader.GetInt32(4);
                    descriptor.IsCyclic = ReadBool(reader, 5);
                }
            }
        }

        private SqlValueType CreateValueType(IDataRecord row,
          int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
        {
            string typeName = row[typeNameIndex].ToString().ToUpper();
            if (typeName == "NUMERIC" || typeName == "DECIMAL")
            {
                int precision = Convert.ToInt32(row[precisionIndex]);
                int scale = Convert.ToInt32(row[scaleIndex]);
                return new SqlValueType(SqlType.Decimal, precision, scale);
            }
            if (typeName.StartsWith("TIMESTAMP"))
                return new SqlValueType(SqlType.DateTime);
            if (typeName == "VARCHAR" || typeName == "CHAR")
            {
                int length = Convert.ToInt32(row[charLengthIndex]);
                var sqlType = typeName.Length == 4 ? SqlType.Char : SqlType.VarChar;
                return new SqlValueType(sqlType, length);
            }
            var typeInfo = Driver.ServerInfo.DataTypes[typeName];
            return typeInfo != null
              ? new SqlValueType(typeInfo.Type)
              : new SqlValueType(typeName);
        }

        private static void CreateIndexBasedConstraint(
          Table table, string constraintName, string constraintType, List<TableColumn> columns)
        {
            switch (constraintType)
            {
                case "P":
                    table.CreatePrimaryKey(constraintName, columns.ToArray());
                    return;
                case "U":
                    table.CreateUniqueConstraint(constraintName, columns.ToArray());
                    return;
                default:
                    throw new ArgumentOutOfRangeException("constraintType");
            }
        }

        private static bool ReadBool(IDataRecord row, int index)
        {
            short value = Convert.ToInt16(row.GetString(index));
            switch (value)
            {
                case 1:
                    return true;
                case 0:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(string.Format(Strings.ExInvalidBooleanSmallintValue, value));
            }
        }

        private static string ReadStringOrNull(IDataRecord row, int index)
        {
            return row.IsDBNull(index) ? null : row.GetString(index);
        }

        private static void ReadConstraintProperties(Constraint constraint,
          IDataRecord row, int isDeferrableIndex, int isInitiallyDeferredIndex)
        {
            constraint.IsDeferrable = row.GetString(isDeferrableIndex) == "DEFERRABLE";
            constraint.IsInitiallyDeferred = row.GetString(isInitiallyDeferredIndex) == "DEFERRED";
        }

        private static void ReadCascadeAction(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex)
        {
            var deleteRule = row.GetString(deleteRuleIndex);
            switch (deleteRule)
            {
                case "CASCADE":
                    foreignKey.OnDelete = ReferentialAction.Cascade;
                    return;
                case "SET NULL":
                    foreignKey.OnDelete = ReferentialAction.SetNull;
                    return;
                case "NO ACTION":
                    foreignKey.OnDelete = ReferentialAction.NoAction;
                    return;
            }
        }

        private string PerformReplacements(string query)
        {
            foreach (var registry in replacementsRegistry)
                query = query.Replace(registry.Key, registry.Value);
            return query;
        }

        protected override DbDataReader ExecuteReader(string commandText)
        {
            return base.ExecuteReader(commandText);
        }

        protected override DbDataReader ExecuteReader(ISqlCompileUnit statement)
        {
            var commandText = Connection.Driver.Compile(statement).GetCommandText();
            return base.ExecuteReader(commandText);
        }


        // Constructors

        public Extractor(SqlDriver driver)
            : base(driver)
        {
        }

    }
}
