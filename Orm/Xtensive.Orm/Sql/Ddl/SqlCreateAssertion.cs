// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateAssertion : SqlStatement, ISqlCompileUnit
  {
    public Assertion Assertion { get; }

    internal override SqlCreateAssertion Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateAssertion(t.Assertion));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateAssertion(Assertion assertion) : base(SqlNodeType.Create)
    {
      Assertion = assertion;
    }
  }
}
