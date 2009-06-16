// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Aspects;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Persistent type that has a value-type behavior.
  /// </summary>
  /// <remarks>
  /// Like <see cref="Entity"/>, it supports inheritance and consists of one or more properties 
  /// of value type, <see cref="Structure"/>, or <see cref="Entity"/> references.
  /// However unlike entity, structure is not identified by <see cref="Key"/>
  /// and has value type behavior: it can exist only inside entity, it is stored in
  /// its owners space and cannot be referenced directly.
  /// </remarks>
  /// <example> In following example address fields (City, Street and Building) will be included in Person table.
  /// <code>
  /// public class Person : Entity
  /// {
  ///   [Field, Key]
  ///   public int Id { get; set; }
  /// 
  ///   public string Name { get; set; }
  /// 
  ///   public Address Address { get; set; }
  /// }
  /// 
  /// public class Address : Structure
  /// {
  ///   [Field]
  ///   public City City { get; set; }
  ///   
  ///   [Field]
  ///   public string Street { get; set; }
  /// 
  ///   [Field]
  ///   public string Building { get; set; }
  /// }
  /// </code>
  /// </example>
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
    protected internal override bool CanBeValidated {
      get { return IsBoundToEntity; }
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
      if (CanBeValidated)
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected Structure(SerializationInfo info, StreamingContext context)
    {
      throw new NotImplementedException();
    }
  }
}