// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropConstraint : SqlCascadableAction
  {
    public Constraint Constraint { get; private set; }
    
    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlDropConstraint(Constraint, Cascade);
      context.NodeMapping[this] = clone;

      return clone;
    }

    // Constructors

    internal SqlDropConstraint(Constraint constraint)
      : base(true)
    {
      Constraint = constraint;
    }

    internal SqlDropConstraint(Constraint constraint, bool cascade)
      : base(cascade)
    {
      Constraint = constraint;
    }
  }
}
