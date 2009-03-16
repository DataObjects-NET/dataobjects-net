// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="IndexInfo"/> objects.
  /// </summary>
  [Serializable]
  public class IndexInfoCollection: NodeCollection<IndexInfo, StorageInfo>
  {
    /// <summary>
    /// Gets the storage this collection belongs to.
    /// </summary>
    public StorageInfo Storage
    {
      [DebuggerStepThrough]
      get { return Parent as StorageInfo; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    public IndexInfoCollection(StorageInfo storage)
      : base(storage)
    {
    }
  }
}