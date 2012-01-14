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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropDomain clone = new SqlDropDomain(domain);
      context.NodeMapping[this] = clone;

      return clone;
    }

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
