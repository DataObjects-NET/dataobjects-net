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
    private readonly ExecutionContext executionContext;
    private readonly WeakSetSlim<Key> cache = new WeakSetSlim<Key>();

    /// <summary>
    /// Gets the next key in key sequence.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public Key GetNext<T>() 
      where T: Entity
    {
      return GetNext(typeof (T));
    }

    /// <summary>
    /// Gets the next key in key sequence.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public Key GetNext(Type type)
    {
      return Build(type, null);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key data.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="keyData">The key data.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public Key Build<T>(params object[] keyData) 
      where T: Entity
    {
      return Build(typeof (T), keyData);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key data.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <param name="keyData">The key data.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public Key Build(Type type, params object[] keyData)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(keyData, "keyData");
      if (!type.IsSubclassOf(typeof(Entity)))
        throw new ArgumentException(Strings.ExTypeMustBeEntityDescendant, "type");
      TypeInfo typeInfo = executionContext.Model.Types[type];
      return BuildPrimaryKey(typeInfo, keyData);
    }

    internal Key BuildPrimaryKey(TypeInfo type, params object[] keyData)
    {
      IKeyProvider keyProvider = executionContext.KeyProviders[type.Hierarchy];
      Tuple tuple = Tuple.Create(type.Hierarchy.TupleDescriptor);
      if (keyData == null || keyData.Length==0)
        keyProvider.GetNext(tuple);
      else
        keyProvider.Build(tuple, keyData);

      return ResolveKeyCandidate(new Key(type, tuple));
    }

    internal Key BuildPrimaryKey(HierarchyInfo hierarchy, Tuple data)
    {
      Tuple tuple = Tuple.Create(hierarchy.TupleDescriptor);
      data.Copy(tuple, 0, 0, hierarchy.Columns.Count);
      Key candidate = new Key(hierarchy, tuple);
      candidate.ResolveType(data);
      return ResolveKeyCandidate(candidate);
    }

    internal Key BuildForeignKey(Persistent obj, FieldInfo field)
    {
      TypeInfo typeInfo = executionContext.Model.Types[field.ValueType];
      for (int i = field.MappingInfo.Offset; i < field.MappingInfo.EndOffset; i++)
        if (obj.Tuple.IsNull(i))
          return null;
      Tuple tuple = Tuple.Create(typeInfo.Hierarchy.TupleDescriptor);
      obj.Tuple.Copy(tuple, field.MappingInfo.Offset, 0, field.MappingInfo.Length);
      return ResolveKeyCandidate(new Key(typeInfo.Hierarchy, tuple));
    }

    private Key ResolveKeyCandidate(Key candidate)
    {
      if (cache.Contains(candidate))
        return cache[candidate];
      cache.Add(candidate);
      return candidate;
    }


    // Constructors

    internal KeyManager(ExecutionContext execitionContext)
    {
      this.executionContext = execitionContext;
    }
  }
}