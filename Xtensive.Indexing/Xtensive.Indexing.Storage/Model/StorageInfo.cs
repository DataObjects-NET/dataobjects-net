// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single indexing storage.
  /// </summary>
  [Serializable]
  public class StorageInfo : NodeBase<StorageInfo>,
    IModel
  {
    private ActionSequence actions;

    /// <inheritdoc/>
    public ActionSequence Actions
    {
      get { return actions; }
      set
      {
        EnsureIsEditable();
        actions = value;
      }
    }

    /// <summary>
    /// Gets primary indexes.
    /// </summary>
    [Property]
    public PrimaryIndexInfoCollection PrimaryIndexes { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      PrimaryIndexes = new PrimaryIndexInfoCollection(this);
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<StorageInfo, StorageInfo, StorageInfo>(this);
    }


    //Constructors

    public StorageInfo(string name)
      : base(null, name)
    {
    }
  }
}