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
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
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

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return base.BuildHeader().Select(ColumnIndexes);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Header.Columns.Select(c => c.Name).ToCommaDelimitedString();
    }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      var selectOrdering = new DirectionCollection<int>();
      foreach (KeyValuePair<int, Direction> pair in Source.ExpectedOrder) {
        var columnIndex = ColumnIndexes.IndexOf(pair.Key);
        if (columnIndex < 0) {
          if (selectOrdering.Count > 0)
            selectOrdering.Clear();
          break;
        }
        selectOrdering.Add(columnIndex, pair.Value);
      }
      return selectOrdering;
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SelectProvider(CompilableProvider provider, int[] columnIndexes)
      : base(ProviderType.Select, provider)
    {
      this.columnIndexes = columnIndexes;      
    }
  }
}