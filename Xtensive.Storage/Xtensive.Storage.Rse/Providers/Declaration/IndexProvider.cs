// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public sealed class IndexProvider : CompilableProvider
  {
    private readonly IndexInfo index;

    // TODO: replace with IndexInfoRef
    public IndexInfo Index
    {
      get { return index; }
    }

    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Indexed | ProviderOptions.Ordered; }
    }

    protected override RecordHeader BuildHeader()
    {
      DirectionCollection<ColumnInfo> orderingRule;
      if (index.IsUnique | index.IsPrimary)
        orderingRule = new DirectionCollection<ColumnInfo>(index.KeyColumns);
      else
        orderingRule = new DirectionCollection<ColumnInfo>(
          index.KeyColumns
            .Union(index.ValueColumns.Select(info => new KeyValuePair<ColumnInfo, Direction>(info, Direction.Positive))));
      var orderedBy = new DirectionCollection<int>(orderingRule.Select((pair, i) => new KeyValuePair<int, Direction>(i, pair.Value)));
      var tupleDescriptor = TupleDescriptor.Create(index.Columns.Select(columnInfo => columnInfo.ValueType));
      var keyDescriptor = TupleDescriptor.Create(orderedBy.Select(pair => tupleDescriptor[pair.Key]));
      return new RecordHeader(index.Columns, keyDescriptor, orderedBy);
    }

    // Constructor

    public IndexProvider(IndexInfo index)
    {
      this.index = index;
    }
  }
}