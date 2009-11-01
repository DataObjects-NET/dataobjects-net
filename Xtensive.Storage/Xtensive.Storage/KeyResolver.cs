// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  /// <summary>
  /// Resolves a <see cref="Key"/> to <see cref="Entity"/> descendant.
  /// </summary>
  public static class KeyResolver
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
        ResolveKeyType(key);

      Session session = Session.Current;
      EntityData data = session.IdentityMap[key, false];
      if (data != null)
        return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);

      Tuple tuple = session.Handler.Fetch(key);
      if (tuple == null)
        return null;

      data = new EntityData(key, new DifferentialTuple(tuple));
      session.IdentityMap.Add(data);

      return Entity.Activate(data.Type.UnderlyingType, data);
    }

    private static void ResolveKeyType(Key key)
    {
      Session session = Session.Current;
      if (!session.Domain.Model.Types.FindDescendants(key.Hierarchy.Root).GetEnumerator().MoveNext())
        key.Type = key.Hierarchy.Root;
      else
        key.ResolveType(session.Handler.FetchKey(key));
    }
  }
}