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
    #region IFilterable<ColumnAttributes,ColumnInfo> Members

    public IEnumerable<ColumnInfo> Find(ColumnAttributes criteria) => Find(criteria, MatchType.Partial);

    public IEnumerable<ColumnInfo> Find(ColumnAttributes criteria, MatchType matchType) =>
      criteria == ColumnAttributes.None
        ? Array.Empty<ColumnInfo>()   // We don't have any instance that has attributes == FieldAttributes.None
        : matchType switch {
          MatchType.Partial => this.Where(f => (f.Attributes & criteria) > 0),
          MatchType.Full => this.Where(f => (f.Attributes & criteria) == criteria),
          _ => this.Where(f => (f.Attributes & criteria) == 0)
        };

    #endregion

    
    // Constructors
    
    /// <inheritdoc/>
    public ColumnInfoCollection(Node owner, string name)
      : base(owner, name)
    {
      NameIndex = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
    }
  }
}