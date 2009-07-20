// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RemovalContext
  {
    public readonly HashSet<Entity> Items = new HashSet<Entity>();
    public readonly Queue<Entity> Queue = new Queue<Entity>();

    public bool IsEmpty
    {
      get { return Items.Count==0; }
    }

    public void Clear()
    {
      Items.Clear();
      Queue.Clear();
    }
  }
}