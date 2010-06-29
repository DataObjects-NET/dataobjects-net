// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Storage.Resources;

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
    private readonly TypeInfo typeInfo;
    private Persistent owner;
    private Entity entity;

    /// <inheritdoc/>
    public override TypeInfo TypeInfo {
      [DebuggerStepThrough]
      get { return typeInfo; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public Persistent Owner
    {
      get { return owner; }
      private set {
        entity = null;
        owner = value;
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the entity.
    /// </summary>
    [Infrastructure]
    public Entity Entity
    {
      get
      {
        if (Owner == null)
          return null;
        return entity ?? (entity = (Owner as Entity) ?? ((Structure) Owner).Entity);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Structure"/> instance is bound to entity.
    /// </summary>
    [Infrastructure]
    public bool IsBoundToEntity {
      get {
        return (Owner!=null) && ((Owner is Entity) || ((Structure) Owner).IsBoundToEntity);
      }
    }

    /// <inheritdoc/>
    protected internal override sealed Tuple Tuple {
      [DebuggerStepThrough]
      get { return tuple; }
    }

    /// <inheritdoc/> 
    protected internal override sealed bool CanBeValidated {
      get { return IsBoundToEntity; }
    }

    /// <inheritdoc/>
    public override sealed event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEventBroker.AddSubscriber(GetOwnerEntityKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEventBroker.RemoveSubscriber(GetOwnerEntityKey(Owner), Field,
         EntityEventBroker.PropertyChangedEventKey, value);
      }
    }

    #region System-level event-like members & GetSubscription members

    internal override sealed void SystemBeforeInitialize(bool materialize)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializingPersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
    }

    internal sealed override void SystemInitialize(bool materialize)
    {
      if (Session.IsSystemLogicOnly)
        return;

      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializePersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnInitialize();
      if (!materialize && CanBeValidated && Session.Domain.Configuration.AutoValidation)
        this.Validate();
    }

    internal override sealed void SystemInitializationError(Exception error)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializationErrorPersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First);
      OnInitializationError(error);
    }

    internal override sealed void SystemBeforeGetValue(FieldInfo field)
    {
      if (!Session.IsSystemLogicOnly) {
        var subscriptionInfo = GetSubscription(EntityEventBroker.GettingFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo, FieldInfo>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, field);
        OnGettingFieldValue(field);
      }
      if (Owner == null) 
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemBeforeGetValue(ownerField);
    }

    internal override sealed void SystemGetValue(FieldInfo field, object value)
    {
      if (!Session.IsSystemLogicOnly) {
        var subscriptionInfo = GetSubscription(EntityEventBroker.GetFieldEventKey);
        if (subscriptionInfo.Second != null)
          ((Action<Key, FieldInfo, FieldInfo, object>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, field, value);
        OnGetFieldValue(Field, value);
      }
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemGetValue(ownerField, value);
    }

    internal override sealed void SystemGetValueCompleted(FieldInfo field, object value, Exception exception)
    {
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemGetValueCompleted(ownerField, value, exception);
    }

    internal override sealed void SystemSetValueAttempt(FieldInfo field, object value)
    {
      if (!Session.IsSystemLogicOnly)
      {
        var subscriptionInfo = GetSubscription(EntityEventBroker.SettingFieldAttemptEventKey);
        if (subscriptionInfo.Second != null)
          ((Action<Key, FieldInfo, FieldInfo, object>)subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, field, value);
        OnSettingFieldValue(field, value);
      }
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemSetValueAttempt(ownerField, value);
    }

    internal override sealed void SystemBeforeSetValue(FieldInfo field, object value)
    {
      if (!Session.IsSystemLogicOnly) {
        var subscriptionInfo = GetSubscription(EntityEventBroker.SettingFieldEventKey);
        if (subscriptionInfo.Second != null)
          ((Action<Key, FieldInfo, FieldInfo, object>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, field, value);
        OnSettingFieldValue(Field, value);
      }
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemBeforeSetValue(ownerField, value);
    }

    internal override sealed void SystemBeforeTupleChange()
    {
      if (Owner != null)
        Owner.SystemBeforeTupleChange();
    }

    internal override sealed void SystemSetValue(FieldInfo field, object oldValue, object newValue)
    {
      if (!Session.IsSystemLogicOnly) {
        if (CanBeValidated && Session.Domain.Configuration.AutoValidation)
          this.Validate();
        var subscriptionInfo = GetSubscription(EntityEventBroker.SetFieldEventKey);
        if (subscriptionInfo.Second != null)
          ((Action<Key, FieldInfo, FieldInfo, object, object>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, field, oldValue, newValue);

        NotifyFieldChanged(field);
        OnSetFieldValue(Field, oldValue, newValue);
      }
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemSetValue(ownerField, oldValue, newValue);
    }

    internal override sealed void SystemSetValueCompleted(FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      if (Owner == null)
        return;
      var ownerField = Owner.TypeInfo.StructureFieldMapping[new Pair<FieldInfo>(Field, field)];
      Owner.SystemSetValueCompleted(ownerField, oldValue, newValue, exception);
    }

    /// <inheritdoc/>
    protected override sealed Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = GetOwnerEntityKey(Owner);
      if (entityKey!=null)
        return new Pair<Key, Delegate>(entityKey,
          Session.EntityEventBroker.GetSubscriber(entityKey, Field, eventKey));
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
      var thisIsBound = IsBoundToEntity;
      var otherisBound = other.IsBoundToEntity;
      if (thisIsBound || otherisBound)
        return InnerEquals(other, thisIsBound, otherisBound);
      return AdvancedComparer<Tuple>.Default.Equals(Tuple, other.Tuple);
    }

    [ActivateSession, Transactional]
    private bool InnerEquals(Structure other, bool thisIsBound, bool otherIsBound)
    {
      if (thisIsBound) {
        EnsureIsFetched(Field);
        if (Entity.IsRemoved)
          throw new InvalidOperationException(Strings.ExEntityIsRemoved);
      }
      if (otherIsBound) {
        other.EnsureIsFetched(other.Field);
        if (other.Entity.IsRemoved)
          throw new InvalidOperationException(Strings.ExEntityIsRemoved);
      }
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
      try {
        typeInfo = GetTypeInfo();
        tuple = typeInfo.TuplePrototype.Clone();
      }
      catch {
        LeaveCtorTransactionScope(false);
        throw;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="data">Underlying <see cref="Tuple"/> value.</param>
    protected Structure(Tuple data)
    {
      try {
        typeInfo = GetTypeInfo();
        tuple = data;
      }
      catch {
        LeaveCtorTransactionScope(false);
        throw;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// Used internally to initialize the structure on materialization.
    /// </summary>
    /// <param name="owner">The owner of this instance.</param>
    /// <param name="field">The owner field that describes this instance.</param>
    protected Structure(Persistent owner, FieldInfo field)
    {
      bool successfully = false;
      try {
        typeInfo = GetTypeInfo();
        Owner = owner;
        Field = field;
        if (owner==null || field==null)
          tuple = typeInfo.TuplePrototype.Clone();
        else
          tuple = field.ExtractValue(
            new ReferencedTuple(() => Owner.Tuple));
        SystemBeforeInitialize(false);
        successfully = true;
      }
      finally {
        // Required, since generated .ctors in descendants 
        // don't call Initialize / InitializationFailed
        LeaveCtorTransactionScope(successfully);
      }
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected Structure(SerializationInfo info, StreamingContext context)
    {
      // TODO: Implement!
      throw new NotImplementedException();
    }
  }
}