// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateDomain : SqlStatement, ISqlCompileUnit
  {
    private Domain domain;

    public Domain Domain {
      get {
        return domain;
      }
    }

    internal override SqlCreateDomain Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateDomain(t.domain));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateDomain(Domain domain) : base(SqlNodeType.Create)
    {
      this.domain = domain;
    }
  }
}
