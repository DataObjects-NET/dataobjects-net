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
  /// Represents a set of identifying values of an <see cref="Entity"/>.
  /// Every entity is uniquely identified by its <see cref="Key"/>.
  /// </summary>
  public sealed class Key : IEquatable<Key>
  {
    private TypeInfo type;
    private readonly HierarchyInfo hierarchy;
    private readonly int hashCode;

    /// <summary>
    /// Gets the underlying tuple.
    /// </summary>
    [DebuggerHidden]
    public Tuple Tuple { get; private set; }

    /// <summary>
    /// Gets the hierarchy this instance belongs to.
    /// </summary>
    [DebuggerHidden]
    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
    }

    /// <summary>
    /// Gets the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    [DebuggerHidden]
    public TypeInfo Type
    {
      get { return type; }
      internal set
      {
        if (type!=null) {
          throw Exceptions.AlreadyInitialized("Type");
        }
        type = value;
      }
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    public T GetValue<T>(int fieldIndex)
    {
      return Tuple.GetValue<T>(fieldIndex);
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    public object GetValue(int fieldIndex)
    {
      return Tuple.GetValue(fieldIndex);
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to resolve this instance to.</typeparam>
    public T Resolve<T>()
      where T : Entity
    {
      return KeyResolver.Resolve<T>(this);
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    public Entity Resolve()
    {
      return KeyResolver.Resolve(this);
    }

    internal Entity Resolve(Tuple tuple)
    {
      return KeyResolver.Resolve(this, tuple);
    }

    internal void ResolveType(Tuple tuple)
    {
      int columnIndex = Hierarchy.Root.Fields[Session.Current.HandlerAccessor.Domain.NameProvider.TypeId].MappingInfo.Offset;
      int typeId = tuple.GetValue<int>(columnIndex);
      Type = Session.Current.HandlerAccessor.Domain.Model.Types[typeId];
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key data.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="keyData">The key data.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Build<T>(params object[] keyData) 
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
    public static Key Build(Type type, params object[] keyData)
    {
      return Session.Current.HandlerAccessor.Domain.KeyManager.Build(type, keyData);
    }

    #region Equals & GetHashCode

    /// <inheritdoc/>
    [DebuggerHidden]
    public bool Equals(Key other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (hashCode!=other.hashCode)
        return false;
      return Tuple.Equals(other.Tuple) && hierarchy.Equals(other.hierarchy);
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public override bool Equals(object obj)
    {
      if (obj is Key) {
        return Equals((Key) obj);
      }
      return false;
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public override int GetHashCode()
    {
      return hashCode;
    }

    #endregion


    // Constructors

    internal Key(TypeInfo type, Tuple tuple)
      : this(type.Hierarchy, tuple)
    {
      this.type = type;
    }

    internal Key(HierarchyInfo hierarchy, Tuple tuple)
    {
      Tuple = tuple.ToReadOnly(TupleTransformType.TransformedTuple);
      this.hierarchy = hierarchy;
      hashCode = tuple.GetHashCode() ^ hierarchy.GetHashCode();
    }
  }
}