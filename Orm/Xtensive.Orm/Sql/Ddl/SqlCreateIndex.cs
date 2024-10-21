// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateIndex : SqlStatement, ISqlCompileUnit
  {
    public Index Index { get; }

    internal override SqlCreateIndex Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateIndex(t.Index));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
    
    // Constructors

    internal SqlCreateIndex(Index index)
      : base(SqlNodeType.Create)
    {
      Index = index;
    }
  }
}
