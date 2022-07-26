// Copyright (C) 2003-2022 Xtensive LLC.
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
  public abstract class SqlFunctionCallBase : SqlExpression
  {
    /// <summary>
    /// Gets the expressions.
    /// </summary>
    public IReadOnlyList<SqlExpression> Arguments { get; protected set; }

    internal SqlFunctionCallBase(SqlNodeType nodeType, IEnumerable<SqlExpression> arguments)
      : base(nodeType)
    {
      Arguments = arguments.ToList();
    }

    internal SqlFunctionCallBase(SqlNodeType nodeType, SqlExpression[] arguments)
      : base(nodeType)
    {
      Arguments = arguments ?? Array.Empty<SqlExpression>();
    }
  }

  [Serializable]
  public class SqlFunctionCall : SqlFunctionCallBase
  {
    /// <summary>
    /// Gets the function type.
    /// </summary>
    public SqlFunctionType FunctionType { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlFunctionCall>(expression);
      FunctionType = replacingExpression.FunctionType;
      Arguments = replacingExpression.Arguments;
    }

    internal override SqlFunctionCall Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlFunctionCall(t.FunctionType, t.Arguments.Select(o => o.Clone(c)).ToArray(t.Arguments.Count)));

    public override void AcceptVisitor(ISqlVisitor visitor) => visitor.Visit(this);

    // Constructors

    internal SqlFunctionCall(SqlFunctionType functionType, IEnumerable<SqlExpression> arguments)
      : base(SqlNodeType.FunctionCall, arguments)
    {
      FunctionType = functionType;
    }

    internal SqlFunctionCall(SqlFunctionType functionType, params SqlExpression[] arguments)
      : base(SqlNodeType.FunctionCall, arguments)
    {
      FunctionType = functionType;
    }
  }
}
