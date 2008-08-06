// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that gives access to the specified <see cref="Index"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Reference to the <see cref="IndexInfo"/> instance within the domain.
    /// </summary>
    public IndexInfoRef Index { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    /// <inheritdoc/>    
    protected override void Initialize()
    {      
    }

    /// <inheritdoc/>
    public override string GetStringParameters()
    {
      return Index.ToString();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexProvider(IndexInfo index)
    {
      header = new RecordSetHeader(index);
      Index = new IndexInfoRef(index);
    }
  }
}