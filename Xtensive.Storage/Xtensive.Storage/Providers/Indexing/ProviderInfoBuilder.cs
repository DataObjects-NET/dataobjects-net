// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;

namespace Xtensive.Storage.Providers.Indexing
{
  internal static class ProviderInfoBuilder
  {
    public static ProviderInfo Build()
    {
      const ProviderFeatures features =
        ProviderFeatures.Batches |
        ProviderFeatures.ClusteredIndexes |
        ProviderFeatures.IncludedColumns |
        ProviderFeatures.KeyColumnSortOrder |
        ProviderFeatures.Paging |
        ProviderFeatures.FullFeaturedBooleanExpressions |
        ProviderFeatures.Apply |
        ProviderFeatures.RowNumber;
      return new ProviderInfo(new Version(0, 3), features, int.MaxValue);
    }
  }
}