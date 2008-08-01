// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares select operator over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SelectProvider : UnaryProvider
  {
    private RecordSetHeader header;
    private readonly int[] columnToSelect;

    /// <summary>
    /// Indexes of columns that should be selected from the <see cref="UnaryProvider.Source"/>.
    /// </summary>
    public int[] ColumnsToSelect
    {
      get
      {
        var result = new int[columnToSelect.Length];
        columnToSelect.CopyTo(result, 0);
        return result;
      }
    }

    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    protected override void Initialize()
    {
      header = new RecordSetHeader(Source.Header, ColumnsToSelect);
    }

    /// <inheritdoc/>
    public override string GetStringParameters()
    {
      return Header.Columns.Select(c => c.Name).ToCommaDelimitedString();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SelectProvider(CompilableProvider provider, int[] columnIndexes)
      : base(provider)
    {
      columnToSelect = columnIndexes;
      Initialize();
    }
  }
}