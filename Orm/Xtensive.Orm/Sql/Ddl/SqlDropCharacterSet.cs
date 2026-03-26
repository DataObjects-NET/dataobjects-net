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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropCharacterSet(CharacterSet);

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
