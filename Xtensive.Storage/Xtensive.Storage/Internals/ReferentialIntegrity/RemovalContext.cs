// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RemovalContext : IDisposable
  {
    public ReferenceManager ReferenceManager { get; private set; }

    public bool Notify { get; private set; }

    public HashSet<EntityState> RemovalQueue { get; private set; }

    public void Dispose()
    {
      RemovalQueue.Clear();
      ReferenceManager.Context = null;
    }


    // Constructors

    public RemovalContext(ReferenceManager referenceManager, bool notify)
    {
      ReferenceManager = referenceManager;
      RemovalQueue = new HashSet<EntityState>();
      Notify = notify;
    }
  }
}