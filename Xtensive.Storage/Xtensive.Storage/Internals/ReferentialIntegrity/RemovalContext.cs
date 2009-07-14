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
    private readonly RemovalManager removalManager;

    public HashSet<EntityState> RemovalQueue { get; private set; }

    public void Dispose()
    {
      RemovalQueue.Clear();
      removalManager.Context = null;
    }


    // Constructors

    public RemovalContext(RemovalManager removalManager)
    {
      this.removalManager = removalManager;
      RemovalQueue = new HashSet<EntityState>();
    }
  }
}