// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
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
