// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlUserFunctionCall : SqlFunctionCall
  {
    /// <summary>
    /// Gets the function name.
    /// </summary>
    public string Name { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      SqlUserFunctionCall replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlUserFunctionCall>(expression);
      Name = replacingExpression.Name;
      Arguments = replacingExpression.Arguments;
    }

    internal override SqlUserFunctionCall Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlUserFunctionCall(t.Name, t.Arguments.Select(o => o.Clone(c)).ToArray(t.Arguments.Count)));

    internal SqlUserFunctionCall(string name, IEnumerable<SqlExpression> arguments)
      : base(SqlFunctionType.UserDefined, arguments)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
    }

    internal SqlUserFunctionCall(string name, params SqlExpression[] arguments)
      : base(SqlFunctionType.UserDefined, arguments)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
