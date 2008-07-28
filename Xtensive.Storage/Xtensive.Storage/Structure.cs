// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Represents a set of related values.
  /// </summary>
  /// <remarks>
  /// Like <see cref="Entity"/>, it supports inheritance and consists of one or more properties 
  /// of <see cref="ValueType"/>, <see cref="Structure"/>, or <see cref="Entity"/> references.
  /// However unlike <see cref="Entity"/>, <see cref="Structure"/> it is not identified by <see cref="Key"/> 
  /// and has <see cref="ValueType"/> behavior: it can exist only inside <see cref="Entity"/>, it is stored in
  /// its owners space and cannot be referenced directly.
  /// </remarks>
  public abstract class Structure : Persistent,
    IEquatable<Structure>,
    IFieldHandler
  {
    private static readonly Dictionary<Type, Func<Persistent, FieldInfo, Structure>> activators = new Dictionary<Type, Func<Persistent, FieldInfo, Structure>>();

    private readonly Persistent owner;
    private readonly FieldInfo field;
    private readonly Tuple tuple;
    private readonly TypeInfo type;

    /// <inheritdoc/>
    [DebuggerHidden]
    public override TypeInfo Type
    {
      get { return type; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public Persistent Owner
    {
      get { return owner; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public FieldInfo Field
    {
      get { return field; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    protected internal override Tuple Tuple
    {
      get { return tuple; }
    }

    #region GetHashCode, Equals

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Tuple.GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null || !(obj is Structure)) {
        return false;
      }
      return Equals((Structure)obj);
    }

    /// <inheritdoc/>
    public bool Equals(Structure other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return AdvancedComparer<Tuple>.Default.Equals(Tuple, other.Tuple);
    }

    #endregion

    #region Inner events

    /// <inheritdoc/>
    protected internal override sealed void OnGettingValue(FieldInfo field)
    {
      if (owner!=null)
        owner.OnGettingValue(this.field);
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSettingValue(FieldInfo field)
    {
      if (owner!=null)
        owner.OnSettingValue(this.field);
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo field)
    {
      if (owner==null)
        return;
      owner.OnSetValue(this.field);
    }

    #endregion

    protected static void RegisterActivator(Type type, Func<Persistent, FieldInfo, Structure> activator)
    {
      if (!activators.ContainsKey(type))
        activators.Add(type, activator);
    }

    internal static Structure Activate(Type type, Persistent owner, FieldInfo field)
    {
      if (!activators.ContainsKey(type)) {
        var constructorInvocationDelegate = DelegateHelper.CreateConstructorDelegate<Func<Persistent, FieldInfo, Structure>>(type);
        activators.Add(type, constructorInvocationDelegate);
        return constructorInvocationDelegate(owner, field);
      }
      return activators[type](owner, field);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Structure()
    {
      type = Session.Domain.Model.Types[GetType()];
      tuple = Tuple.Create(type.TupleDescriptor);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">The owner of this instance.</param>
    /// <param name="field">The owner field that describes this instance.</param>
    protected Structure(Persistent owner, FieldInfo field)
    {
      type = Session.Domain.Model.Types[GetType()];
      this.owner = owner;
      this.field = field;
      tuple = new SegmentTransform(false, owner.Tuple.Descriptor, new Segment<int>(field.MappingInfo.Offset, field.MappingInfo.Length)).Apply(TupleTransformType.TransformedTuple, owner.Tuple);
    }
  }
}