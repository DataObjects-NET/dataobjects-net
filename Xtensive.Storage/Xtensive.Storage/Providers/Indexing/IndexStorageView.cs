// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Transactions;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Commands;
using Xtensive.Storage.StorageModel;
using Xtensive.Storage.Providers.Indexing;

namespace Xtensive.Storage.Providers.Indexing
{
  /// <summary>
  /// An abstract base class for all index storage views.
  /// </summary>
  public abstract class IndexStorageView : MarshalByRefObject, IStorageView
  {
    /// <summary>
    /// Gets the storage.
    /// </summary>
    public IndexStorage Storage { get; private set; }

    /// <inheritdoc/>
    public StorageInfo Model { get; protected set; }

    public Providers.SessionHandler SessionHandler { get; private set; }

    /// <inheritdoc/>
    public abstract ITransaction Transaction { get; }

    /// <inheritdoc/>
    public abstract CommandResult Execute(Command command);

    /// <inheritdoc/>
    public abstract Dictionary<int, CommandResult> Execute(List<Command> commands);

    /// <inheritdoc/>
    public abstract void Update(ActionSequence sequence);

    /// <inheritdoc/>
    public abstract IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo, Providers.SessionHandler sessionHandler);
    
    // TODO: Get rid of this!
    /// <summary>
    /// Clears the schema.
    /// </summary>
    public abstract void ClearSchema();

    // TODO: Get rid of this!
    /// <summary>
    /// Creates the new schema.
    /// </summary>
    /// <param name="newSchema">The new schema.</param>
    public abstract void CreateNewSchema(StorageInfo newSchema);

    /// <summary>
    /// Used to periodically ping this instance.
    /// </summary>
    public void Ping()
    {
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="model">The model.</param>
    /// <param name="sessionHandler">The session handler.</param>
    protected IndexStorageView(IndexStorage storage, StorageInfo model, Providers.SessionHandler sessionHandler)
    {
      Storage = storage;
      Model = model;
      SessionHandler = sessionHandler;
    }
  }
}