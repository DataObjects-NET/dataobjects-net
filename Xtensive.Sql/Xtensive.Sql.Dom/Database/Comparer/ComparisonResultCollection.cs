// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ComparisonResultCollection<TItem> : CollectionBaseSlim<TItem>
    where TItem : ComparisonResult
  {
    public IEnumerable<ComparisonResult> Find(ComparisonResultLocation locations, ComparisonResultType comparsionTypes, bool recursive, Type[] types)
    {
      if (types==null || types.Length==0) {
        yield break;
      }
      foreach (TItem item in this) {
        if (types.Contains(item.Type) && ((item.ComparisonType & comparsionTypes) > 0)) {
          yield return item;
        }
        if (recursive) {
          foreach (ComparisonResult recursiveResult in item.Find(locations, comparsionTypes, recursive, types)) {
            yield return recursiveResult;
          }
        }
      }
    }
  }
}