// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateAssertion : SqlStatement, ISqlCompileUnit
  {
    private Assertion assertion;

    public Assertion Assertion {
      get {
        return assertion;
      }
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlCreateAssertion(assertion);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateAssertion(Assertion assertion) : base(SqlNodeType.Create)
    {
      this.assertion = assertion;
    }
  }
}
