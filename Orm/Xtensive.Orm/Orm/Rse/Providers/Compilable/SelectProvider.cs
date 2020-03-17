// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Tracing;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that declares select operator over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SelectProvider : UnaryProvider
  {
    private readonly int[] columnIndexes;

    /// <summary>
    /// Indexes of columns that should be selected from the <see cref="UnaryProvider.Source"/>.
    /// </summary>
    public int[] ColumnIndexes {
      [DebuggerStepThrough]
      get { return columnIndexes.Copy(); }
    }

    /// <summary>
    /// Tracing information. It's supposed to be set only for root node in the providers tree.
    /// </summary>
    public TraceInfo TraceInfo { get; set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return base.BuildHeader().Select(ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Header.Columns.Select(c => c.Name).ToCommaDelimitedString();
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    public SelectProvider(CompilableProvider provider, int[] columnIndexes)
      : base(ProviderType.Select, provider)
    {
      this.columnIndexes = columnIndexes;
      Initialize();
    }
  }
}