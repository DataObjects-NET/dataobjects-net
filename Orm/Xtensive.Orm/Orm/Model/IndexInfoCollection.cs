// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="IndexInfo"/> objects.
  /// </summary>
  [Serializable]
  public class IndexInfoCollection: NodeCollection<IndexInfo>,
    IFilterable<IndexAttributes, IndexInfo>
  {
    /// <summary>
    /// Finds <see cref="IndexInfo"/> objects by the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <returns>A sequence of found objects.</returns>
    public IEnumerable<IndexInfo> Find(IndexAttributes criteria)
    {
      return Find(criteria, MatchType.Full);
    }

    /// <summary>
    /// Finds <see cref="IndexInfo"/> objects by the specified criteria and match type.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <param name="matchType">Type of the match.</param>
    /// <returns>A sequence of found objects.</returns>
    public IEnumerable<IndexInfo> Find(IndexAttributes criteria, MatchType matchType)
    {
      if (criteria == IndexAttributes.None)
        return Array.Empty<IndexInfo>();

      switch (matchType) {
        case MatchType.Partial:
          return this.Where(f => (f.Attributes & criteria) > 0).ToList();
        case MatchType.Full:
          return this.Where(f => (f.Attributes & criteria) == criteria).ToList();
        case MatchType.None:
        default:
          return this.Where(f => (f.Attributes & criteria) == 0).ToList();
      }
    }


    // Constructors

    /// <inheritdoc/>
    public IndexInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}