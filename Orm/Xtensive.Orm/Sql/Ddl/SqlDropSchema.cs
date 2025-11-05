// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropSchema : SqlStatement, ISqlCompileUnit
  {
    private bool cascade = true;

    public Schema Schema { get; }

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
        new SqlDropSchema(t.Schema));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropSchema(Schema schema) : base(SqlNodeType.Drop)
    {
      Schema = schema;
    }

    internal SqlDropSchema(Schema schema, bool cascade) : base(SqlNodeType.Drop)
    {
      Schema = schema;
      this.cascade = cascade;
    }
  }
}
