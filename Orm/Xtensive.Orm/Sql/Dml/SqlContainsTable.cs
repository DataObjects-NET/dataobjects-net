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


    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, ICollection<string> columnNames)
      : this(dataTable, freeText, columnNames, Array.Empty<string>(), null)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, ICollection<string> columnNames, ICollection<string> targetColumnNames)
      : this(dataTable, freeText, columnNames, targetColumnNames, null)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression freeText, ICollection<string> columnNames, SqlExpression topN)
      : this(dataTable, freeText, columnNames, Array.Empty<string>(), topN)
    {
    }

    internal SqlContainsTable(DataTable dataTable, SqlExpression searchCondition, ICollection<string> columnNames, ICollection<string> targetColumnNames, SqlExpression topNByRank)
      : base(string.Empty)
    {
      TargetTable = SqlDml.TableRef(dataTable);
      SearchCondition = searchCondition;
      TopNByRank = topNByRank;
      var targetColumnCount = targetColumnNames.Count;
      if (targetColumnCount == 0) {
        TargetColumns = new SqlTableColumnCollection(new List<SqlTableColumn>(1) {Asterisk});
      }
      else {
        var targetColumns = new List<SqlTableColumn>(targetColumnCount);
        targetColumns.AddRange(targetColumnNames.Select(columnName => SqlDml.TableColumn(this, columnName)));
        TargetColumns = new SqlTableColumnCollection(targetColumns);
      }

      var columnList = new List<SqlTableColumn>(columnNames.Count);
      columnList.AddRange(columnNames.Select(columnName => SqlDml.TableColumn(this, columnName)));
      columns = new SqlTableColumnCollection(columnList);
    }
  }
}
