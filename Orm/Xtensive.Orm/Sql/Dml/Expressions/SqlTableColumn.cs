// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Defines a reference to <see cref="DataTableColumn"/> object
  /// </summary>
  [Serializable]
  public class SqlTableColumn : SqlColumn, ISqlLValue
  {
    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = (SqlColumn) expression;
      ArgumentValidator.EnsureArgumentNotNull(replacingExpression.SqlTable, "SqlTable");
      ArgumentValidator.EnsureArgumentNotNull(replacingExpression.Name, "Name");
      base.ReplaceWith(expression);
    }

    internal override SqlTableColumn Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var table = t.SqlTable;
        SqlNode clonedTable;
        if (c.NodeMapping.TryGetValue(t.SqlTable, out clonedTable)) {
          table = (SqlTable) clonedTable;
        }
      
        var clone = new SqlTableColumn(table, t.Name);
        return clone;
      });

    // Constructors

    internal SqlTableColumn(SqlTable sqlTable, string name)
      : base(sqlTable, name)
    {
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
