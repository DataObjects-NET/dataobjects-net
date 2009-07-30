// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Aspects;
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
    private readonly Tuple tuple;
    private readonly TypeInfo type;

    /// <inheritdoc/>
    public override TypeInfo Type {
      [DebuggerStepThrough]
      get { return type; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Structure"/> instance is bound to entity.
    /// </summary>
    [Infrastructure]
    public bool IsBoundToEntity {
      get {
        return (Owner!=null) && 
          ((Owner is Entity) || ((Structure) Owner).IsBoundToEntity);
      }
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

    /// <inheritdoc/>
    public override event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEvents.AddSubscriber(GetOwnerEntityKey(Owner), Field,
          EntityEventManager.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerEntityKey(Owner), Field,
         EntityEventManager.PropertyChangedEventKey, value);
      }
    }

    #region NotifyXxx & GetSubscription members

    internal override sealed void NotifyInitializing()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.InitializingPersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
    }

    internal sealed override void NotifyInitialize()
    {
      if (!Session.IsSystemLogicOnly) {
        var subscriptionInfo = GetSubscription(EntityEventManager.InitializePersistentEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field);
        OnInitialize();
      }
      if (CanBeValidated)
        this.Validate();
    }

    internal override sealed void NotifyGettingFieldValue(FieldInfo fieldInfo)
    {
      if (!Session.IsSystemLogicOnly) {
        var subscriptionInfo = GetSubscription(EntityEventManager.GettingFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo, FieldInfo>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, fieldInfo);
        OnGettingFieldValue(fieldInfo);
      }
      if (Owner!=null)
        Owner.NotifyGettingFieldValue(Field);
    }

    internal override sealed void NotifyGetFieldValue(FieldInfo fieldInfo, object value)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.GetFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, FieldInfo, object>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, fieldInfo, value);
      OnGetFieldValue(Field, value);
    }

    internal override sealed void NotifySettingFieldValue(FieldInfo fieldInfo, object value)
    {
      if (Owner!=null)
        Owner.NotifySettingFieldValue(Field, value);
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.SettingFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, FieldInfo, object>) subscriptionInfo.Second)
           .Invoke(subscriptionInfo.First, Field, fieldInfo, value);
      OnSettingFieldValue(Field, value);
    }

    internal override sealed void NotifySetFieldValue(FieldInfo fieldInfo, object oldValue, object newValue)
    {
      if (Owner!=null)
        Owner.NotifySetFieldValue(Field, oldValue, newValue);
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.SetFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, FieldInfo, object, object>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, fieldInfo, oldValue, newValue);
      OnSetFieldValue(Field, oldValue, newValue);
      base.NotifySetFieldValue(fieldInfo, oldValue, newValue);
    }

    protected internal override void NotifyPropertyChanged(FieldInfo fieldInfo)
    {
      if (!Session.EntityEvents.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.PropertyChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((PropertyChangedEventHandler) subscriptionInfo.Second).Invoke(this, new PropertyChangedEventArgs(fieldInfo.Name));
    }

    protected Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = GetOwnerEntityKey(Owner);
      if (entityKey!=null)
        return new Pair<Key, Delegate>(entityKey,
          Session.EntityEvents.GetSubscriber(entityKey, Field, eventKey));
      else
        return new Pair<Key, Delegate>(null, null);
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
    public bool Equals(Structure other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (IsBoundToEntity || other.IsBoundToEntity)
        return InnerEquals(other);
      else
        return AdvancedComparer<Tuple>.Default.Equals(Tuple, other.Tuple);
    }

    [AspectBehavior]
    private bool InnerEquals(Structure other)
    {
      return AdvancedComparer<Tuple>.Default.Equals(Tuple, other.Tuple);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Tuple.GetHashCode();
    }

    #endregion

    #region Private / internal methods

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      if (Owner!=null)
        Owner.EnsureIsFetched(field);
    }

    private static Key GetOwnerEntityKey(Persistent owner)
    {
      var asFieldValueAdapter = owner as IFieldValueAdapter;
      if (asFieldValueAdapter != null)
        return GetOwnerEntityKey(asFieldValueAdapter.Owner);
      return owner != null ? ((Entity) owner).Key : null;
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
    protected Structure(Persistent owner, FieldInfo field)
    {
      type = GetTypeInfo();
      Owner = owner;
      Field = field;
      if (owner == null || field == null)
        tuple = type.TuplePrototype.Clone();
      else
        tuple = field.ExtractValue(
          new ReferencedTuple(() => Owner.Tuple));
      NotifyInitializing();
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