// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.14

using System;
using System.Runtime.Remoting.Lifetime;
using System.Transactions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// An abstract base class for all index storages.
  /// </summary>
  public abstract class IndexStorage : MarshalByRefObject, IStorage
  {
    /// <summary>
    /// Gets the model.
    /// </summary>
    public StorageInfo Model { get; protected set; }
    
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; }
    
    /// <inheritdoc/>
    public abstract IStorageView CreateView(IsolationLevel isolationLevel);

    /// <inheritdoc/>
    public abstract IStorageView GetView(Guid transactionId);

    /// <inheritdoc/>
    public override object InitializeLifetimeService()
    {
      var lease = (ILease)base.InitializeLifetimeService();
      if (lease.CurrentState == LeaseState.Initial)
        lease.InitialLeaseTime=TimeSpan.Zero;

      return lease;
    }

    /// <summary>
    /// Pings this instance.
    /// </summary>
    public void Ping()
    {
    }

    /// <summary>
    /// Gets real index.
    /// </summary>
    /// <param name="indexInfo">The index info.</param>
    /// <returns>The real index.</returns>
    public abstract IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo);

    /// <summary>
    /// Gets the transform for index.
    /// </summary>
    /// <param name="indexInfo">The index info.</param>
    /// <returns>The index transform.</returns>
    public abstract MapTransform GetTransform(IndexInfo indexInfo);


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The name.</param>
    protected IndexStorage(string name)
    {
      Name = name;
    }
  }
}