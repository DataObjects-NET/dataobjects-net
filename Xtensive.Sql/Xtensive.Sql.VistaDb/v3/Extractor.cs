// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using VistaDB.Provider;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.VistaDb.v3
{
  internal class Extractor: Model.Extractor
  {
    private VistaDBConnection vdbConnnection;
    private Catalog catalog;

    protected override void Initialize()
    {
      vdbConnnection = (VistaDBConnection) Connection.UnderlyingConnection;
    }

    public override Catalog ExtractCatalog()
    {
      catalog = new Catalog(Connection.Url.Resource);
      ExtractSchemas();
      foreach (var schema in catalog.Schemas) {
        ExtractTables(schema);
        ExtractViews(schema);
        ExtractColumns(schema);
        ExtractUniqueConstraints(schema);
        ExtractIndexes(schema);
      }
      ExtractForeignKeys();
      return catalog;
    }

    public override Schema ExtractSchema(string name)
    {
      throw new NotImplementedException();
    }

    private void ExtractSchemas()
    {
      Schema schema = catalog.CreateSchema("dbo");
      schema.Owner = "dbo";
    }

    private void ExtractViews(Schema schema)
    {
      System.Data.DataTable vdbView = vdbConnnection.GetSchema("VIEWS");
      for (int i = 0; i < vdbView.Rows.Count; i++) {
        string viewName = vdbView.Rows[i].ItemArray[vdbView.Columns.IndexOf("TABLE_NAME")].ToString();
        string viewDescr = vdbView.Rows[i].ItemArray[vdbView.Columns.IndexOf("TABLE_DESCRIPTION")].ToString();
        schema.CreateView(viewName, SqlDml.Native(viewDescr));
      }
    }

    private void ExtractTables(Schema schema)
    {
      System.Data.DataTable vdbTable = vdbConnnection.GetSchema("TABLES");
      int tableNameIndex = vdbTable.Columns.IndexOf("TABLE_NAME");
      for (int i = 0; i < vdbTable.Rows.Count; i++) {
        string tableName = vdbTable.Rows[i].ItemArray[tableNameIndex].ToString();
        schema.CreateTable(tableName);
      }
    }

    private void ExtractColumns(Schema schema)
    {
      System.Data.DataTable vdbColumns = vdbConnnection.GetSchema("COLUMNS");
      int tableNameIndex = vdbColumns.Columns.IndexOf("TABLE_NAME");
      int columnNameIndex = vdbColumns.Columns.IndexOf("COLUMN_NAME");
      int dataTypeIndex = vdbColumns.Columns.IndexOf("DATA_TYPE");
      int charMaxLengthIndex = vdbColumns.Columns.IndexOf("CHARACTER_MAXIMUM_LENGTH");
      int precisionIndex = vdbColumns.Columns.IndexOf("NUMERIC_PRECISION");
      int scaleIndex = vdbColumns.Columns.IndexOf("NUMERIC_SCALE");
      int isNullableIndex = vdbColumns.Columns.IndexOf("IS_NULLABLE");
      int collationNameIndex = vdbColumns.Columns.IndexOf("COLLATION_NAME");
      int columnDefaultIndex = vdbColumns.Columns.IndexOf("COLUMN_DEFAULT");
      for (int i = 0; i < vdbColumns.Rows.Count; i++) {
        string tableName = vdbColumns.Rows[i].ItemArray[tableNameIndex].ToString();
        string columnName = vdbColumns.Rows[i].ItemArray[columnNameIndex].ToString();
        // dataType and size
        string dataType = vdbColumns.Rows[i].ItemArray[dataTypeIndex].ToString().ToLower();
        string charSet = vdbColumns.Rows[i].ItemArray[charMaxLengthIndex].ToString();

        // precision
        string pres = vdbColumns.Rows[i].ItemArray[precisionIndex].ToString();
        byte precision = 0;
        if (pres != "" || Convert.IsDBNull(pres))
          precision = Convert.ToByte(pres);

        // scale
        string sc = vdbColumns.Rows[i].ItemArray[scaleIndex].ToString();
        byte scale = 0;
        if (sc != "" || Convert.IsDBNull(sc))
          scale = Convert.ToByte(sc);

        DataTypeInfo dataTypeInfo = Connection.Driver.ServerInfo.DataTypes[dataType];
        SqlType sqlType = dataTypeInfo != null ? dataTypeInfo.Type : SqlType.Unknown;
        // dataType and size
        int size = 0;
        if ((sqlType == SqlType.Char) || (sqlType == SqlType.VarChar) || (sqlType == SqlType.Binary) ||
            (sqlType == SqlType.VarBinary)) {
          size = Convert.ToInt32(charSet);
          if (size == -1) {
            size = 0;
            switch (sqlType) {
            case SqlType.VarChar:
              sqlType = SqlType.VarCharMax;
              break;
            case SqlType.VarBinary:
              sqlType = SqlType.VarBinaryMax;
              break;
            }
          }
        }

        SqlValueType sqlDataType = size == 0
          ? new SqlValueType(sqlType, precision, scale)
          : new SqlValueType(sqlType, size);

        string isNull = vdbColumns.Rows[i].ItemArray[isNullableIndex].ToString().ToLower();
        string collation = vdbColumns.Rows[i].ItemArray[collationNameIndex].ToString();
        string defValue = vdbColumns.Rows[i].ItemArray[columnDefaultIndex].ToString();

        //create column
        Table table = schema.Tables[tableName];
        TableColumn column = table.CreateColumn(columnName, sqlDataType);
        column.IsNullable = isNull == "true";
        if (!string.IsNullOrEmpty(collation))
          column.Collation = schema.Collations[collation] ?? schema.CreateCollation(collation);
        column.DefaultValue = SqlDml.Native(defValue.Trim(')', '('));
      }

      // view columns
      System.Data.DataTable vdbViewColumns = vdbConnnection.GetSchema("VIEWCOLUMNS");
      int viewNameIndex = vdbViewColumns.Columns.IndexOf("VIEW_NAME");
      int viewColumnNameIndex = vdbViewColumns.Columns.IndexOf("COLUMN_NAME");
      for (int i = 0; i < vdbViewColumns.Rows.Count; i++) {
        string viewName = vdbViewColumns.Rows[i].ItemArray[viewNameIndex].ToString();
        string columnViewName = vdbViewColumns.Rows[i].ItemArray[viewColumnNameIndex].ToString();
        View view = schema.Views[viewName];
        view.CreateColumn(columnViewName);
      }
    }

    private void ExtractIndexes(Schema schema)
    {
      System.Data.DataTable vdbIndex = vdbConnnection.GetSchema("INDEXES");
      string columnsArr;
      string column;
      string[] columnsName;
      int tableNameIndex = vdbIndex.Columns.IndexOf("TABLE_NAME");
      int indexNameIndex = vdbIndex.Columns.IndexOf("INDEX_NAME");
      int expressionIndex = vdbIndex.Columns.IndexOf("EXPRESSION");
      int uniqueIndex = vdbIndex.Columns.IndexOf("UNIQUE");
      for (int i = 0; i < vdbIndex.Rows.Count; i++) {
        string tableName = vdbIndex.Rows[i].ItemArray[tableNameIndex].ToString();
        string indexName = vdbIndex.Rows[i].ItemArray[indexNameIndex].ToString();
        columnsArr = vdbIndex.Rows[i].ItemArray[expressionIndex].ToString();
        string unique = vdbIndex.Rows[i].ItemArray[uniqueIndex].ToString();
        columnsName = columnsArr.Split(';');

        Table table = schema.Tables[tableName];
        Index index = table.CreateIndex(indexName);
        for (int j = 0; j < columnsName.Length; j++) {
          column = columnsName[j];
          if (column.Contains("(") && column.Contains(")")) {
            String newCol = columnsName[j].Remove(0, 4).Trim('(', ')');
            index.CreateIndexColumn(table.TableColumns[newCol], false);
          }
          else
            index.CreateIndexColumn(table.TableColumns[column], true);
        }
        index.IsUnique = unique == "true";
      }
    }

    private ReferentialAction GetReferentialAction(string actionName)
    {
      switch (actionName) {
      case "Cascade":
        return ReferentialAction.Cascade;
      case "None":
        return ReferentialAction.NoAction;
      case "SetDefault":
        return ReferentialAction.SetDefault;
      case "SetNull":
        return ReferentialAction.SetNull;
      default:
        return ReferentialAction.NoAction;
      }
    }

    private void ExtractForeignKeys()
    {
      System.Data.DataTable vdbFK = vdbConnnection.GetSchema("FOREIGNKEYCOLUMNS");
      int constraintNameIndex = vdbFK.Columns.IndexOf("CONSTRAINT_NAME");
      int tableNameIndex = vdbFK.Columns.IndexOf("TABLE_NAME");
      int fKeyFromColumnNameIndex = vdbFK.Columns.IndexOf("FKEY_FROM_COLUMN");
      int fKeyToTableIndex = vdbFK.Columns.IndexOf("FKEY_TO_TABLE");
      int fKeyToColumnIndex = vdbFK.Columns.IndexOf("FKEY_TO_COLUMN");
      for (int i = 0; i < vdbFK.Rows.Count; i++) {
        string fkName = vdbFK.Rows[i].ItemArray[constraintNameIndex].ToString();
        string referencingTableName = vdbFK.Rows[i].ItemArray[tableNameIndex].ToString();
        string referencingColumnName = vdbFK.Rows[i].ItemArray[fKeyFromColumnNameIndex].ToString();
        string referencedTableName = vdbFK.Rows[i].ItemArray[fKeyToTableIndex].ToString();
        string referencedColumnName = vdbFK.Rows[i].ItemArray[fKeyToColumnIndex].ToString();

        Schema refShema = catalog.Schemas["dbo"];
        Table referencingTable = refShema.Tables[referencingTableName];
        Table referencedTable = refShema.Tables[referencedTableName];

        ForeignKey foreignKey = (ForeignKey)referencingTable.TableConstraints[fkName];
        if (foreignKey == null) {
          foreignKey = referencingTable.CreateForeignKey(fkName);
          foreignKey.ReferencedTable = referencedTable;
        }

        TableColumn referencingColumn = referencingTable.TableColumns[referencingColumnName];
        TableColumn referencedColumn = referencedTable.TableColumns[referencedColumnName];
        foreignKey.Columns.Add(referencingColumn);
        foreignKey.ReferencedColumns.Add(referencedColumn);
      }
    }

    private void ExtractUniqueConstraints(Schema schema)
    {
      System.Data.DataTable vdbIndex = vdbConnnection.GetSchema("INDEXES");
      int tableNameIndex = vdbIndex.Columns.IndexOf("TABLE_NAME");
      int indexNameIndex = vdbIndex.Columns.IndexOf("INDEX_NAME");
      int expressionIndex = vdbIndex.Columns.IndexOf("EXPRESSION");
      int uniqueIndex = vdbIndex.Columns.IndexOf("UNIQUE");
      string columnsArr;
      string[] columnsName;
      string column;
      for (int i = 0; i < vdbIndex.Rows.Count; i++) {
        string tableName = vdbIndex.Rows[i].ItemArray[tableNameIndex].ToString();
        string constrName = vdbIndex.Rows[i].ItemArray[indexNameIndex].ToString();
        columnsArr = vdbIndex.Rows[i].ItemArray[expressionIndex].ToString();
        string unique = vdbIndex.Rows[i].ItemArray[uniqueIndex].ToString().ToLower();

        Table table = schema.Tables[tableName];
        if (unique == "true") {
          UniqueConstraint uniqueConstraint = (UniqueConstraint)table.TableConstraints[constrName];
          if (uniqueConstraint == null)
            uniqueConstraint = table.CreateUniqueConstraint(constrName);
          columnsName = columnsArr.Split(';');

          for (int j = 0; j < columnsName.Length; j++) {
            column = columnsName[j];
            if (column.Contains("(") && column.Contains(")")) {
              String newCol = column.Remove(0, 4).Trim('(', ')');
              uniqueConstraint.Columns.Add(table.TableColumns[newCol]);
            }
            else
              uniqueConstraint.Columns.Add(table.TableColumns[column]);
          }
        }
      }
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}