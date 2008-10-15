// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
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
    private static readonly ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>> activators = 
      ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>>.Create(new object());

    private readonly Persistent owner;
    private readonly FieldInfo field;
    private readonly Tuple data;
    private readonly TypeInfo type;

    /// <inheritdoc/>
    public override TypeInfo Type {
      [DebuggerStepThrough]
      get { return type; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public Persistent Owner {
      [DebuggerStepThrough]
      get { return owner; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field {
      [DebuggerStepThrough]
      get { return field; }
    }

    /// <inheritdoc/>
    protected internal override Tuple Data {
      [DebuggerStepThrough]
      get { return data; }
    }

    #region Equals & GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null || !(obj is Structure)) {
        return false;
      }
      return Equals((Structure)obj);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool Equals(Structure other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return AdvancedComparer<Tuple>.Default.Equals(Data, other.Data);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Data.GetHashCode();
    }

    #endregion

    #region Inner events

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected internal override sealed void OnGettingValue(FieldInfo field)
    {
      if (owner!=null)
        owner.OnGettingValue(this.field);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected internal override sealed void OnSettingValue(FieldInfo field)
    {
      if (owner!=null)
        owner.OnSettingValue(this.field);
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo field)
    {      
      if (owner!=null)
        owner.OnSetValue(this.field);
      base.OnSetValue(field);
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/> 
    protected internal override bool SkipValidation
    {
      get { return false; }
    }

    #endregion

    internal static Structure Activate(Type type, Persistent owner, FieldInfo field)
    {
      return activators.GetValue(type, 
        DelegateHelper.CreateConstructorDelegate<Func<Persistent, FieldInfo, Structure>>)
        .Invoke(owner, field);
    }

    internal sealed override void EnsureIsFetched(FieldInfo field)
    {
      if (owner != null)
        owner.EnsureIsFetched(field);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Structure()
    {
      type = Session.Domain.Model.Types[GetType()];
      data = Tuple.Create(type.TupleDescriptor);
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
      SegmentTransform transform = Session.Domain.Transforms.GetValue(field, arg => new SegmentTransform(false, owner.Data.Descriptor, new Segment<int>(field.MappingInfo.Offset, field.MappingInfo.Length)));
      data = transform.Apply(TupleTransformType.TransformedTuple, owner.Data);
    }
  }
}