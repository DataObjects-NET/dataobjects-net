// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single index.
  /// </summary>
  [Serializable]
  public class IndexInfo : Node<IndexInfo, StorageInfo>
  {
    /// <inheritdoc/>
    protected override NodeCollection<IndexInfo, StorageInfo> GetParentNodeCollection()
    {
      return Parent==null ? null : Parent.Indexes;
    }


    // Consturctors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage"><see cref="Storage"/> property value.</param>
    /// <param name="name">Initial <see cref="Node.Name"/> property value.</param>
    public IndexInfo(StorageInfo storage, string name)
      : base(storage, name)
    {
    }
  }
}