// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
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
    /// <exception cref="NotSupportedException">Type is already initialized.</exception>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get
      {
        if (type == null)
          type = Session.Current.Domain.KeyManager.ResolveType(this);
        return type;
      }
      [DebuggerStepThrough]
      internal set {
        if (type!=null)
          throw Exceptions.AlreadyInitialized("Type");
        type = value;
      }
    }

    #region Tuple methods

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
    /// <exception cref="Exception">Instance is read-only.</exception>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return tuple.Descriptor; }
    }

    #endregion

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
    public static Key Create<T>(Tuple tuple)
      where T: Entity
    {
      return new Key(typeof (T), tuple);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key value.
    /// </summary
    /// <typeparam name="TEntity">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <typeparam name="TKey">Key value type.</typeparam>
    /// <param name="keyValue">Key value.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Create<TEntity, TKey>(TKey keyValue)
      where TEntity: Entity
    {
      return new Key(typeof(TEntity), Create(keyValue));
    }

    internal bool IsResolved()
    {
      return type!=null ? true : false;
    }


    #region Equals, GetHashCode, ==, != 

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
      if (obj is Key)
        return Equals((Key) obj);
      return false;
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator ==(Key left, Key right)
    {
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator !=(Key left, Key right)
    {
      return !Equals(left, right);
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

    ///<summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    ///</summary>
    ///<param name="hierarchy">Hierarchy value</param>
    ///<param name="tuple">Tuple value</param>
    public Key(HierarchyInfo hierarchy, Tuple tuple)
    {
      Hierarchy = hierarchy;
      this.tuple = tuple;
      hashCode = tuple.GetHashCode() ^ hierarchy.GetHashCode();
    }

    ///<summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    ///</summary>
    ///<param name="type">Type value</param>
    ///<param name="tuple">Tuple value</param>
    public Key(Type type, Tuple tuple)
      :this(Session.Current.Domain.Model.Types[type].Hierarchy, tuple)
    {
    }

    internal Key(TypeInfo type, Tuple tuple)
      : this(type.Hierarchy, tuple)
    {
      this.type = type;
    }
  }
}