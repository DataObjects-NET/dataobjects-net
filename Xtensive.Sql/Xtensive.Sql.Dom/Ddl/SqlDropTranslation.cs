// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlDropTranslation : SqlStatement, ISqlCompileUnit
  {
    private Translation translation;

    public Translation Translation {
      get {
        return translation;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropTranslation clone = new SqlDropTranslation(translation);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropTranslation(Translation translation) : base(SqlNodeType.Drop)
    {
      this.translation = translation;
    }
  }
}
