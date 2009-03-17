// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single primary index.
  /// </summary>
  [Serializable]
  public class PrimaryIndexInfo:IndexInfo
  {
    private readonly CollectionBase<string> secondaryIndexNames;

    /// <summary>
    /// Gets the secondary index names.
    /// </summary>
    /// <value>The secondary index names.</value>
    public CollectionBase<string> SecondaryIndexNames
    {
      [DebuggerStepThrough]
      get
      {
        return secondaryIndexNames;
      }
    }

    /// <summary>
    /// Gets or sets the foreign keys.
    /// </summary>
    /// <value>The foreign keys.</value>
    public NodeCollection<ForeignKeyInfo, PrimaryIndexInfo> ForeignKeys
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      secondaryIndexNames.Lock(recursive);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage"><see cref="Storage"/> property value.</param>
    /// <param name="name">Initial <see cref="Node.Name"/> property value.</param>
    public PrimaryIndexInfo(StorageInfo storage, string name)
      : base(storage, name)
    {
      ForeignKeys = new NodeCollection<ForeignKeyInfo, PrimaryIndexInfo>(this);
      secondaryIndexNames = new CollectionBase<string>();
    }
  }
}