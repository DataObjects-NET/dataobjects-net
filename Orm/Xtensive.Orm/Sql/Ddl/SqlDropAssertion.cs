// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropAssertion : SqlStatement, ISqlCompileUnit
  {
    private Assertion assertion;

    public Assertion Assertion {
      get {
        return assertion;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropAssertion clone = new SqlDropAssertion(assertion);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropAssertion(Assertion assertion) : base(SqlNodeType.Drop)
    {
      this.assertion = assertion;
    }
  }
}
