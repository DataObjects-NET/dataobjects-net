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
    private DataTable dataTable;

    /// <summary>
    /// Gets the name of the instance.
    /// </summary>
    /// <value>The name.</value>
    public override string Name
    {
      get { return string.IsNullOrEmpty(base.Name) ? dataTable.DbName : base.Name; }
    }

    /// <summary>
    /// Gets the referenced table.
    /// </summary>
    /// <value>The table.</value>
    public DataTable DataTable
    {
      get { return dataTable; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlTableRef {Name = Name, dataTable = DataTable};
      context.NodeMapping[this] = clone;
      var columnClones = new Collection<SqlTableColumn>();
      foreach (var column in columns)
        columnClones.Add((SqlTableColumn) column.Clone(context));
      clone.columns = new SqlTableColumnCollection(columnClones);

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    private SqlTableRef()
    {
    }

    internal SqlTableRef(DataTable dataTable)
      : this(dataTable, string.Empty)
    {
    }

    internal SqlTableRef(DataTable dataTable, string name)
      : this(dataTable, name, ArrayUtils<string>.EmptyArray)
    {
      this.dataTable = dataTable;
      var tableColumns = new List<SqlTableColumn>();
      foreach (DataTableColumn c in dataTable.Columns)
        tableColumns.Add(SqlDml.TableColumn(this, c.Name));
      columns = new SqlTableColumnCollection(tableColumns);
    }

    internal SqlTableRef(DataTable dataTable, string name, params string[] columnNames)
      : base(name)
    {
      this.dataTable = dataTable;
      var tableColumns = columnNames.Length == 0 
        ? dataTable.Columns.Select(c => SqlDml.TableColumn(this, c.Name)).ToList() 
        : columnNames.Select(cn => SqlDml.TableColumn(this, cn)).ToList();
      columns = new SqlTableColumnCollection(tableColumns);
    }
  }
}