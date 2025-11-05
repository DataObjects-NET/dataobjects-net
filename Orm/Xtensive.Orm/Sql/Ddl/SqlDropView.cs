// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropView : SqlStatement, ISqlCompileUnit
  {
    private bool cascade = true;

    public View View { get; }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override SqlDropView Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropView(t.View));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropView(View view) : base(SqlNodeType.Drop)
    {
      View = view;
    }

    internal SqlDropView(View view, bool cascade) : base(SqlNodeType.Drop)
    {
      View = view;
      this.cascade = cascade;
    }
  }
}
