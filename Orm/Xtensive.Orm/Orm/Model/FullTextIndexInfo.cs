// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.23

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a single full-text index.
  /// </summary>
  [Serializable]
  public sealed class FullTextIndexInfo : Node 
  {
    private readonly IndexInfo primaryIndex;
    private readonly FullTextColumnInfoCollection columns;

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
      this.primaryIndex = primaryIndex;
      columns = new FullTextColumnInfoCollection(this, "Columns");
    }
  }
}