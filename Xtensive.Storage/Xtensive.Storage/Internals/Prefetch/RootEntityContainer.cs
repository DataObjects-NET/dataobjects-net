// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals.Prefetch
{
  [Serializable]
  internal sealed class RootEntityContainer : EntityContainer
  {
    public override EntityGroupTask GetTask()
    {
      if (Task == null) {
        if (!SelectColumnsToBeLoaded())
          return null;
        Task = new EntityGroupTask(Type, ColumnIndexesToBeLoaded.ToArray(), Processor);
      }
      return Task;
    }


    // Constructors

    public RootEntityContainer(Key key, TypeInfo type, bool exactType, PrefetchProcessor processor)
      : base(key, type, exactType, processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }
  }
}