// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Schema comparison result.
  /// </summary>
  [Serializable]
  public class SchemaComparisonResult : ComparisonResult<Schema>
  {
    private CharacterSetComparisonResult defaultCharacterSet;
    private readonly CollectionBaseSlim<AssertionComparisonResult> assertions = new CollectionBaseSlim<AssertionComparisonResult>();
    private readonly CollectionBaseSlim<CharacterSetComparisonResult> characterSets = new CollectionBaseSlim<CharacterSetComparisonResult>();
    private readonly CollectionBaseSlim<CollationComparisonResult> collations = new CollectionBaseSlim<CollationComparisonResult>();
    private readonly CollectionBaseSlim<TranslationComparisonResult> translations = new CollectionBaseSlim<TranslationComparisonResult>();
    private readonly CollectionBaseSlim<DomainComparisonResult> domains = new CollectionBaseSlim<DomainComparisonResult>();
    private readonly CollectionBaseSlim<SequenceComparisonResult> sequences = new CollectionBaseSlim<SequenceComparisonResult>();
    private readonly CollectionBaseSlim<TableComparisonResult> tables = new CollectionBaseSlim<TableComparisonResult>();
    private readonly CollectionBaseSlim<ViewComparisonResult> views = new CollectionBaseSlim<ViewComparisonResult>();

    /// <summary>
    /// Gets table comparison result.
    /// </summary>
    public CollectionBaseSlim<TableComparisonResult> Tables
    {
      get { return tables; }
    }

    /// <summary>
    /// Gets view comparison results.
    /// </summary>
    public CollectionBaseSlim<ViewComparisonResult> Views
    {
      get { return views; }
    }

    /// <summary>
    /// Gets default character set comparison result.
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
    /// Gets assertion comparison results.
    /// </summary>
    public CollectionBaseSlim<AssertionComparisonResult> Assertions
    {
      get { return assertions; }
    }

    /// <summary>
    /// Gets character set comparison results.
    /// </summary>
    public CollectionBaseSlim<CharacterSetComparisonResult> CharacterSets
    {
      get { return characterSets; }
    }

    /// <summary>
    /// Gets collation comparison results.
    /// </summary>
    public CollectionBaseSlim<CollationComparisonResult> Collations
    {
      get { return collations; }
    }

    /// <summary>
    /// Gets translation comparison results.
    /// </summary>
    public CollectionBaseSlim<TranslationComparisonResult> Translations
    {
      get { return translations; }
    }

    /// <summary>
    /// Gets domain comparison results.
    /// </summary>
    public CollectionBaseSlim<DomainComparisonResult> Domains
    {
      get { return domains; }
    }

    /// <summary>
    /// Gets sequences comparison results.
    /// </summary>
    public CollectionBaseSlim<SequenceComparisonResult> Sequences
    {
      get { return sequences; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        tables.Lock(recursive);
        views.Lock(recursive);
        defaultCharacterSet.LockSafely(recursive);
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