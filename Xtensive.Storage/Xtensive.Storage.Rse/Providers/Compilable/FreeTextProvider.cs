// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.28

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  [Serializable]
  public class FreeTextProvider : CompilableProvider
  {
    private readonly RecordSetHeader indexHeader;

    public Func<string> SearchCriteria { get; private set; }

    public IndexInfoRef PrimaryIndex { get; private set; }

    public bool FullFeatured { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return EmptyOrder;
    }

    public FreeTextProvider(FullTextIndexInfo index, Func<string> searchCriteria, string rankColumnName, bool fullFeatured)
      : base(ProviderType.FreeText)
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
      if (FullFeatured) {
        var primaryIndexRecordsetHeader = index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
        var rankColumn = new MappedColumn(rankColumnName, primaryIndexRecordsetHeader.Length, typeof (double));
        indexHeader = primaryIndexRecordsetHeader.Add(rankColumn);
      }
      else {
        if (index.PrimaryIndex.KeyColumns.Count!=1)
          throw new InvalidOperationException(Strings.ExOnlySingleColumnKeySupported);
        var fieldTypes = index
          .PrimaryIndex 
          .KeyColumns
          .Select(columnInfo => columnInfo.Key.ValueType)
          .AddOne(typeof (double));
        TupleDescriptor tupleDescriptor = TupleDescriptor.Create(fieldTypes);
        var columns = index
          .PrimaryIndex
          .KeyColumns
          .Select((c, i) => (Column) new MappedColumn("KEY", i, c.Key.ValueType))
          .AddOne(new MappedColumn("RANK", tupleDescriptor.Count, typeof (double)));
        indexHeader = new RecordSetHeader(tupleDescriptor, columns);
      }
    }
  }
}