// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.26

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="FieldInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class FieldInfoCollection
    : NodeCollection<FieldInfo>,
      IFilterable<FieldAttributes, FieldInfo>
  {
    internal new static FieldInfoCollection Empty;

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

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      if (this==Empty)
        return;
      base.UpdateState(recursive);
    }

    protected override string GetExceptionMessage(string key)
    {
      return string.Format(Strings.ExItemWithKeyXWasNotFound
        + " You might have forgotten to apply [Field] attribute on property {1}.{0}.", key, Name);
    }

    // Constructors

    /// <inheritdoc/>
    public FieldInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }

    // Type initializer

    static FieldInfoCollection()
    {
      Empty = new FieldInfoCollection(null, "Empty");
    }
  }
}