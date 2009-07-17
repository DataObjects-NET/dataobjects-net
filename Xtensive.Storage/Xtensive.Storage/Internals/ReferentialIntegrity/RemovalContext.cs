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
    private readonly RemovalProcessor processor;
    public readonly HashSet<Entity> Items = new HashSet<Entity>();
    public readonly Queue<Entity> Queue = new Queue<Entity>();

    public void Dispose()
    {
      Items.Clear();
      processor.Context = null;
    }


    // Constructors

    public RemovalContext(RemovalProcessor processor)
    {
      this.processor = processor;
    }
  }
}