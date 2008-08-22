// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Schema"/> comparison result.
  /// </summary>
  [Serializable]
  public class SchemaComparisonResult : NodeComparisonResult<Schema>
  {
    private readonly NodeComparisonResult<User> owner = new NodeComparisonResult<User>();
    private readonly NodeComparisonResult<CharacterSet> defaultCharacterSet = new NodeComparisonResult<CharacterSet>();
    private readonly ComparisonResultCollection<TableComparisonResult> tables = new ComparisonResultCollection<TableComparisonResult>();
    private readonly ComparisonResultCollection<ViewComparisonResult> views = new ComparisonResultCollection<ViewComparisonResult>();
    private readonly ComparisonResultCollection<AssertionComparisonResult> assertions = new ComparisonResultCollection<AssertionComparisonResult>();
    private readonly ComparisonResultCollection<NodeComparisonResult<CharacterSet>> characterSets = new ComparisonResultCollection<NodeComparisonResult<CharacterSet>>();
    private readonly ComparisonResultCollection<CollationComparisonResult> collations = new ComparisonResultCollection<CollationComparisonResult>();
    private readonly ComparisonResultCollection<NodeComparisonResult<Translation>> translations = new ComparisonResultCollection<NodeComparisonResult<Translation>>();
    private readonly ComparisonResultCollection<DomainComparisonResult> domains = new ComparisonResultCollection<DomainComparisonResult>();
    private readonly ComparisonResultCollection<SequenceComparisonResult> sequences = new ComparisonResultCollection<SequenceComparisonResult>();

    /// <summary>
    /// Gets comparison result of owner.
    /// </summary>
    public NodeComparisonResult<User> Owner
    {
      get { return owner; }
    }

    /// <summary>
    /// Gets comparison result of default <see cref="CharacterSet"/>.
    /// </summary>
    public NodeComparisonResult<CharacterSet> DefaultCharacterSet
    {
      get { return defaultCharacterSet; }
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
    public ComparisonResultCollection<NodeComparisonResult<CharacterSet>> CharacterSets
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
    public ComparisonResultCollection<NodeComparisonResult<Translation>> Translations
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
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        owner.Lock(recursive);
        defaultCharacterSet.Lock(recursive);
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