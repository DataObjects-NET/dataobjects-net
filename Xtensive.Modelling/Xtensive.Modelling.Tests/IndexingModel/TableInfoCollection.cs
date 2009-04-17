// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="TableInfo"/> instances.
  /// </summary>
  [Serializable]
  public sealed class TableInfoCollection: NodeCollectionBase<TableInfo, StorageInfo>
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    public TableInfoCollection(StorageInfo storage)
      : base(storage, "Tables")
    {
    }
  }
}