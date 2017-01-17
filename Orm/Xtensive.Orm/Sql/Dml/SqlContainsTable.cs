using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlContainsTable: SqlTable, ISqlQueryExpression
  {
    public SqlTableRef TargetTable { get; private set; }

    public SqlTableColumnCollection TargetColumns { get; private set; }

    public SqlExpression SearchCondition { get; private set; }

    public SqlExpression TopNByRank { get; private set; }

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


    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, IEnumerable<string> columnNames)
      : this(dataTable, freeText, columnNames, ArrayUtils<string>.EmptyArray, null)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, IEnumerable<string> columnNames, ICollection<string> targetColumnNames)
      : this(dataTable, freeText, columnNames, targetColumnNames, null)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, IEnumerable<string> columNames, SqlExpression topN)
      : this(dataTable, freeText, columNames, ArrayUtils<string>.EmptyArray, topN)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression searchCondition, IEnumerable<string> columnNames, ICollection<string> targetColumnNames, SqlExpression topNByRank)
      : base(string.Empty)
    {
      TargetTable = SqlDml.TableRef(dataTable);
      SearchCondition = searchCondition;
      TopNByRank = topNByRank;
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
