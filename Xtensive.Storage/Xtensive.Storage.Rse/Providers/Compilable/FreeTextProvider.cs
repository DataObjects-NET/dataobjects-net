// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.28

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  [Serializable]
  public class FreeTextProvider : CompilableProvider
  {
    private readonly RecordSetHeader indexHeader;

    public Func<string> SearchCriteria { get; private set; }

    public IndexInfoRef PrimaryIndex { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return EmptyOrder;
    }

    public FreeTextProvider(FullTextIndexInfo index, Func<string> searchCriteria)
      : base(ProviderType.FreeText)
    {
      SearchCriteria = searchCriteria;
      indexHeader = index.GetRecordSetHeader();
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
    }
  }
}