// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropDomain : SqlStatement, ISqlCompileUnit
  {
    private Domain domain;
    private bool cascade = true;

    public Domain Domain {
      get {
        return domain;
      }
    }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override SqlDropDomain Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropDomain(t.domain));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropDomain(Domain domain) : base(SqlNodeType.Drop)
    {
      this.domain = domain;
    }

    internal SqlDropDomain(Domain domain, bool cascade) : base(SqlNodeType.Drop)
    {
      this.domain = domain;
      this.cascade = cascade;
    }
  }
}
