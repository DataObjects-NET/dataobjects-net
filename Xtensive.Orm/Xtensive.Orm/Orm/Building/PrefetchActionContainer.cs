// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Orm.Building
{
  [Serializable]
  internal sealed class PrefetchActionContainer
  {
    private readonly TypeInfo type;
    private List<AssociationInfo> associations;
    private ReadOnlyList<PrefetchFieldDescriptor> fields;

    public Action<SessionHandler, IEnumerable<Key>> BuildPrefetchAction()
    {
      fields = new ReadOnlyList<PrefetchFieldDescriptor>(
        associations
          .Select(association => new PrefetchFieldDescriptor(association.OwnerField, true, false))
          .ToList());
      return Prefetch;
    }

    private void Prefetch(SessionHandler sh, IEnumerable<Key> keys)
    {
      foreach (var key in keys)
        sh.Prefetch(key, type, fields);
    }

    public PrefetchActionContainer(TypeInfo type, List<AssociationInfo> associations)
    {
      this.type = type;
      this.associations = associations;
      fields = new ReadOnlyList<PrefetchFieldDescriptor>(new List<PrefetchFieldDescriptor>());
    }
  }
}