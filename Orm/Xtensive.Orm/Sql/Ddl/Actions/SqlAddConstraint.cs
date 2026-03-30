// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
