// Copyright (C) 2010-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.01.27

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Building
{
  [Serializable]
  internal sealed class PrefetchActionContainer
  {
    private readonly TypeInfo type;
    private IReadOnlyList<PrefetchFieldDescriptor> fields;

    // Returns null if associations is empty
    public Action<SessionHandler, IEnumerable<Key>> BuildPrefetchAction(IEnumerable<AssociationInfo> associations)
    {
      fields = associations.Select(static association => new PrefetchFieldDescriptor(association.OwnerField, true, false))
        .ToList();
      return fields.Count > 0 ? Prefetch : null;
    }

    private void Prefetch(SessionHandler sh, IEnumerable<Key> keys)
    {
      foreach (var key in keys)
        sh.Prefetch(key, type, fields);
    }

    public PrefetchActionContainer(TypeInfo type)
    {
      this.type = type;
      fields = Array.Empty<PrefetchFieldDescriptor>();
    }
  }
}
