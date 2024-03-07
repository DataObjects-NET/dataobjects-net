// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlBreak : SqlStatement
  {
    internal override SqlBreak Clone(SqlNodeCloneContext context) => this;

    internal SqlBreak()
      : base(SqlNodeType.Break)
    {
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
