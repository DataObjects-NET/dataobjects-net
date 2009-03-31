// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// An abstract base class for all columns refs.
  /// </summary>
  [Serializable]
  public abstract class ColumnInfoRef: Ref<ColumnInfo, IndexInfo>
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="index"></param>
    /// <inheritdoc/>
    protected ColumnInfoRef(IndexInfo parent, int index)
      : base(parent, index)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="column">The column.</param>
    /// <param name="index">The index.</param>
    protected ColumnInfoRef(IndexInfo parent, ColumnInfo column, int index)
      : base(parent, index)
    {
      Value = column;
    }
  }
}