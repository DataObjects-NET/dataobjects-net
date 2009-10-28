// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if source value equal to one of provided values; otherwise <see langword="false" />.
  /// </summary>
  [Serializable]
  public class IncludeProvider: UnaryProvider
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
    /// Gets filter function.
    /// </summary>
    public Expression<Func<IEnumerable<Tuple>>> Tuples { get; private set; }

    public MapTransform FilterTransform{ get; private set;}
    public CombineTransform CombineTransform{ get; private set;}

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      var newHeader = Source.Header.Add(new SystemColumn(ColumnName, 0, typeof(bool)));
      var types = Mapping.Select(m => newHeader.Columns[m].Type);
      FilterTransform = new MapTransform(true, TupleDescriptor.Create(types), Mapping);
      CombineTransform = new CombineTransform(true, Source.Header.TupleDescriptor, TupleDescriptor.Create(new []{typeof(bool)}));
      return newHeader;
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
    public IncludeProvider(CompilableProvider source, Expression<Func<IEnumerable<Tuple>>> tuples, string columnName, int[] mapping)
      : base(ProviderType.Include, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuples, "filter");
      Tuples = tuples;
      ColumnName = columnName;
      Mapping = mapping;
    }
  }
}