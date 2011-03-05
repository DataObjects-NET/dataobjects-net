// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.10

using System;
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
    public override void Insert(int index, ColumnInfo item)
    {
      throw new NotSupportedException();
    }

    #region IFilterable<ColumnAttributes,ColumnInfo> Members

    public ICountable<ColumnInfo> Find(ColumnAttributes criteria)
    {
      return Find(criteria, MatchType.Partial);
    }

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
    
    /// <inheritdoc/>
    public ColumnInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}