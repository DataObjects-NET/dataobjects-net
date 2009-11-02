// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlTableScanHint : SqlTableHint
  {
    private readonly SqlTableScanMethod scanMethod;
    private readonly Index[] indexes;
    private readonly string[] indexNames;

    /// <summary>
    /// Gets the table scan method.
    /// </summary>
    /// <value>The table scan method.</value>
    public SqlTableScanMethod ScanMethod
    {
      get { return scanMethod; }
    }

    /// <summary>
    /// Gets the indexes.
    /// </summary>
    /// <value>The indexes.</value>
    public Index[] Indexes
    {
      get { return indexes; }
    }

    /// <summary>
    /// Gets the index names.
    /// </summary>
    /// <value>The index names.</value>
    public string[] IndexNames
    {
      get { return indexNames; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      SqlTableScanHint clone;
      if (indexes!=null)
         clone = new SqlTableScanHint(SqlTable, scanMethod, indexes);
      else
        clone = new SqlTableScanHint(SqlTable, scanMethod, indexNames);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlTableScanHint(SqlTable sqlTable, SqlTableScanMethod scanMethod, params Index[] indexes) : this(sqlTable, scanMethod)
    {
      this.indexes = indexes;
    }

    internal SqlTableScanHint(SqlTable sqlTable, SqlTableScanMethod scanMethod, params string[] indexNames) : this(sqlTable, scanMethod)
    {
      this.indexNames = indexNames;
    }

    internal SqlTableScanHint(SqlTable sqlTable, SqlTableScanMethod scanMethod) : base(sqlTable)
    {
      this.scanMethod = scanMethod;
    }

  }
}
