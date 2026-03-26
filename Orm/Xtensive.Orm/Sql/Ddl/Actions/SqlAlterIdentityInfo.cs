// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterIdentityInfo : SqlAction
  {
    public TableColumn Column { get; private set; }
    public SequenceDescriptor SequenceDescriptor { get; set; }
    public SqlAlterIdentityInfoOptions InfoOption { get; set; }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlAlterIdentityInfo(Column, (SequenceDescriptor) SequenceDescriptor.Clone(), InfoOption);

    internal SqlAlterIdentityInfo(TableColumn column, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      Column = column;
      SequenceDescriptor = sequenceDescriptor;
      InfoOption = infoOption;
    }
  }
}
