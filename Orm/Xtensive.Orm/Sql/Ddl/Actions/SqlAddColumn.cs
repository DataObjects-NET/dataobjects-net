// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAddColumn : SqlAction
  {
    public TableColumn Column { get; private set; }

    internal override SqlAddColumn Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAddColumn(t.Column));

    // Constructors

    internal SqlAddColumn(TableColumn column)
    {
      Column = column;
    }
  }
}
