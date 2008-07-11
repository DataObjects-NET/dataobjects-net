// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System.Collections.Generic;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse
{
  public class KeyInfo : LockableBase
  {
    private RecordColumnCollection keyColumns;

    public RecordColumnCollection KeyColumns
    {
      get { return keyColumns; }
      set { keyColumns = value; }
    }

    
    // Constructors

    public KeyInfo(IEnumerable<RecordColumn> keyColumns)
    {
       this.keyColumns = new RecordColumnCollection(keyColumns, "KeyColumns");
    }
  }
}