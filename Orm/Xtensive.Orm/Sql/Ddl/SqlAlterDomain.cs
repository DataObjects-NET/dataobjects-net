// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterDomain : SqlStatement, ISqlCompileUnit
  {
    private SqlAction action;
    private Domain domain;

    public SqlAction Action {
      get {
        return action;
      }
    }

    public Domain Domain {
      get {
        return domain;
      }
    }


    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAlterDomain clone = new SqlAlterDomain(domain, (SqlAction)action.Clone(context));
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterDomain(Domain domain, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      this.action = action;
      this.domain = domain;
    }
  }
}
