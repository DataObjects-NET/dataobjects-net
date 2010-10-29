// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.29

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlFreeTextTable : SqlTable, ISqlQueryExpression
  {
    public SqlTableRef TargetTable { get; private set; }

    public SqlTableColumnCollection TargetColumns { get; private set; }

    public SqlExpression FreeText { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      throw new NotImplementedException();
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public new IEnumerator<ISqlQueryExpression> GetEnumerator()
    {
      yield return this;
    }

    public SqlQueryExpression Except(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }

    public SqlQueryExpression ExceptAll(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }

    public SqlQueryExpression Intersect(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }

    public SqlQueryExpression IntersectAll(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }

    public SqlQueryExpression Union(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }

    public SqlQueryExpression UnionAll(ISqlQueryExpression operand)
    {
      throw new NotImplementedException();
    }


    // Constructors

    internal SqlFreeTextTable(DataTable dataTable, SqlExpression freeText, IEnumerable<string> columnNames)
      : this(dataTable, freeText, columnNames, ArrayUtils<string>.EmptyArray)
    {
    }

    internal SqlFreeTextTable(DataTable dataTable, SqlExpression freeText, IEnumerable<string> columnNames, ICollection<string> targetColumnNames)
      : base(string.Empty)
    {
      TargetTable = SqlDml.TableRef(dataTable);
      FreeText = freeText;
      var targetColumns = new List<SqlTableColumn>();
      if (targetColumnNames.Count == 0)
        targetColumns.Add(Asterisk);
      else
        targetColumns = targetColumnNames.Select(cn => SqlDml.TableColumn(this, cn)).ToList();
      TargetColumns = new SqlTableColumnCollection(targetColumns);

      columns = new SqlTableColumnCollection(columnNames.Select(column=>SqlDml.TableColumn(this, column)).ToList());
    }
  }
}