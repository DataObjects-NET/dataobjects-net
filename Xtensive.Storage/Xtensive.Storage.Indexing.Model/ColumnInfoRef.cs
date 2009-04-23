// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// An abstract base class for all columns refs.
  /// </summary>
  [Serializable]
  public abstract class ColumnInfoRef<TParent> : Ref<ColumnInfo, TParent>
    where TParent: IndexInfo
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <inheritdoc/>
    protected ColumnInfoRef(TParent parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The column.</param>
    protected ColumnInfoRef(TParent parent, ColumnInfo column)
      : base(parent)
    {
      Value = column;
    }
  }
}