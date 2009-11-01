// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlContinue : SqlStatement
  {
    internal override object Clone(SqlNodeCloneContext context)
    {
      return this;
    }

    internal SqlContinue()
      : base(SqlNodeType.Continue)
    {
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
