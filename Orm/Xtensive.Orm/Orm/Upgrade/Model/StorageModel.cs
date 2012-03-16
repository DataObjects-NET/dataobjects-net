// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Storage schema.
  /// </summary>
  [Serializable]
  public sealed class StorageModel : NodeBase<StorageModel>,
    IModel
  {
    /// <summary>
    /// Default <see cref="StorageModel"/> node name.
    /// </summary>
    public readonly static string DefaultName = ".";

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

    /// <summary>
    /// Gets sequences.
    /// </summary>
    [Property]
    public SequenceInfoCollection Sequences { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Tables == null)
        Tables = new TableInfoCollection(this);
      if (Sequences==null)
        Sequences=new SequenceInfoCollection(this);
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<StorageModel, StorageModel, StorageModel>(this);
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The storage name.</param>
    public StorageModel(string name)
      : base(null, name)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public StorageModel()
      : base(null, DefaultName)
    {
    }
  }
}