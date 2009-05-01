// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Diagnostics;
using Xtensive.Core.Aspects;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Attributes;
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
  [SystemType]
  public abstract class Structure : Persistent,
    IEquatable<Structure>,
    IFieldValueAdapter
  {
    private readonly Persistent owner;
    private readonly FieldInfo field;
    private readonly Tuple tuple;
    private readonly TypeInfo type;

    /// <inheritdoc/>
    internal override TypeInfo Type {
      [DebuggerStepThrough]
      get { return type; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    Persistent IFieldValueAdapter.Owner {
      [DebuggerStepThrough]
      get { return owner; }
    }

    [Infrastructure]
    private bool IsBoundToEntity
    {      
      get {
        return (owner!=null) && 
          ((owner is Entity) || ((Structure) owner).IsBoundToEntity);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    FieldInfo IFieldValueAdapter.Field {
      [DebuggerStepThrough]
      get { return field; }
    }

    /// <inheritdoc/>
    protected internal override Tuple Tuple {
      [DebuggerStepThrough]
      get { return tuple; }
    }

    /// <inheritdoc/> 
    protected internal override bool SkipValidation {
      get { return !IsBoundToEntity; }
    }

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      if (owner!=null)
        owner.EnsureIsFetched(field);
    }

    #region System-level event-like members

    // This is done just to make it sealed
    internal override sealed void OnInitializing(bool notify)
    {
      base.OnInitializing(notify);
    }

    // This is done just to make it sealed
    internal sealed override void OnInitialize(bool notify)
    {
      base.OnInitialize();
      if (!SkipValidation)
        this.Validate();
    }

    internal override sealed void OnGettingFieldValue(FieldInfo field, bool notify)
    {
      base.OnGettingFieldValue(field, notify);
      if (owner!=null)
        owner.OnGettingFieldValue(this.field, notify);
    }

    // This is done just to make it sealed
    internal override sealed void OnGetFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnGetFieldValue(field, value, notify);
    }

    internal override sealed void OnSettingFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnSettingFieldValue(field, value, notify);
      if (owner!=null)
        owner.OnSettingFieldValue(this.field, value, notify);
    }

    internal override sealed void OnSetFieldValue(FieldInfo field, object oldValue, object newValue, bool notify)
    {
      if (owner!=null)
        owner.OnSetFieldValue(this.field, oldValue, newValue, notify);
      base.OnSetFieldValue(field, oldValue, newValue, notify);
    }

    #endregion

    #region Equals & GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null || !(obj is Structure)) {
        return false;
      }
      return Equals((Structure) obj);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool Equals(Structure other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return AdvancedComparer<Tuple>.Default.Equals(Tuple, other.Tuple);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Tuple.GetHashCode();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Structure()
    {
      type = GetTypeInfo();
      tuple = type.TuplePrototype.Clone();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">The owner of this instance.</param>
    /// <param name="field">The owner field that describes this instance.</param>
    /// <param name="notify">If set to <see langword="true"/>, 
    /// initialization related events will be raised.</param>
    protected Structure(Persistent owner, FieldInfo field, bool notify)
    {
      type = GetTypeInfo();
      this.owner = owner;
      this.field = field;
      if (owner == null || field == null)
        tuple = type.TuplePrototype.Clone();
      else
        tuple = field.ExtractValue(
          new ReferencedTuple(() => this.owner.Tuple));
      OnInitializing(notify);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="data">Underlying <see cref="Tuple"/> value.</param>
    protected Structure(Tuple data)
    {
      type = GetTypeInfo();
      tuple = data;
    }
  }
}