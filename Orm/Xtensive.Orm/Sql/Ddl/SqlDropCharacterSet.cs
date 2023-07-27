// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropCharacterSet : SqlStatement, ISqlCompileUnit
  {
    private CharacterSet characterSet;

    public CharacterSet CharacterSet {
      get {
        return characterSet;
      }
    }

    internal override SqlDropCharacterSet Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropCharacterSet(t.characterSet));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropCharacterSet(CharacterSet characterSet) : base(SqlNodeType.Drop)
    {
      this.characterSet = characterSet;
    }
  }
}
