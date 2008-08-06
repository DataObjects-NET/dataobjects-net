// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public class KeyManager
  {
    private readonly Domain domain;
    private readonly WeakSetSlim<Key> cache = new WeakSetSlim<Key>();
    internal Registry<HierarchyInfo, DefaultGenerator> Generators { get; private set; }

    #region Next methods

    internal Key Next(Type type)
    {
      TypeInfo typeInfo = domain.Model.Types[type];
      DefaultGenerator provider = Generators[typeInfo.Hierarchy];
      Key key = new Key(typeInfo.Hierarchy, provider.Next());
      key.Type = typeInfo;
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
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!type.IsSubclassOf(typeof(Entity)))
        throw new ArgumentException(Strings.ExTypeMustBeEntityDescendant, "type");
      if (tuple.ContainsEmptyValue())
        throw new InvalidOperationException(string.Format("Cannot create Key from tuple: '{0}'", tuple));

      TypeInfo typeInfo = domain.Model.Types[type];
      Key key = Create(typeInfo, tuple);
      return Cache(key);
    }

    internal Key Get(HierarchyInfo hierarchy, Tuple tuple)
    {
      Key key = Create(hierarchy, tuple);
      TryGetType(key, tuple);
      return Cache(key);
    }

    internal Key Get(FieldInfo field, Tuple tuple)
    {
      // Tuple with empty values is treated as empty Entity reference
      if (tuple.ContainsEmptyValue())
        return null;

      TypeInfo type = domain.Model.Types[field.ValueType];
      Key key = Create(type, tuple);
      return Cache(key);
    }

    #endregion

    #region Other private \ internal methods

    private static Key Create(TypeInfo type, Tuple tuple)
    {
      Key key = Create(type.Hierarchy, tuple);
      key.Type = type;
      return key;
    }

    private static Key Create(HierarchyInfo hierarchy, Tuple tuple)
    {
      Tuple keyTuple = Tuple.Create(hierarchy.TupleDescriptor);
      tuple.CopyTo(keyTuple, 0, keyTuple.Count);
      return new Key(hierarchy, keyTuple);
    }

    internal Key GetCached(Key key)
    {
      return cache[key];
    }

    private void TryGetType(Key key, Tuple tuple)
    {
      int columnIndex = key.Hierarchy.Root.Fields[NameBuilder.TypeIdFieldName].MappingInfo.Offset;

      if (columnIndex > tuple.Count -1)
        return;

      key.Type = domain.Model.Types[tuple.GetValue<int>(columnIndex)];
    }

    private Key Cache(Key candidate)
    {
      Key cached = cache[candidate];
      if (cached == null) {
        cache.Add(candidate);
        return candidate;
      }

      // Updating type property
      if (cached.Type == null && candidate.Type != null)
        cached.Type = candidate.Type;

      return cached;
    }

    #endregion


    // Constructors

    internal KeyManager(Domain domain)
    {
      this.domain = domain;
      Generators = new Registry<HierarchyInfo, DefaultGenerator>();
    }
  }
}