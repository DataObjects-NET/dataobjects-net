// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class RootEntityPrefetchContainer : EntityPrefetchContainer
  {
    public override EntityGroupPrefetchTask GetTask()
    {
      if (Task == null) {
        if (!SelectColumnsToBeLoaded())
          return null;
        Task = new EntityGroupPrefetchTask(Type, ColumnIndexesToBeLoaded.ToArray(), Processor);
      }
      return Task;
    }


    // Constructors

    public RootEntityPrefetchContainer(Key key, TypeInfo type, bool exactType, PrefetchProcessor processor)
      : base(key, type, exactType, processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }
  }
}