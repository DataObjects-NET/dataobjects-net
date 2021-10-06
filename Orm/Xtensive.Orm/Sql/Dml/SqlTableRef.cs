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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : CreateClone(context);

    private SqlTableRef CreateClone(SqlNodeCloneContext context)
    {
      var clone = new SqlTableRef {Name = Name, DataTable = DataTable};
      context.NodeMapping[this] = clone;
      var columnClones = new List<SqlTableColumn>(columns.Count);
      columnClones.AddRange(columns.Select(column => (SqlTableColumn) column.Clone(context)));

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
      List<SqlTableColumn> tableColumns;
      if (columnNames.Length == 0) {
        tableColumns = new List<SqlTableColumn>(dataTable.Columns.Count);
        tableColumns.AddRange(dataTable.Columns.Select(column => SqlDml.TableColumn(this, column.Name)));
      }
      else {
        tableColumns = new List<SqlTableColumn>(columnNames.Length);
        tableColumns.AddRange(columnNames.Select(columnName => SqlDml.TableColumn(this, columnName)));
      }
      columns = new SqlTableColumnCollection(tableColumns);
    }
  }
}