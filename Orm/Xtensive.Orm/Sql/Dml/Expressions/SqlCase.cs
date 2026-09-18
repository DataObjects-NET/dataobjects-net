// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlCase: SqlExpression, IEnumerable<KeyValuePair<SqlExpression, SqlExpression>>
  {
    private readonly IList<KeyValuePair<SqlExpression, SqlExpression>> cases = new List<KeyValuePair<SqlExpression, SqlExpression>>();
    private SqlExpression value;
    private SqlExpression @else;

    public int Count
    {
      get { return cases.Count; }
    }

    public SqlExpression this[SqlExpression key]
    {
      get
      {
        int index = IndexOf(key);
        return index >= 0 ? cases[index].Value : null;
      }
      set
      {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        int index = IndexOf(key);
        KeyValuePair<SqlExpression, SqlExpression> @case = new KeyValuePair<SqlExpression, SqlExpression>(key, value);
        if (index >= 0)
          cases[index] = @case;
        else 
          cases.Add(@case);
      }
    }

    public SqlExpression Value
    {
      get { return value; }
      set
      {
        SqlValidator.EnsureIsArithmeticExpression(value);
        this.value = value;
      }
    }

    public SqlExpression Else
    {
      get { return @else; }
      set { @else = value; }
    }

    public SqlCase Add(SqlExpression key, SqlExpression value)
    {
      ArgumentNullException.ThrowIfNull(key);
      ArgumentNullException.ThrowIfNull(value);
      cases.Add(new KeyValuePair<SqlExpression, SqlExpression>(key, value));
      return this;
    }

    public int IndexOf(SqlExpression key)
    {
      int result = 0;
      foreach (KeyValuePair<SqlExpression, SqlExpression> pair in cases) {
        if ((object) pair.Key == (object) key)
          return result;
        result++;
      }
      return -1;
    }

    public void Clear()
    {
      cases.Clear();
      value = null;
      @else = null;
    }

    public bool Remove(SqlExpression key)
    {
      for (int i = 0; i < cases.Count; i++)
        if ((object) cases[i] == (object) key) {
          cases.RemoveAt(i);
          return true;
        }
      return false;
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlCase>(expression);
      value = replacingExpression.Value;
      @else = replacingExpression.Else;
      cases.Clear();
      foreach (KeyValuePair<SqlExpression, SqlExpression> pair in replacingExpression)
        cases.Add(pair);
    }

    internal override SqlCase Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlCase(t.value?.Clone(c));

        if (t.@else is not null)
          clone.Else = t.@else.Clone(c);

        foreach (KeyValuePair<SqlExpression, SqlExpression> pair in t.cases)
          clone[pair.Key.Clone(c)] = pair.Value.Clone(c);

        return clone;
      });

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    IEnumerator<KeyValuePair<SqlExpression, SqlExpression>> IEnumerable<KeyValuePair<SqlExpression, SqlExpression>>.
      GetEnumerator()
    {
      foreach (KeyValuePair<SqlExpression, SqlExpression> pair in cases) {
        yield return pair;
      }
    }

    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<SqlExpression, SqlExpression>>)this).GetEnumerator();
    }

    internal SqlCase(SqlExpression value)
      : base(SqlNodeType.Case)
    {
      this.value = value;
    }
  }
}