// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.08.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Packs columns in a source provider into one column, groups them by key columns.
  /// </summary>
  [Serializable]
  public sealed class PackProvider : UnaryProvider
  {
    private const string ToStringFormat = "Pack {0}, by ({1})";

    /// <summary>
    /// Gets header key transform.
    /// </summary>
    public MapTransform KeyTransform { get; private set; }

    /// <summary>
    /// Gets header group transform.
    /// </summary>
    public MapTransform GroupTransform { get; private set; }

    /// <summary>
    /// Gets header pack transform.
    /// </summary>
    public MapTransform PackTransform { get; private set; }

    /// <summary>
    /// Gets column indexes to group by.
    /// </summary>
    public int[] KeyColumnIndexes { get; private set; }

    /// <summary>
    /// Gets column indexes to pack.
    /// </summary>
    public int[] PackColumnIndexes { get; private set; }

    /// <summary>
    /// Gets the packed column.
    /// </summary>
    public SystemColumn PackedColumn { get; private set; }


    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      var groupIndexes = Enumerable.Range(0, Source.Header.Columns.Count).Except(PackColumnIndexes).ToArray();
      GroupTransform = new MapTransform(false, Header.TupleDescriptor, groupIndexes);
      PackTransform = new MapTransform(false, Source.Header.Select(PackColumnIndexes).TupleDescriptor, PackColumnIndexes);
      KeyTransform = new MapTransform(false, Source.Header.Select(KeyColumnIndexes).TupleDescriptor, KeyColumnIndexes);
    }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      var columnIndexes = Enumerable.Range(0, Source.Header.Columns.Count).Except(PackColumnIndexes);
      return Source.Header
        .Select(columnIndexes)
        .Add(PackedColumn);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return string.Format(ToStringFormat,
        PackColumnIndexes.ToCommaDelimitedString(),
        KeyColumnIndexes.ToCommaDelimitedString());
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="keyColumnIndexes">The column indexes to group by.</param>
    /// <param name="packColumnIndexes">The column indexes to pack.</param>
    public PackProvider(CompilableProvider source, int[] keyColumnIndexes, int[] packColumnIndexes, string packColumnName)
      : base(ProviderType.Pack, source)
    {
      KeyColumnIndexes = keyColumnIndexes;
      PackColumnIndexes = packColumnIndexes;
      PackedColumn = new SystemColumn(packColumnName, Source.Header.Length - PackColumnIndexes.Length, typeof (ReadOnlyCollection<Tuple>));
    }
  }
}