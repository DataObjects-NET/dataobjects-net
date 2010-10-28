// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Index;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// An abstract base class for all index storage views.
  /// </summary>
  public abstract class IndexStorageView : MarshalByRefObject, IStorageView
  {
    /// <summary>
    /// Gets or sets the session handler.
    /// </summary>
    public SessionHandler SessionHandler { get; private set; }

    /// <summary>
    /// Gets the storage.
    /// </summary>
    public IndexStorage Storage { get; private set; }

    /// <inheritdoc/>
    public StorageInfo Model { get; protected set; }

    /// <inheritdoc/>
    public abstract ITransaction Transaction { get; }

    /// <inheritdoc/>
    public abstract CommandResult Execute(Command command);

    /// <inheritdoc/>
    public abstract Dictionary<int, CommandResult> Execute(List<Command> commands);

    /// <inheritdoc/>
    public abstract void Update(ActionSequence sequence);

    /// <inheritdoc/>
    public abstract IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo);
    
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

    /// <summary>
    /// Initializes this view.
    /// </summary>
    /// <param name="sessionHandler">The session handler.</param>
    public virtual void Initialize(SessionHandler sessionHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(sessionHandler, "sessionHandler");
      if (SessionHandler!=null)
        throw Exceptions.AlreadyInitialized(null);
      SessionHandler = sessionHandler;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="model">The model.</param>
    protected IndexStorageView(IndexStorage storage, StorageInfo model)
    {
      Storage = storage;
      Model = model;
    }
  }
}