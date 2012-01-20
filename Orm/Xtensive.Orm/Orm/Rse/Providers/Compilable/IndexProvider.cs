// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Collections;
using Xtensive.Orm.Model;
using Xtensive.Threading;
using Xtensive.Orm.Rse.Compilation;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Gives access to the specified <see cref="Index"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : CompilableProvider
  {
    private readonly RecordSetHeader indexHeader;

    /// <summary>
    /// Reference to the <see cref="IndexInfo"/> instance within the domain.
    /// </summary>
    public IndexInfoRef Index { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Index.ToString();
    }

 
    // Factory method

    /// <summary>
    /// Gets the <see cref="IndexProvider"/> for the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to get the provider for.</param>
    /// <returns>Existing or newly created provider for the specified <paramref name="index"/>.</returns>
    public static IndexProvider Get(IndexInfo index)
    {
      return new IndexProvider(index);
    }


    // Constructors

    private IndexProvider(IndexInfo index)
      : base (ProviderType.Index)
    {
      indexHeader = index.GetRecordSetHeader();
      Index = new IndexInfoRef(index);
      Initialize(typeof(IndexProvider)); // Since .ctor is private!
    }
  }
}