// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateSchema : SqlStatement, ISqlCompileUnit
  {
    private Schema schema;

    public Schema Schema {
      get {
        return schema;
      }
    }

    internal override SqlCreateSchema Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateSchema(t.schema));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateSchema(Schema schema) : base(SqlNodeType.Create)
    {
      this.schema = schema;
    }
  }
}
