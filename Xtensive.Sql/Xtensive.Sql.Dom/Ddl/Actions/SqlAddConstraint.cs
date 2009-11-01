// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlAddConstraint : SqlAction
  {
    private Constraint constraint;

    public Constraint Constraint {
      get {
        return constraint;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAddConstraint clone = new SqlAddConstraint(constraint);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlAddConstraint(Constraint constraint)
    {
      this.constraint = constraint;
    }
  }
}
