// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateTranslation : SqlStatement, ISqlCompileUnit
  {
    public Translation Translation { get; }

    internal override SqlCreateTranslation Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateTranslation(t.Translation));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateTranslation(Translation translation) : base(SqlNodeType.Create)
    {
      Translation = translation;
    }
  }
}
