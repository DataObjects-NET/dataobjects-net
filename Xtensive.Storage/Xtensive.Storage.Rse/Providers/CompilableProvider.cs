// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>,
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
    /// Creates the <see cref="RecordSet"/> wrapping this provider.
    /// </summary>
    public RecordSet Result
    {
      get { return new RecordSet(this); }
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