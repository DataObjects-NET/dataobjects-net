// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropSchema : SqlStatement, ISqlCompileUnit
  {
    private Schema schema;
    private bool cascade = true;

    public Schema Schema {
      get {
        return schema;
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

    internal override SqlDropSchema Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropSchema(t.schema));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropSchema(Schema schema) : base(SqlNodeType.Drop)
    {
      this.schema = schema;
    }

    internal SqlDropSchema(Schema schema, bool cascade) : base(SqlNodeType.Drop)
    {
      this.schema = schema;
      this.cascade = cascade;
    }
  }
}
