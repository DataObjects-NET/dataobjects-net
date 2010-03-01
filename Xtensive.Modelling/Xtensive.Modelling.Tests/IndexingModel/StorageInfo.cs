// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Indexing storage.
  /// </summary>
  [Serializable]
  public sealed class StorageInfo : NodeBase<StorageInfo>,
    IModel
  {
    private ActionSequence actions;

    /// <inheritdoc/>
    public ActionSequence Actions {
      get { return actions; }
      set {
        EnsureIsEditable();
        actions = value;
      }
    }

    /// <summary>
    /// Gets tables.
    /// </summary>
    [Property(Priority = 0)]
    public TableInfoCollection Tables { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Tables == null)
        Tables = new TableInfoCollection(this);
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<StorageInfo, StorageInfo, StorageInfo>(this);
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The storage name.</param>
    public StorageInfo(string name)
      : base(null, name)
    {
    }
  }
}