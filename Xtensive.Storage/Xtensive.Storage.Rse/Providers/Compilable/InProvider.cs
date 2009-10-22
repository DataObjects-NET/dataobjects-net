// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if <see cref="UnaryProvider.Source"/> value equal to one of <see cref="TupleSource"/> values; otherwise <see langword="false" />.
  /// </summary>
  [Serializable]
  public class InProvider: UnaryProvider
  {
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string ColumnName { get; private set; }

    /// <summary>
    /// Gets source mapping.
    /// </summary>
    public int[] Mapping { get; private set; }

    /// <summary>
    /// Gets tuple source function.
    /// </summary>
    public Func<IEnumerable<Tuple>> TupleSource { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return new RecordSetHeader(
        TupleDescriptor.Create(new[] { typeof(bool) }),
        new[] { new SystemColumn(ColumnName, 0, typeof(bool)) });
    }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return EmptyOrder;
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public InProvider(CompilableProvider source, string columnName, Func<IEnumerable<Tuple>> tupleSource, int[] mapping)
      : base(ProviderType.In, source)
    {
      ColumnName = columnName;
      TupleSource = tupleSource;
      Mapping = mapping;
    }
  }
}