// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropTranslation(translation);

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
