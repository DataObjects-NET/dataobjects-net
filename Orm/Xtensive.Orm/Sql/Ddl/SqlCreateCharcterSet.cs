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
    private CharacterSet characterSet;

    public CharacterSet CharacterSet {
      get {
        return characterSet;
      }
    }

    internal override SqlCreateCharacterSet Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateCharacterSet(t.characterSet));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateCharacterSet(CharacterSet characterSet) : base(SqlNodeType.Create)
    {
      this.characterSet = characterSet;
    }
  }
}
