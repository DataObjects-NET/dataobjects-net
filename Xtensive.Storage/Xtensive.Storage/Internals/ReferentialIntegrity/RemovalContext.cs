// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System.Collections.Generic;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RemovalContext : Core.Context<RemovalScope>
  {
    private readonly HashSet<Entity> removalQueue = new HashSet<Entity>();

    public bool Notify { get; private set; }

    public HashSet<Entity> RemovalQueue
    {
      get { return removalQueue; }
    }

    /// <inheritdoc/>
    protected override RemovalScope CreateActiveScope()
    {
      return new RemovalScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return RemovalScope.Context==this; }
    }


    // Constructor

    public RemovalContext(bool notify)
    {
      Notify = notify;
    }
  }
}