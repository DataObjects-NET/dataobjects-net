// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal sealed class RootEntityContainer : EntityContainer
  {
    private PrefetchFieldDescriptor referencingFieldDescriptor;
    private Key ownerKey;

    public override EntityGroupTask GetTask()
    {
      if (Task == null) {
        if (!SelectColumnsToBeLoaded())
          return null;
        Task = new EntityGroupTask(Type, ColumnIndexesToBeLoaded.ToArray(), Manager);
      }
      return Task;
    }

    public void SetParametersOfReference(PrefetchFieldDescriptor referencingField, Key key)
    {
      referencingFieldDescriptor = referencingField;
      ownerKey = key;
    }

    public void NotifyOwnerAboutKeyWithUnknownType()
    {
      if (Task != null && ownerKey != null)
        referencingFieldDescriptor.NotifySubscriber(ownerKey, Key);
    }
    
    
    // Constructors

    public RootEntityContainer(Key key, TypeInfo type, bool exactType, PrefetchManager manager)
      : base(key, type, exactType, manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }
  }
}