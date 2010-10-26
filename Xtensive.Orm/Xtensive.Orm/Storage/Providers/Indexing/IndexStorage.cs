// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.14

using System;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Transactions;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Providers.Indexing
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
    public abstract IStorageView CreateView(Providers.SessionHandler sessionHandler, IsolationLevel isolationLevel);

    /// <inheritdoc/>
    public abstract IStorageView GetView(Providers.SessionHandler sessionHandler, Guid transactionId);

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

    /// <summary>
    /// Used to periodically ping this instance.
    /// </summary>
    public void Ping()
    {
    }

    /// <inheritdoc/>
#if NET40
    [SecurityCritical]
#endif
    public override object InitializeLifetimeService()
    {
      var lease = (ILease)base.InitializeLifetimeService();
      if (lease.CurrentState == LeaseState.Initial)
        lease.InitialLeaseTime = TimeSpan.Zero;

      return lease;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The storage name.</param>
    protected IndexStorage(string name)
    {
      Name = name;
    }
  }
}