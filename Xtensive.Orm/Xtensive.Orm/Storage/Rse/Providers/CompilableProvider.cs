// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordQuery"/> <see cref="RecordQuery.Provider"/>,
  /// that can be compiled.
  /// </summary>
  [Serializable]
  public abstract class CompilableProvider : Provider
  {
    private DirectionCollection<int> expectedOrder;
    private TupleDescriptor orderTupleDescriptor;
    
    /// <summary>
    /// Gets the empty order.
    /// </summary>
    protected internal static DirectionCollection<int> EmptyOrder { get; private set; }

    /// <summary>
    /// Creates the <see cref="RecordQuery"/> wrapping this provider.
    /// </summary>
    public RecordQuery Result
    {
      get { return new RecordQuery(this); }
    }

    /// <summary>
    /// Gets the expected indexes of columns <see cref="Provider.Header"/> should be ordered by.
    /// </summary>
    public DirectionCollection<int> ExpectedOrder
    {
      get { return expectedOrder; }
    }

    /// <summary>
    /// Creates <see cref="DirectionCollection{T}"/> describing the expected ordering of columns.
    /// </summary>
    /// <returns>Created <see cref="DirectionCollection{T}"/> to assign to 
    /// <see cref="ExpectedOrder"/>.</returns>
    protected abstract DirectionCollection<int> CreateExpectedColumnsOrdering();

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      orderTupleDescriptor = Header.OrderTupleDescriptor;
      expectedOrder = CreateExpectedColumnsOrdering();
      expectedOrder.Lock(true);
      ClearOrderingInHeader();
    }

    internal void SetActualOrdering(DirectionCollection<int> ordering)
    {
      ArgumentValidator.EnsureArgumentNotNull(ordering, "ordering");
      ordering.Lock(true);
      SetHeader(new RecordSetHeader(Header.TupleDescriptor, Header.Columns, Header.ColumnGroups,
        orderTupleDescriptor, ordering));
    }

    private void ClearOrderingInHeader()
    {
      if (Header.Order.Count > 0)
        SetHeader(new RecordSetHeader(Header.TupleDescriptor, Header.Columns, Header.ColumnGroups,
          Header.OrderTupleDescriptor, null));
    }


    // Constructors

    /// <inheritdoc/>
    protected CompilableProvider(ProviderType type, params Provider[] sources)
      : base(type, sources)
    {
    }

    // Type initializer

    /// <summary>
    /// <see cref="ClassDocTemplate.TypeInitializer" copy="true"/>
    /// </summary>
    static CompilableProvider()
    {
      EmptyOrder = new DirectionCollection<int>();
      EmptyOrder.Lock(true);
    }
  }
}