// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlAlterIdentityInfo : SqlAction
  {
    private TableColumn column;
    private SequenceDescriptor sequenceDescriptor;
    private SqlAlterIdentityInfoOptions infoOption = SqlAlterIdentityInfoOptions.All;

    public TableColumn Column {
      get {
        return column;
      }
    }

    public SequenceDescriptor SequenceDescriptor {
      get {
        return sequenceDescriptor;
      }
      set {
        sequenceDescriptor = value;
      }
    }

    public SqlAlterIdentityInfoOptions InfoOption {
      get {
        return infoOption;
      }
      set {
        infoOption = value;
      }
    }
    
    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAlterIdentityInfo clone = new SqlAlterIdentityInfo(column, (SequenceDescriptor)sequenceDescriptor.Clone(), infoOption);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlAlterIdentityInfo(TableColumn column, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      this.column = column;
      this.sequenceDescriptor = sequenceDescriptor;
      this.infoOption = infoOption;
    }
  }
}
