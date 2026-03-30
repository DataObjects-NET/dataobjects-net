// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropCharacterSet : SqlStatement, ISqlCompileUnit
  {
    public CharacterSet CharacterSet { get; }

    internal override SqlDropCharacterSet Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropCharacterSet(t.CharacterSet));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropCharacterSet(CharacterSet characterSet) : base(SqlNodeType.Drop)
    {
      CharacterSet = characterSet;
    }
  }
}
