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


    /// <summary>
    /// Finds the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    public ICountable<FieldInfo> Find(FieldAttributes criteria)
    {
      return Find(criteria, MatchType.Partial);
    }


    /// <summary>
    /// Finds the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria.</param>
    /// <param name="matchType">Type of the match.</param>
    /// <returns></returns>
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


    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="recursive">if set to <c>true</c> [recursive].</param>
    public override void UpdateState(bool recursive)
    {
      if (this==Empty)
        return;
      base.UpdateState(recursive);
    }

    /// <summary>
    /// Gets the exception message.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected override string GetExceptionMessage(string key)
    {
      return string.Format(Strings.ExItemWithKeyXWasNotFound
        + " You might have forgotten to apply [Field] attribute on property {0}.{1}.", Name, key);
    }

    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="FieldInfoCollection"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The name.</param>
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