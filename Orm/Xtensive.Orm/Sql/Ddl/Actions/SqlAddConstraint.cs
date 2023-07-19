// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAddConstraint : SqlAction
  {
    public Constraint Constraint { get; private set; }

    internal override SqlAddConstraint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAddConstraint(t.Constraint));

    // Constructors

    internal SqlAddConstraint(Constraint constraint)
    {
      Constraint = constraint;
    }
  }
}
