// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.ObjectModel;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Describes a reference to <see cref="Table"/> object;
  /// </summary>
  [Serializable]
  public class SqlTableRef : SqlTable
  {
    private readonly DataTable dataTable;

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

      SqlTableRef clone;
      clone = new SqlTableRef(dataTable, Name);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    internal SqlTableRef(DataTable dataTable) : this(dataTable, string.Empty)
    {
    }

    internal SqlTableRef(DataTable dataTable, string name) : base(name)
    {
      this.dataTable = dataTable;
      Collection<SqlTableColumn> tableColumns = new Collection<SqlTableColumn>();
      foreach (DataTableColumn c in dataTable.Columns)
        tableColumns.Add(Sql.TableColumn(this, c.Name));
      columns = new SqlTableColumnCollection(tableColumns);
    }
  }
}