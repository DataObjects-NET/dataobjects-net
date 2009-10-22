// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if source value equal to one of provided values; otherwise <see langword="false" />.
  /// </summary>
  [Serializable]
  public class InProvider: BinaryProvider
  {
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string ColumnName { get; private set; }

    /// <summary>
    /// Gets source mapping.
    /// </summary>
    public int[] Mapping { get; private set; }

    public MapTransform MapTransform{ get; private set;}

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      MapTransform = new MapTransform(true, Left.Header.TupleDescriptor, Mapping);
      if (MapTransform.Descriptor!=Right.Header.TupleDescriptor) 
        throw new InvalidOperationException(Resources.Strings.ExFilterTupleDescriptorMistmatchesWithSourceMappingDescriptor);

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
    public InProvider(CompilableProvider source, CompilableProvider filter, string columnName, int[] mapping)
      : base(ProviderType.In, source, filter)
    {
      ColumnName = columnName;
      Mapping = mapping;
    }
  }
}