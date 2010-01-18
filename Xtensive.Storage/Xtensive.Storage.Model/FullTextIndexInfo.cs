// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.23

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Describes a full-text index in terms of storage.
  /// </summary>
  [Serializable]
  public class FullTextIndexInfo : Node 
  {
    private readonly IndexInfo primaryIndex;
    private readonly FullTextColumnInfoCollection columns = new FullTextColumnInfoCollection();

    /// <summary>
    /// Gets the primary index.
    /// </summary>
    public IndexInfo PrimaryIndex
    {
      get { return primaryIndex; }
    }

    /// <summary>
    /// Gets the full-text index columns.
    /// </summary>
    public FullTextColumnInfoCollection Columns
    {
      get { return columns; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Columns.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextIndexInfo(IndexInfo primaryIndex, string name)
      : base(name)
    {
      this.primaryIndex=primaryIndex;
    }
  }
}