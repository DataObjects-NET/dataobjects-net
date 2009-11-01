// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlDropConstraint : SqlAction
  {
    private Constraint constraint;
    private bool cascade;

    public Constraint Constraint {
      get {
        return constraint;
      }
    }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }


    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropConstraint clone = new SqlDropConstraint(constraint);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlDropConstraint(Constraint constraint)
    {
      this.constraint = constraint;
    }

    internal SqlDropConstraint(Constraint constraint, bool cascade)
    {
      this.constraint = constraint;
      this.cascade = cascade;
    }
  }
}
