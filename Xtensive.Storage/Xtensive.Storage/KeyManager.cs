// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public class KeyManager
  {
    private readonly Domain domain;
    private readonly WeakSetSlim<Key> cache = new WeakSetSlim<Key>();

    internal Registry<HierarchyInfo, DefaultGenerator> Generators { get; private set; }

    #region Next methods

    /// <summary>
    /// Gets the next key in key sequence.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public Key Next(Type type)
    {
      EnsureIsValid(type);
      return Next(domain.Model.Types[type]);
    }

    internal Key Next(TypeInfo type)
    {
      DefaultGenerator provider = Generators[type.Hierarchy];
      Key key = new Key(type.Hierarchy, provider.Next());
      key.Type = type;
      cache.Add(key);
      return key;
    }

    #endregion

    #region Get methods

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified <paramref name="tuple"/>.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <param name="tuple"><see cref="Tuple"/> with key values.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public Key Get(Type type, Tuple tuple)
    {
      EnsureIsValid(type);
      return Get(domain.Model.Types[type], tuple);
    }

    internal Key Get(TypeInfo type, Tuple tuple)
    {
      if (!Validate(tuple))
        return null;

      Key key = Get(type.Hierarchy, tuple);
      key.Type = type;
      return GetCached(key);
    }

    internal static Key Get(HierarchyInfo hierarchy, Tuple tuple)
    {
      Tuple keyTuple = Tuple.Create(hierarchy.TupleDescriptor);
      tuple.CopyTo(keyTuple, 0, keyTuple.Count);
      return new Key(hierarchy, keyTuple);
    }

    internal Key Get(FieldInfo field, Tuple tuple)
    {
      if (!Validate(tuple))
        return null;

      TypeInfo type = domain.Model.Types[field.ValueType];
      return GetCached(new Key(type.Hierarchy, tuple));
    }

    #endregion

    #region Validation methods

    private static void EnsureIsValid(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!type.IsSubclassOf(typeof(Entity)))
        throw new ArgumentException(Strings.ExTypeMustBeEntityDescendant, "type");
    }

    private static bool Validate(Tuple tuple)
    {
      // Only not null values can be used to build key.
      for (int i = 0; i < tuple.Count; i++)
        if (tuple.IsNull(i))
          return false;
      return true;
    }

    #endregion

    private Key GetCached(Key key)
    {
      if (cache.Contains(key))
        return cache[key];

      cache.Add(key);
      return key;
    }


    // Constructors

    internal KeyManager(Domain domain)
    {
      this.domain = domain;
      Generators = new Registry<HierarchyInfo, DefaultGenerator>();
    }
  }
}