// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Internals
{
  [DebuggerDisplay("Header = {Header}")]
  internal sealed class RecordSetMapping
  {
    public RecordSetHeader Header { get; private set;}
    public ColumnGroupMapping[] Mappings { get; private set; }


    // Constructors

    public RecordSetMapping(RecordSetHeader header, IEnumerable<ColumnGroupMapping> mappings)
    {
      Header = header;
      Mappings = mappings.ToArray();
    }
  }
}