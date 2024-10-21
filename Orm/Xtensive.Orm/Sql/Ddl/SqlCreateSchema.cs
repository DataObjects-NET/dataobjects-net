// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateSchema : SqlStatement, ISqlCompileUnit
  {
    public Schema Schema { get; }

    internal override SqlCreateSchema Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateSchema(t.Schema));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateSchema(Schema schema) : base(SqlNodeType.Create)
    {
      Schema = schema;
    }
  }
}
