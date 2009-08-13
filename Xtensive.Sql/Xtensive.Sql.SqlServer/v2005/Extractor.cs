// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.11

using System;
using System.Collections.Generic;
using Xtensive.Sql.Model;
using DataTable=Xtensive.Sql.Model.DataTable;

namespace Xtensive.Sql.SqlServer.v2005
{
  internal class Extractor : Model.Extractor
  {
    private int schemaId;
    private int firstDomainId;
    private readonly Dictionary<int, Schema> schemaIndex = new Dictionary<int, Schema>();
    private readonly Dictionary<int, Domain> domainIndex = new Dictionary<int, Domain>();
    private readonly Dictionary<int, string> typeNameIndex = new Dictionary<int, string>();
    private readonly Dictionary<int, DataTableProxy> dataTableIndex = new Dictionary<int, DataTableProxy>();

    public override Catalog ExtractCatalog()
    {
      ExtractSchemas();
      ExtractTypes();
      ExtractTablesAndViews();
      ExtractColumns();
      ExtractIndexes();
      ExtractForeignKeys();
      return Catalog;
    }

    protected override Schema ExtractSchema()
    {
      return ExtractCatalog().DefaultSchema;
    }

    // All schemas
    private void ExtractSchemas()
    {
      const string query = "select s.schema_id, s.name, dp.name from sys.schemas as s inner join sys.database_principals as dp on s.principal_id = dp.principal_id where s.schema_id < 16384";

      using (var cmd = CreateCommand(query))
      using (var reader = cmd.ExecuteReader())
        while (reader.Read()) {
          Schema schema;
          int identifier = reader.GetInt32(0);
          string name = reader.GetString(1);
          if (Schema!=null && Schema.Name==name) {
            schema = Schema;
            schemaId = identifier;
          }
          else
            schema = Catalog.CreateSchema(name);
          schemaIndex[identifier] = schema;
          schema.Owner = reader.GetString(2);
        }
    }

    // Types & domains must be extracted for all schemas
    private void ExtractTypes()
    {
      const string query = "select schema_id, user_type_id, system_type_id, name, precision, scale, max_length from sys.types order by user_type_id";

      int currentSchemaId = schemaId;
      Schema schema = Schema;
      using (var command = CreateCommand(query))
      using (var reader = command.ExecuteReader())
        while (reader.Read()) {

          int userTypeId = reader.GetInt32(1);
          int systemTypeId = reader.GetByte(2);
          string name = reader.GetString(3);
          typeNameIndex[userTypeId] = name;

          if (userTypeId==systemTypeId)
            continue;

          // user defined type
          if (firstDomainId == 0)
            firstDomainId = userTypeId;

          GetSchema(reader.GetInt32(0), ref currentSchemaId, ref schema);
          var dataType = GetValueType(typeNameIndex[systemTypeId], reader.GetByte(4), reader.GetByte(5), reader.GetInt16(6));
          var domain = schema.CreateDomain(name, dataType);
          domainIndex[userTypeId] = domain;
        }
    }

    private void ExtractTablesAndViews()
    {
      string query = "select t.schema_id, t.object_id, t.name, t.type from (select schema_id, object_id, name, 0 type from sys.tables union select schema_id, object_id, name, 1 type from sys.views) as t";
      if (Schema!=null)
        query += " where t.schema_id = " + schemaId;
      query += " order by t.schema_id, t.object_id";

      int currentSchemaId = schemaId;
      Schema schema = Schema;
      using (var cmd = CreateCommand(query))
      using (var reader = cmd.ExecuteReader())
        while (reader.Read()) {
          GetSchema(reader.GetInt32(0), ref currentSchemaId, ref schema);
          int tableType = reader.GetInt32(3);
          DataTable dataTable;
          if (tableType==0)
            dataTable = schema.CreateTable(reader.GetString(2));
          else
            dataTable = schema.CreateView(reader.GetString(2));
          dataTableIndex[reader.GetInt32(1)] = new DataTableProxy(dataTable);
        }
    }

    private void ExtractColumns()
    {
      var trimChars = new[] {'(', ')'};
      string query = "select t.schema_id, c.object_id, c.column_id, c.name, c.user_type_id, c.precision, c.scale, c.max_length, c.collation_name, c.is_nullable, c.is_identity, dc.name, dc.definition from sys.columns as c inner join (select schema_id, object_id, 0 as type from sys.tables union select schema_id, object_id, 1 as type from sys.views) as t on c.object_id = t.object_id left outer join sys.default_constraints as dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id";
      if (Schema!=null)
        query += " where t.schema_id = " + schemaId;
      query += " order by t.schema_id, c.object_id, c.column_id";

      int currentTableId = 0;
      DataTableProxy dataTable = null;
      int currentSchemaId = schemaId;
      Schema schema = Schema;
      using (var cmd = CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          
          int tableId = reader.GetInt32(1);
          int columnId = reader.GetInt32(2);
          GetDataTable(tableId, ref currentTableId, ref dataTable);
          var table = dataTable.Table as Table;
          // Table column
          if (table != null) {
            var typeId = reader.GetInt32(4);
            var sqlDataType = GetValueType(typeNameIndex[typeId], reader.GetByte(5), reader.GetByte(6), reader.GetInt16(7));
            var column = table.CreateColumn(reader.GetString(3), sqlDataType);
            int count = table.TableColumns.Count;
            if (columnId != count) // <-db column index is not equal to column position in table.Columns. This is common after column removal & insertions.
              dataTable.RegisterColumnMapping(columnId, count - 1);

            if (typeId >= firstDomainId) {
              Domain domain;
              if (domainIndex.TryGetValue(typeId, out domain))
                column.Domain = domain;
            }

            // Collation
            if (!reader.IsDBNull(8)) {
              GetSchema(reader.GetInt32(0), ref currentSchemaId, ref schema);
              string collationName = reader.GetString(8);
              column.Collation = schema.Collations[collationName] ?? schema.CreateCollation(collationName);
            }

            // Nullability
            column.IsNullable = reader.GetBoolean(9);

            // Default constraint
            if (!reader.IsDBNull(11)) {
              table.CreateDefaultConstraint(reader.GetString(11), column);
              column.DefaultValue = reader.GetString(12).Trim(trimChars);
            }
          }
          else {
            var view = (View) dataTable.Table;
            view.CreateColumn(reader.GetString(3));
          }
        }
      }

      query = "select t.schema_id, ic.object_id, ic.column_id, ic.seed_value, ic.increment_value, ic.last_value from sys.identity_columns as ic inner join sys.tables as t on ic.object_id = t.object_id where seed_value is not null and increment_value is not null";
      if (Schema != null)
        query += " and t.schema_id = " + schemaId;
      query += " order by t.schema_id, ic.object_id";

      using (var cmd = CreateCommand(query))
        using (var reader = cmd.ExecuteReader())
          while (reader.Read()) {

            var dataColumn = dataTableIndex[reader.GetInt32(1)].GetColumn(reader.GetInt32(2));

            var tableColumn = (TableColumn)dataColumn;
            tableColumn.SequenceDescriptor = new SequenceDescriptor(tableColumn);
            if (!reader.IsDBNull(3))
              tableColumn.SequenceDescriptor.StartValue = Convert.ToInt64(reader.GetValue(3));
            if (!reader.IsDBNull(4))
              tableColumn.SequenceDescriptor.Increment = Convert.ToInt64(reader.GetValue(4));
            if (!reader.IsDBNull(5))
              tableColumn.SequenceDescriptor.LastValue = Convert.ToInt64(reader.GetValue(5));
          }
    }

    public void ExtractCheckConstraints()
    {
      // TODO: Implement
    }

    private void ExtractIndexes()
    {
      string query = "select t.schema_id, t.object_id, t.type, i.index_id, i.name, i.type, i.is_primary_key, i.is_unique, i.is_unique_constraint, i.fill_factor, ic.column_id, 0, ic.key_ordinal, ic.is_descending_key, ic.is_included_column from sys.indexes i inner join (select schema_id, object_id, 0 as type from sys.tables union select schema_id, object_id, 1 as type from sys.views) as t on i.object_id = t.object_id inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id where i.type < 3";
      if (Schema!=null)
        query += " and schema_id = " + schemaId;
      query += " order by t.schema_id, t.object_id, i.index_id, ic.is_included_column, ic.key_ordinal";

      int tableId = 0, indexId = 0;
      DataTableProxy table = null;
      Index index = null;
      PrimaryKey primaryKey;
      UniqueConstraint uniqueConstraint = null;
      using (var cmd = CreateCommand(query))
      using (var reader = cmd.ExecuteReader())
        while (reader.Read()) {

          int columnId = reader.GetInt32(10);

          // First column in index => new index
          if (reader.GetByte(12) == 1) {
            uniqueConstraint = null;
            index = null;
            // Table could be changed only on new index creation
            GetDataTable(reader.GetInt32(1), ref tableId, ref table);
            var name = reader.GetString(4);

            // Index is a part of primary key constraint
            if (reader.GetBoolean(6)) {
              primaryKey = ((Table) table.Table).CreatePrimaryKey(name);
              primaryKey.Columns.Add((TableColumn)table.GetColumn(columnId));
            }
            else {
              index = table.Table.CreateIndex(name);
              index.IsUnique = reader.GetBoolean(7);
              index.IsClustered = reader.GetByte(5) == 1;
              index.FillFactor = reader.GetByte(9);

              // Index is a part of unique constraint
              if (reader.GetBoolean(8)) {
                uniqueConstraint = ((Table) table.Table).CreateUniqueConstraint(name);
              }
            }
          }

          // Column is a part of unique constraint
          if (uniqueConstraint!=null && reader.GetBoolean(8))
            uniqueConstraint.Columns.Add((TableColumn) table.GetColumn(columnId));

          if (index != null) {
            // Column is non key column
            if (reader.GetBoolean(14))
              index.NonkeyColumns.Add(table.GetColumn(columnId));
            else
              index.CreateIndexColumn(table.GetColumn(columnId), !reader.GetBoolean(13));
          }
        }
    }

    private void ExtractForeignKeys()
    {
      string query = "select fk.schema_id, fk.object_id, fk.name, fk.delete_referential_action, fk.update_referential_action, fkc.constraint_column_id, fkc.parent_object_id, fkc.parent_column_id, fkc.referenced_object_id, fkc.referenced_column_id from sys.foreign_keys fk inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id";
      if (Schema!=null)
        query += " where fk.schema_id = " + schemaId;
      query += " order by fk.schema_id, fkc.parent_object_id, fk.object_id, fkc.constraint_column_id";

      int tableId = 0, constraintId = 0;
      DataTableProxy referencingTable = null;
      DataTableProxy referencedTable = null;
      ForeignKey foreignKey = null;
      using (var cmd = CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          // First column in constraint => new constraint
          if (reader.GetInt32(5) == 1) {
            GetDataTable(reader.GetInt32(6), ref tableId, ref referencingTable);
            foreignKey = ((Table)referencingTable.Table).CreateForeignKey(reader.GetString(2));
            referencedTable = dataTableIndex[reader.GetInt32(8)];
            foreignKey.ReferencedTable = (Table) referencedTable.Table;
            foreignKey.OnDelete = GetReferentialAction(reader.GetByte(3));
            foreignKey.OnUpdate = GetReferentialAction(reader.GetByte(4));
          }

          foreignKey.Columns.Add((TableColumn) referencingTable.GetColumn(reader.GetInt32(7)));
          foreignKey.ReferencedColumns.Add((TableColumn) referencedTable.GetColumn(reader.GetInt32(9)));
        }
      }
    }

    private static ReferentialAction GetReferentialAction(int actionCode)
    {
      switch (actionCode) {
        case 0:
          return ReferentialAction.NoAction;
        case 1:
          return ReferentialAction.Cascade;
        case 2:
          return ReferentialAction.SetNull;
      }
      return ReferentialAction.SetDefault;
    }

    // Do not touch this!!! The author is Denis Krjuchkov
    private SqlValueType GetValueType(string dataType, int numericPrecision, int numericScale, int maxLength)
    {
      var typeName = dataType;
      var typeInfo = Connection.Driver.ServerInfo.DataTypes[typeName];
      SqlType type;
      if (typeInfo!=null) {
        type = typeInfo.Type;
        typeName = null;
      }
      else
        type = SqlType.Unknown;

      int? precision = numericPrecision;
      int? scale = numericScale;

      if (typeInfo!=null && typeInfo.MaxPrecision==null) {
        // reseting precision & scale for types that do not require specifying them
        precision = null;
        scale = null;
      }

      if (numericPrecision > 0 || numericScale > 0)
        maxLength = 0;

      int? size = maxLength;
      if (size <= 0) {
        size = null;
        switch (type) {
        case SqlType.VarChar:
          type = SqlType.VarCharMax;
          break;
        case SqlType.VarBinary:
          type = SqlType.VarBinaryMax;
          break;
        }
      }
      if (typeInfo!=null) {
        if (typeInfo.MaxLength==null) {
          // reseting length for types that do not require specifying it
          size = null;
        }
        else if (size != null && size > 0)
          switch (type) {
            case SqlType.Char:
            case SqlType.VarChar:
            case SqlType.VarCharMax:
              size /= 2;
            break;
          }
      }

      return new SqlValueType(type, typeName, size, precision, scale);
    }

    private void GetSchema(int id, ref int currentId, ref Schema currentObj)
    {
      if ((Schema!=null && id == currentId)) {
        currentObj = Schema;
        return;
      }

      if (id == currentId) {
        return;
      }

      currentObj = schemaIndex[id];
      currentId = id;
    }

    private void GetDataTable(int id, ref int currentId, ref DataTableProxy currentObj)
    {
      if (id == currentId)
        return;

      currentObj = dataTableIndex[id];
      currentId = id;
    }


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}