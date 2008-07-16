// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public sealed class SelectProvider : TransparentProvider
  {
    private readonly RecordHeader header;

    public override IEnumerator<Tuple> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }


    // Constructors

    public SelectProvider(RecordHeader header, Provider source, int[] columnIndexes)
      : base(header, source)
    {
    }
  }
}