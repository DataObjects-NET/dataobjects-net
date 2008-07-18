// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Resolves a <see cref="Key"/> to <see cref="Entity"/> descendant.
  /// </summary>
  internal static class KeyResolver
  {
    /// <summary>
    /// Resolves the specified key.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to resolve the <paramref name="key"/> to.</typeparam>
    /// <param name="key">The key.</param>
    public static T Resolve<T>(Key key) 
      where T : Entity
    {
      return (T)Resolve(key);
    }

    /// <summary>
    /// Resolves the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    public static Entity Resolve(Key key) 
    {
      if (key.Type == null)
        ResolveType(key);

      EntityData data;
      if (TryResolveKey(key, out data))
        return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);

      return Resolve(key, Fetcher.Fetch(key));
    }

    internal static Entity Resolve(Key key, Tuple tuple)
    {
      if (tuple == null)
        return null;

      EntityData data;
      if (TryResolveKey(key, out data))
        return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);

      data = new EntityData(key, new DifferentialTuple(tuple));
      Session.Current.IdentityMap.Add(data);

      return Entity.Activate(data.Type.UnderlyingType, data);
    }

    private static bool TryResolveKey(Key key, out EntityData data)
    {
      Session session = Session.Current;
      data = session.IdentityMap[key, false];
      return data!=null;
    }

    private static void ResolveType(Key key)
    {
      Session session = Session.Current;
      if (!session.Domain.Model.Types.FindDescendants(key.Hierarchy.Root).GetEnumerator().MoveNext())
        key.Type = key.Hierarchy.Root;
      else
        key.ResolveType(Fetcher.FetchKey(key));
    }
  }
}