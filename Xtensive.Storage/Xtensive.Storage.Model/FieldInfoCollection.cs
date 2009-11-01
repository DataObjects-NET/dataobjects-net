// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class FieldInfoCollection
    : NodeCollection<FieldInfo>,
      IFilterable<FieldAttributes, FieldInfo>
  {
    internal static FieldInfoCollection Empty;

    /// <inheritdoc/>
    public ICountable<FieldInfo> Find(FieldAttributes criteria)
    {
      return Find(criteria, MatchType.Partial);
    }

    /// <inheritdoc/>
    public ICountable<FieldInfo> Find(FieldAttributes criteria, MatchType matchType)
    {
      // We don't have any instance that has attributes == FieldAttributes.None
      if (criteria == FieldAttributes.None)
        return new DummyCountable<FieldInfo>();

      switch (matchType) {
        case MatchType.Partial:
          return new BufferedEnumerable<FieldInfo>(this.Where(f => (f.Attributes & criteria) > 0));
        case MatchType.Full:
          return new BufferedEnumerable<FieldInfo>(this.Where(f => (f.Attributes & criteria) == criteria));
        case MatchType.None:
        default:
          return new BufferedEnumerable<FieldInfo>(this.Where(f => (f.Attributes & criteria) == 0));
      }
    }

    public ICountable<FieldInfo> Find(NestingLevel criteria)
    {
      return new BufferedEnumerable<FieldInfo>(Find(this, criteria));
    }

    private static IEnumerable<FieldInfo> Find(IEnumerable<FieldInfo> fields, NestingLevel level)
    {
      foreach(FieldInfo item in fields) {
        if ((level | NestingLevel.Root) > 0)
          yield return item;
        if ((level | NestingLevel.Nested) > 0 && item.Fields.Count > 0)
          foreach (FieldInfo item2 in Find(item.Fields, level))
            yield return item2;
      }
    }


    // Constructors

    static FieldInfoCollection()
    {
      Empty = new FieldInfoCollection();
      Empty.Lock();
    }
  }
}