// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Sql.Dml
{
  using ValuesDictionary = Dictionary<SqlColumn, List<SqlExpression>>;

  [Serializable]
  public class SqlInsert : SqlQueryStatement, ISqlCompileUnit
  {
    public class ValuesCollection
    {
      private readonly ValuesDictionary values = new();

      public int Count => values.Count;

      public ValuesDictionary.KeyCollection Columns => values.Keys;

      public IReadOnlyList<SqlExpression> ValuesByColumn(SqlColumn column) => values[column];

      public bool ContainsColumn(SqlColumn column) => values.ContainsKey(column);

      public bool ContainsValue(SqlExpression value) => values.Values.Any(list => list.Contains(value));

      public void SetValueByColumn(SqlColumn column, SqlExpression value) => values[column] = new List<SqlExpression> { value };

      public void Clear() => values.Clear();

      public void Add(SqlColumn column, SqlExpression value)
      {
        if (values.TryGetValue(column, out var list)) {
          list.Add(value);
        }
        else {
          SetValueByColumn(column, value);
        }
      }

      internal ValuesCollection Clone(SqlNodeCloneContext context) =>
        new ValuesCollection(values.ToDictionary(
          p => (SqlColumn) p.Key.Clone(context),
          p => {
            var list = new List<SqlExpression>(p.Value.Count);
            foreach (var value in p.Value) {
              list.Add(value is null ? null : (SqlExpression) value.Clone(context));
            }
            return list;
          }
        ));

      internal ValuesCollection(Dictionary<SqlColumn, List<SqlExpression>> values = null)
      {
        this.values = values ?? new Dictionary<SqlColumn, List<SqlExpression>>();
      }
    }

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Into { get; set; }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public ValuesCollection Values { get; private set; } = new();

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlSelect From { get; set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      SqlInsert clone = new SqlInsert();
      clone.Into = (SqlTableRef) Into?.Clone(context);
      clone.From = (SqlSelect) From?.Clone(context);
      clone.Values = Values.Clone(context);

      if (Hints.Count > 0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint) hint.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlInsert() : base(SqlNodeType.Insert)
    {
    }

    internal SqlInsert(SqlTableRef tableRef) : this()
    {
      Into = tableRef;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
