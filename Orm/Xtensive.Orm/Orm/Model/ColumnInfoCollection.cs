// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="ColumnInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfoCollection : NodeCollection<ColumnInfo>,
    IFilterable<ColumnAttributes, ColumnInfo>
  {
    /// <summary>
    /// Inserts the specified item.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item.</param>
    public override void Insert(int index, ColumnInfo item)
    {
      throw new NotSupportedException();
    }

    #region IFilterable<ColumnAttributes,ColumnInfo> Members

    /// <summary>
    /// Finds the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    public ICountable<ColumnInfo> Find(ColumnAttributes criteria)
    {
      return Find(criteria, MatchType.Partial);
    }

    /// <summary>
    /// Finds the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <param name="matchType">Type of the match.</param>
    /// <returns></returns>
    public ICountable<ColumnInfo> Find(ColumnAttributes criteria, MatchType matchType)
    {
      // We don't have any instance that has attributes == FieldAttributes.None
      if (criteria == ColumnAttributes.None)
        return new EmptyCountable<ColumnInfo>();

      switch (matchType) {
        case MatchType.Partial:
          return new BufferedEnumerable<ColumnInfo>(this.Where(f => (f.Attributes & criteria) > 0));
        case MatchType.Full:
          return new BufferedEnumerable<ColumnInfo>(this.Where(f => (f.Attributes & criteria) == criteria));
        case MatchType.None:
        default:
          return new BufferedEnumerable<ColumnInfo>(this.Where(f => (f.Attributes & criteria) == 0));
      }
    }

    #endregion

    
    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnInfoCollection"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The name.</param>
    public ColumnInfoCollection(Node owner, string name)
      : base(owner, name)
    {
      NameIndex = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
    }
  }
}