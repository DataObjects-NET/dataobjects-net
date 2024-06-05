// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropColumn : SqlCascadableAction
  {
    public TableColumn Column { get; private set; }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropColumn(Column);

    // Constructors

    internal SqlDropColumn(TableColumn column)
      : base(true)
    {
      Column = column;
    }

    internal SqlDropColumn(TableColumn column, bool cascade)
      : base(cascade)
    {
      Column = column;
    }
  }
}
