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

    // Constructors

    /// <inheritdoc/>
    protected CompilableProvider(ProviderType type, params Provider[] sources)
      : base(type, sources)
    {}

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