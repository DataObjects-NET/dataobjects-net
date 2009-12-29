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

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    } 

    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return EmptyOrder;
    }

    public FreeTextProvider(FullTextIndexInfo index)
      : base(ProviderType.FreeText)
    {
      indexHeader = index.GetRecordSetHeader();
    }
  }
}