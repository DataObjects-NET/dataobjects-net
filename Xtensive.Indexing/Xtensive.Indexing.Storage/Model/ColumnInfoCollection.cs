// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="ColumnInfo"/> objects.
  /// </summary>
  [Serializable]
  public class ColumnInfoCollection: NodeCollection<ColumnInfo, IndexInfo>
  {
    /// <summary>
    /// Gets the index this collection belongs to.
    /// </summary>
    public IndexInfo Index
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index"><see cref="Index"/> property value.</param>
    public ColumnInfoCollection(IndexInfo index)
    {
      Index = index;
    }
  }
}