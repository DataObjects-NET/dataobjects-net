// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Orm.Internals
{
  [DebuggerDisplay("Header = {Header}")]
  internal sealed class RecordSetMapping
  {
    public Domain CurrentDomain { get; private set; }
    public RecordSetHeader Header { get; private set;}
    public ReadOnlyList<ColumnGroupMapping> Mappings { get; private set; }


    // Constructors

    public RecordSetMapping(Domain currentDomain, RecordSetHeader header, IList<ColumnGroupMapping> mappings)
    {
      CurrentDomain = currentDomain;
      Header = header;
      Mappings = new ReadOnlyList<ColumnGroupMapping>(mappings);
    }
  }
}