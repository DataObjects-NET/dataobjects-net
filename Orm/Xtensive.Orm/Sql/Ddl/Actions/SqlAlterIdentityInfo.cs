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

    internal override SqlAlterIdentityInfo Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterIdentityInfo(t.Column, (SequenceDescriptor) t.SequenceDescriptor.Clone(), t.InfoOption));

    internal SqlAlterIdentityInfo(TableColumn column, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      Column = column;
      SequenceDescriptor = sequenceDescriptor;
      InfoOption = infoOption;
    }
  }
}
