// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System.Collections.Generic;

namespace Xtensive.Storage.Rse
{
  public class KeyInfo
  {
    private readonly RecordColumnCollection columns;

    public RecordColumnCollection Columns
    {
      get { return columns; }
    }

    
    // Constructors

    public KeyInfo(IEnumerable<RecordColumn> columns)
    {
       this.columns = new RecordColumnCollection(columns, "columns");
    }
  }
}