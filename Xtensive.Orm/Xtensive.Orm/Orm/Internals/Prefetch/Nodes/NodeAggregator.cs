// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class NodeAggregator<T>
  {
    public static IList<KeyExtractorNode<T>> Aggregate(IEnumerable<KeyExtractorNode<T>> source)
    {
      return source
        .GroupBy(ken => ken.Path, (path, @group) => new KeyExtractorNode<T>(
          path, 
          @group.First().ExtractKeys, 
          @group.SelectMany(ken => ken.NestedNodes).ToList()))
        .Select(Visit)
        .Cast<KeyExtractorNode<T>>()
        .ToList();
    }

    private static Node Visit(Node node)
    {
      throw new NotImplementedException();
    }
  }
}