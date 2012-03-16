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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlAlterIdentityInfo(Column, (SequenceDescriptor) SequenceDescriptor.Clone(), InfoOption);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlAlterIdentityInfo(TableColumn column, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      Column = column;
      SequenceDescriptor = sequenceDescriptor;
      InfoOption = infoOption;
    }
  }
}
