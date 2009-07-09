// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.26

using System;
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
        return new EmptyCountable<FieldInfo>();

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


    // Constructors

    static FieldInfoCollection()
    {
      Empty = new FieldInfoCollection();
    }
  }
}