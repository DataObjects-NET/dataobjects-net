// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateCharacterSet : SqlStatement, ISqlCompileUnit
  {
    public CharacterSet CharacterSet { get; }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlCreateCharacterSet(CharacterSet);

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
