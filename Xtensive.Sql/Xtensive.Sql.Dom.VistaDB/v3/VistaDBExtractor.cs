// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using VistaDB.Provider;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Extractor;

namespace Xtensive.Sql.Dom.VistaDB.v3
{
  public class VistaDBExtractor: SqlExtractor
  {
    private VistaDBConnection vdbConnnection;

    /// <summary>
    /// Extracts the users.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="server">The server.</param>
    public override void ExtractUsers(SqlExtractorContext context, Server server)
    {
    }

    /// <summary>
    /// Extracts the schemas.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="catalog">The catalog.</param>
    public override void ExtractSchemas(SqlExtractorContext context, Catalog catalog)
    {
      Schema schema = catalog.CreateSchema("dbo");
      schema.Owner = catalog.Server.Users["dbo"];
    }

    /// <summary>
    /// Extracts the views.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractViews(SqlExtractorContext context, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      System.Data.DataTable vdbView = vdbConnnection.GetSchema("VIEWS");
      for (int i = 0; i < vdbView.Rows.Count; i++) {
        string viewName = vdbView.Rows[i].ItemArray[vdbView.Columns.IndexOf("TABLE_NAME")].ToString();
        string viewDescr = vdbView.Rows[i].ItemArray[vdbView.Columns.IndexOf("TABLE_DESCRIPTION")].ToString();
        schema.CreateView(viewName, Sql.Constant(viewDescr));
      }
    }

    /// <summary>
    /// Extracts the tables.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractTables(SqlExtractorContext context, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      System.Data.DataTable vdbTable = vdbConnnection.GetSchema("TABLES");
      int tableNameIndex = vdbTable.Columns.IndexOf("TABLE_NAME");
      for (int i = 0; i < vdbTable.Rows.Count; i++) {
        string tableName = vdbTable.Rows[i].ItemArray[tableNameIndex].ToString();
        Schema tableSchema = schema ?? context.Model.DefaultServer.DefaultCatalog.Schemas["dbo"];
        tableSchema.CreateTable(tableName);
      }
    }

    /// <summary>
    /// Extracts the columns.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractColumns(SqlExtractorContext context, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      string columnName, tableName, dataType, charSet, isNull, pres, sc, collation, defValue, viewName, columnViewName;
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
        tableName = vdbColumns.Rows[i].ItemArray[tableNameIndex].ToString();
        columnName = vdbColumns.Rows[i].ItemArray[columnNameIndex].ToString();
        // dataType and size
        dataType = vdbColumns.Rows[i].ItemArray[dataTypeIndex].ToString().ToLower();
        charSet = vdbColumns.Rows[i].ItemArray[charMaxLengthIndex].ToString();

        // precision
        pres = vdbColumns.Rows[i].ItemArray[precisionIndex].ToString();
        byte precision = 0;
        if (pres != "" || Convert.IsDBNull(pres))
          precision = Convert.ToByte(pres);

        // scale
        sc = vdbColumns.Rows[i].ItemArray[scaleIndex].ToString();
        byte scale = 0;
        if (sc != "" || Convert.IsDBNull(sc))
          scale = Convert.ToByte(sc);

        DataTypeInfo dataTypeInfo = context.Connection.Driver.ServerInfo.DataTypes[dataType];
        SqlDataType sqlType = dataTypeInfo != null ? dataTypeInfo.SqlType : SqlDataType.Unknown;
        // dataType and size
        int size = 0;
        if ((sqlType == SqlDataType.Char) || (sqlType == SqlDataType.AnsiChar) || (sqlType == SqlDataType.VarChar) ||
            (sqlType == SqlDataType.AnsiVarChar) || (sqlType == SqlDataType.Binary) ||
            (sqlType == SqlDataType.VarBinary)) {
          size = Convert.ToInt32(charSet);
          if (size == -1) {
            size = 0;
            switch (sqlType) {
              case SqlDataType.VarChar:
                sqlType = SqlDataType.VarCharMax;
                break;
              case SqlDataType.AnsiVarChar:
                sqlType = SqlDataType.AnsiVarCharMax;
                break;
              case SqlDataType.VarBinary:
                sqlType = SqlDataType.VarBinaryMax;
                break;
            }
          }
        }

        SqlValueType sqlDataType = size == 0
                                     ? new SqlValueType(sqlType, precision, scale)
                                     : new SqlValueType(sqlType, size);

        isNull = vdbColumns.Rows[i].ItemArray[isNullableIndex].ToString().ToLower();
        collation = vdbColumns.Rows[i].ItemArray[collationNameIndex].ToString();
        defValue = vdbColumns.Rows[i].ItemArray[columnDefaultIndex].ToString();

        //create column
        Table table = schema.Tables[tableName];
        TableColumn column = table.CreateColumn(columnName, sqlDataType);
        column.IsNullable = isNull == "true";
        if (!string.IsNullOrEmpty(collation))
          column.Collation = schema.Collations[collation] ?? schema.CreateCollation(collation);
        column.DefaultValue = Sql.Constant(defValue.Trim(')', '('));
      }

      // view columns
      System.Data.DataTable vdbViewColumns = vdbConnnection.GetSchema("VIEWCOLUMNS");
      int viewNameIndex = vdbViewColumns.Columns.IndexOf("VIEW_NAME");
      int viewColumnNameIndex = vdbViewColumns.Columns.IndexOf("COLUMN_NAME");
      for (int i = 0; i < vdbViewColumns.Rows.Count; i++) {
        viewName = vdbViewColumns.Rows[i].ItemArray[viewNameIndex].ToString();
        columnViewName = vdbViewColumns.Rows[i].ItemArray[viewColumnNameIndex].ToString();
        View view = schema.Views[viewName];
        view.CreateColumn(columnViewName);
      }
    }

    /// <summary>
    /// Extracts the indexes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractIndexes(SqlExtractorContext context, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      System.Data.DataTable vdbIndex = vdbConnnection.GetSchema("INDEXES");
      string tableName, indexName, columnsArr, unique;
      string column;
      string[] columnsName;
      int tableNameIndex = vdbIndex.Columns.IndexOf("TABLE_NAME");
      int indexNameIndex = vdbIndex.Columns.IndexOf("INDEX_NAME");
      int expressionIndex = vdbIndex.Columns.IndexOf("EXPRESSION");
      int uniqueIndex = vdbIndex.Columns.IndexOf("UNIQUE");
      for (int i = 0; i < vdbIndex.Rows.Count; i++) {
        tableName = vdbIndex.Rows[i].ItemArray[tableNameIndex].ToString();
        indexName = vdbIndex.Rows[i].ItemArray[indexNameIndex].ToString();
        columnsArr = vdbIndex.Rows[i].ItemArray[expressionIndex].ToString();
        unique = vdbIndex.Rows[i].ItemArray[uniqueIndex].ToString();
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

    public override void ExtractForeignKeys(SqlExtractorContext context, Schema schema)
    {
      ExtractForeignKeys(context, null, schema);
    }

    public override void ExtractForeignKeys(SqlExtractorContext context, Catalog catalog)
    {
      ExtractForeignKeys(context, catalog, null);
    }

    protected virtual ReferentialAction GetReferentialAction(string actionName)
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

    private void ExtractForeignKeys(SqlExtractorContext context, Catalog catalog, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      System.Data.DataTable vdbFK = vdbConnnection.GetSchema("FOREIGNKEYCOLUMNS");
      int constraintNameIndex = vdbFK.Columns.IndexOf("CONSTRAINT_NAME");
      int tableNameIndex = vdbFK.Columns.IndexOf("TABLE_NAME");
      int fKeyFromColumnNameIndex = vdbFK.Columns.IndexOf("FKEY_FROM_COLUMN");
      int fKeyToTableIndex = vdbFK.Columns.IndexOf("FKEY_TO_TABLE");
      int fKeyToColumnIndex = vdbFK.Columns.IndexOf("FKEY_TO_COLUMN");
      string fkName, referencingTableName, referencedTableName, referencingColumnName, referencedColumnName;
      for (int i = 0; i < vdbFK.Rows.Count; i++) {
        fkName = vdbFK.Rows[i].ItemArray[constraintNameIndex].ToString();
        referencingTableName = vdbFK.Rows[i].ItemArray[tableNameIndex].ToString();
        referencingColumnName = vdbFK.Rows[i].ItemArray[fKeyFromColumnNameIndex].ToString();
        referencedTableName = vdbFK.Rows[i].ItemArray[fKeyToTableIndex].ToString();
        referencedColumnName = vdbFK.Rows[i].ItemArray[fKeyToColumnIndex].ToString();

        Schema refShema = schema ?? catalog.Schemas["dbo"];
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

    /// <summary>
    /// Extracts the unique constraints.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractUniqueConstraints(SqlExtractorContext context, Schema schema)
    {
      vdbConnnection = (VistaDBConnection)context.Connection.RealConnection;
      System.Data.DataTable vdbIndex = vdbConnnection.GetSchema("INDEXES");
      int tableNameIndex = vdbIndex.Columns.IndexOf("TABLE_NAME");
      int indexNameIndex = vdbIndex.Columns.IndexOf("INDEX_NAME");
      int expressionIndex = vdbIndex.Columns.IndexOf("EXPRESSION");
      int uniqueIndex = vdbIndex.Columns.IndexOf("UNIQUE");
      string tableName, constrName, columnsArr, unique;
      string[] columnsName;
      string column;
      for (int i = 0; i < vdbIndex.Rows.Count; i++) {
        tableName = vdbIndex.Rows[i].ItemArray[tableNameIndex].ToString();
        constrName = vdbIndex.Rows[i].ItemArray[indexNameIndex].ToString();
        columnsArr = vdbIndex.Rows[i].ItemArray[expressionIndex].ToString();
        unique = vdbIndex.Rows[i].ItemArray[uniqueIndex].ToString().ToLower();

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

    /// <summary>
    /// Initialize the model
    /// </summary>
    /// <param name="connection">The connection</param>
    public override void Initialize(SqlConnection connection)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VistaDBExtractor"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal VistaDBExtractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}