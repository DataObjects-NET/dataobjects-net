// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropConstraint : SqlCascadableAction
  {
    public Constraint Constraint { get; private set; }
    
    internal override SqlDropConstraint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropConstraint(t.Constraint, t.Cascade));

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
