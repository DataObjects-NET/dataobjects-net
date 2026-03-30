// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateCharacterSet : SqlStatement, ISqlCompileUnit
  {
    public CharacterSet CharacterSet { get; }

    internal override SqlCreateCharacterSet Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateCharacterSet(t.CharacterSet));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateCharacterSet(CharacterSet characterSet) : base(SqlNodeType.Create)
    {
      this.CharacterSet = characterSet;
    }
  }
}
