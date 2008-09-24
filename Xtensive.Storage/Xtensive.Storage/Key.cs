// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains a set of identifying values of an <see cref="Entity"/>.
  /// Every entity is uniquely identified by its <see cref="Key"/>.
  /// </summary>
  public sealed class Key : Tuple,
    IEquatable<Key>
  {
    private TypeInfo type;
    private readonly int hashCode;
    private readonly Tuple tuple;

    

    /// <summary>
    /// Gets the hierarchy this instance belongs to.
    /// </summary>
    public HierarchyInfo Hierarchy { get; private set; }

    /// <summary>
    /// Gets the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return type; }
      [DebuggerStepThrough]
      internal set {
        if (type!=null && type!=value)
          throw Exceptions.AlreadyInitialized("Type");
        type = value;
      }
    }

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return tuple.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return tuple.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly("Key");
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return tuple.Descriptor; }
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to resolve this instance to.</typeparam>
    public T Resolve<T>()
      where T : Entity
    {
      return (T)Resolve();
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    public Entity Resolve()
    {
      return KeyResolver.Resolve(this);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified <paramref name="tuple"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="tuple"><see cref="Tuple"/> with key values.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Get<T>(Tuple tuple)
      where T: Entity
    {
      return Get(typeof (T), tuple);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key value.
    /// </summary
    /// <typeparam name="TEntity">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <typeparam name="TKey">Key value type.</typeparam>
    /// <param name="keyValue">Key value.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Get<TEntity, TKey>(TKey keyValue)
      where TEntity: Entity
    {
      return Get(typeof(TEntity), Tuple.Create(keyValue));
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified <paramref name="tuple"/>.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <param name="tuple"><see cref="Tuple"/> with key values.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public static Key Get(Type type, Tuple tuple)
    {
      return Session.Current.Domain.KeyManager.Get(type, tuple);
    }

    #region Equals & GetHashCode

    /// <inheritdoc/>
    public bool Equals(Key other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (hashCode!=other.hashCode)
        return false;
      return tuple.Equals(other.tuple) && Hierarchy.Equals(other.Hierarchy);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (obj is Key) {
        return Equals((Key) obj);
      }
      return false;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      return hashCode;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}, {1}", (Type != null) ? Type.Name : Hierarchy.Name, tuple.ToRegular());
    }


    // Constructors

    internal Key(HierarchyInfo hierarchy, Tuple tuple)
    {
      Hierarchy = hierarchy;
      this.tuple = tuple;
      hashCode = tuple.GetHashCode() ^ hierarchy.GetHashCode();
    }
  }
}