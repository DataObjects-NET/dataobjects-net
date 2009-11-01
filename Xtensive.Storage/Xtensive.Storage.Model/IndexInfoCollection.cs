// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.16

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class IndexInfoCollection: NodeCollection<IndexInfo>,
    IFilterable<IndexAttributes, IndexInfo>
  {
    public ICountable<IndexInfo> Find(IndexAttributes criteria)
    {
      return Find(criteria, MatchType.Full);
    }

    public ICountable<IndexInfo> Find(IndexAttributes criteria, MatchType matchType)
    {
      if (criteria == IndexAttributes.None)
        return new DummyCountable<IndexInfo>();

      switch (matchType)
      {
        case MatchType.Partial:
          return new BufferedEnumerable<IndexInfo>(this.Where(f => (f.Attributes & criteria) > 0));
        case MatchType.Full:
          return new BufferedEnumerable<IndexInfo>(this.Where(f => (f.Attributes & criteria) == criteria));
        case MatchType.None:
        default:
          return new BufferedEnumerable<IndexInfo>(this.Where(f => (f.Attributes & criteria) == 0));
      }
    }

    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (IndexInfo indexInfo in this)
        indexInfo.Lock();
    }
  }
}