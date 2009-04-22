// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal static class MappingHelper
  {
    public static List<int> BuildGroupMapping(IList<int> mapping, Provider originProvider, Provider resultProvider)
    {
      var originGroups = originProvider.Header.ColumnGroups.ToList();
      var resultGroups = resultProvider.Header.ColumnGroups.ToList();

      return originGroups
        .Select((og, i) => new { Group = og, Index = i })
        .Where(gi => resultGroups.Any(rg => rg.Keys.Select(rki => mapping[rki]).SequenceEqual(gi.Group.Keys)))
        .Select(gi => gi.Index)
        .ToList();
    }

    public static List<int> Merge(IEnumerable<int> left, IEnumerable<int> right)
    {
      return left
        .Union(right)
        .Distinct()
        .OrderBy(i => i)
        .ToList();
    }

    public static List<int> MergeMappings(Provider originalLeft, List<int> leftMap, List<int> rightMap)
    {
      var leftCount = originalLeft.Header.Length;
      var result = leftMap
        .Concat(rightMap.Select(i => i + leftCount))
        .ToList();
      return result;
    }

  }
}