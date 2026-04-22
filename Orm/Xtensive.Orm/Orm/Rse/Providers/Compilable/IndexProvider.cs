// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Orm.Model;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Gives access to the specified <see cref="Index"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : CompilableProvider
  {
    /// <summary>
    /// Reference to the <see cref="IndexInfo"/> instance within the domain.
    /// </summary>
    public IndexInfoRef Index { get; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Index.ToString();
    }


    // Constructors

    public IndexProvider(IndexInfo index)
      : base(ProviderType.Index, index.GetRecordSetHeader())
    {
      Index = new IndexInfoRef(index);
    }
  }
}