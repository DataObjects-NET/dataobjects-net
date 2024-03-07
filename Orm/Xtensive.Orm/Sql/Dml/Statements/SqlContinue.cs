// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlContinue : SqlStatement
  {
    internal override SqlContinue Clone(SqlNodeCloneContext context) => this;

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
