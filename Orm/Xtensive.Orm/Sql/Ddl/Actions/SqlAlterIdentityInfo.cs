// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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
