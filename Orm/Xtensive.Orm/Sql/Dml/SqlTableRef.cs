// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Collections;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Describes a reference to <see cref="Table"/> object;
  /// </summary>
  [Serializable]
  public class SqlTableRef : SqlTable
  {
    /// <summary>
    /// Gets the name of the instance.
    /// </summary>
    /// <value>The name.</value>
    public override string Name => string.IsNullOrEmpty(base.Name) ? DataTable.DbName : base.Name;

    /// <summary>
    /// Gets the referenced table.
    /// </summary>
    /// <value>The table.</value>
    public DataTable DataTable { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this)) {
        return context.NodeMapping[this];
      }

      var clone = new SqlTableRef {Name = Name, DataTable = DataTable};
      context.NodeMapping[this] = clone;
      var index = 0;
      var columnClones = new SqlTableColumn[columns.Count];
      foreach (var column in columns) {
        columnClones[index++] = (SqlTableColumn) column.Clone(context);
      }

      clone.columns = new SqlTableColumnCollection(columnClones);

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor) => visitor.Visit(this);

    // Constructors

    private SqlTableRef()
    { }

    internal SqlTableRef(DataTable dataTable)
      : this(dataTable, string.Empty, Array.Empty<string>())
    { }

    internal SqlTableRef(DataTable dataTable, string name)
      : this(dataTable, name, Array.Empty<string>())
    { }

    internal SqlTableRef(DataTable dataTable, string name, params string[] columnNames)
      : base(name)
    {
      DataTable = dataTable;
      SqlTableColumn[] tableColumns;
      if (columnNames.Length == 0) {
        var index = 0;
        tableColumns = new SqlTableColumn[dataTable.Columns.Count];
        foreach (var column in dataTable.Columns) {
          tableColumns[index++] = SqlDml.TableColumn(this, column.Name);
        }
      }
      else {
        tableColumns = new SqlTableColumn[columnNames.Length];
        for (var index = 0; index < tableColumns.Length; index++) {
          tableColumns[index] = SqlDml.TableColumn(this, columnNames[index]);
        }
      }
      columns = new SqlTableColumnCollection(tableColumns);
    }
  }
}