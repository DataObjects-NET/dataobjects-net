// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public abstract class SqlAction : SqlNode
  {
    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      throw new NotSupportedException();
    }

    protected SqlAction() : base(SqlNodeType.Action) { }
  }
}
