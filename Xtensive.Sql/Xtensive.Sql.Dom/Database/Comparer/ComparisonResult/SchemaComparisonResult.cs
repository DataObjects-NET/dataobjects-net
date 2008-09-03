// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Schema"/> comparison result.
  /// </summary>
  [Serializable]
  public class SchemaComparisonResult : NodeComparisonResult,
    IComparisonResult<Schema>
  {
    private UserComparisonResult owner;
    private CharacterSetComparisonResult defaultCharacterSet;
    private readonly ComparisonResultCollection<TableComparisonResult> tables = new ComparisonResultCollection<TableComparisonResult>();
    private readonly ComparisonResultCollection<ViewComparisonResult> views = new ComparisonResultCollection<ViewComparisonResult>();
    private readonly ComparisonResultCollection<AssertionComparisonResult> assertions = new ComparisonResultCollection<AssertionComparisonResult>();
    private readonly ComparisonResultCollection<CharacterSetComparisonResult> characterSets = new ComparisonResultCollection<CharacterSetComparisonResult>();
    private readonly ComparisonResultCollection<CollationComparisonResult> collations = new ComparisonResultCollection<CollationComparisonResult>();
    private readonly ComparisonResultCollection<TranslationComparisonResult> translations = new ComparisonResultCollection<TranslationComparisonResult>();
    private readonly ComparisonResultCollection<DomainComparisonResult> domains = new ComparisonResultCollection<DomainComparisonResult>();
    private readonly ComparisonResultCollection<SequenceComparisonResult> sequences = new ComparisonResultCollection<SequenceComparisonResult>();

    /// <inheritdoc/>
    public new Schema NewValue
    {
      get { return (Schema) base.NewValue; }
    }

    /// <inheritdoc/>
    public new Schema OriginalValue
    {
      get { return (Schema) base.OriginalValue; }
    }

    /// <summary>
    /// Gets comparison result of owner.
    /// </summary>
    public UserComparisonResult Owner
    {
      get { return owner; }
      set
      {
        this.EnsureNotLocked();
        owner = value;
      }
    }

    /// <summary>
    /// Gets comparison result of default <see cref="CharacterSet"/>.
    /// </summary>
    public CharacterSetComparisonResult DefaultCharacterSet
    {
      get { return defaultCharacterSet; }
      set
      {
        this.EnsureNotLocked();
        defaultCharacterSet = value;
      }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Table"/>s.
    /// </summary>
    public ComparisonResultCollection<TableComparisonResult> Tables
    {
      get { return tables; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="View"/>s.
    /// </summary>
    public ComparisonResultCollection<ViewComparisonResult> Views
    {
      get { return views; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Assertion"/>s.
    /// </summary>
    public ComparisonResultCollection<AssertionComparisonResult> Assertions
    {
      get { return assertions; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="CharacterSet"/>s.
    /// </summary>
    public ComparisonResultCollection<CharacterSetComparisonResult> CharacterSets
    {
      get { return characterSets; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Collation"/>s.
    /// </summary>
    public ComparisonResultCollection<CollationComparisonResult> Collations
    {
      get { return collations; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Translation"/>s.
    /// </summary>
    public ComparisonResultCollection<TranslationComparisonResult> Translations
    {
      get { return translations; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Domain"/>s.
    /// </summary>
    public ComparisonResultCollection<DomainComparisonResult> Domains
    {
      get { return domains; }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Sequences"/>s.
    /// </summary>
    public ComparisonResultCollection<SequenceComparisonResult> Sequences
    {
      get { return sequences; }
    }

    /// <inheritdoc/>
    public override IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(owner)
          .AddOne(defaultCharacterSet)
          .Union<IComparisonResult>(tables)
          .Union<IComparisonResult>(views)
          .Union<IComparisonResult>(assertions)
          .Union<IComparisonResult>(characterSets)
          .Union<IComparisonResult>(collations)
          .Union<IComparisonResult>(translations)
          .Union<IComparisonResult>(domains)
          .Union<IComparisonResult>(sequences);
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        owner.LockSafely(recursive);
        defaultCharacterSet.LockSafely(recursive);
        tables.Lock(recursive);
        views.Lock(recursive);
        assertions.Lock(recursive);
        characterSets.Lock(recursive);
        collations.Lock(recursive);
        translations.Lock(recursive);
        domains.Lock(recursive);
        sequences.Lock(recursive);
      }
    }
  }
}