// Copyright (C) 2003-2010 Xtensive LLC.
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

    // Constructors

    protected SqlAction() : base(SqlNodeType.Action)
    {
    }
  }
}
