// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Comparison;
using Xtensive.Core;

using Xtensive.Orm.Validation;
using Xtensive.Tuples;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Serialization;
using Xtensive.Orm.Services;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm
{
  /// <summary>
  /// Abstract base class for any persistent structure.
  /// Persistent structures are types having <see cref="ValueType"/> behavior -
  /// they have no keys, and thus can be stored only as parts of entities.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Like <see cref="Entity"/>, structures support inheritance and consist of one or more persistent 
  /// fields (properties) of scalar, <see cref="Structure"/>, or <see cref="Entity"/> type.
  /// </para>
  /// <para>
  /// However unlike entity, structure is not identified by <see cref="Key"/>
  /// and has value type behavior: it can be stored only inside some entity.
  /// </para>
  /// </remarks>
  /// <example>In the following example Address fields (City, Street and Building) will be included in Person table.
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
    public Persistent Owner
    {
      get { return owner; }
      private set {
        entity = null;
        owner = value;
      }
    }

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the entity.
    /// </summary>
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
        Session.EntityEvents.AddSubscriber(GetOwnerEntityKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerEntityKey(Owner), Field,
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
      if (!materialize && CanBeValidated)
        Session.ValidationContext.RegisterForValidation(Entity);
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
        OnSettingFieldValue(field, value);
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

    internal override void SystemTupleChange()
    {
      if (Owner != null)
        Owner.SystemTupleChange();
    }

    internal override sealed void SystemSetValue(FieldInfo field, object oldValue, object newValue)
    {
      if (!Session.IsSystemLogicOnly) {
        using (Session.Operations.EnableSystemOperationRegistration()) {
          if (CanBeValidated)
            Session.ValidationContext.RegisterForValidation(Entity, field);
          var subscriptionInfo = GetSubscription(EntityEventBroker.SetFieldEventKey);
          if (subscriptionInfo.Second!=null)
            ((Action<Key, FieldInfo, FieldInfo, object, object>) subscriptionInfo.Second)
              .Invoke(subscriptionInfo.First, Field, field, oldValue, newValue);

          NotifyFieldChanged(field);
          OnSetFieldValue(field, oldValue, newValue);
        }
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
          Session.EntityEvents.GetSubscriber(entityKey, Field, eventKey));
      return new Pair<Key, Delegate>(null, null);
    }

    #endregion

    #region Equals & GetHashCode

    public static bool operator==(Structure left, Structure right)
    {
      if (!ReferenceEquals(left, null))
        return left.Equals(right);
      return ReferenceEquals(right, null);
    }

    public static bool operator !=(Structure left, Structure rigth)
    {
      return !(left==rigth);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is Structure other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Structure other)
    {
      if (other==null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      var thisIsBound = IsBoundToEntity;
      var otherIsBound = other.IsBoundToEntity;
      if (thisIsBound || otherIsBound)
        return InnerEquals(other, thisIsBound, otherIsBound);
      return Equals(Tuple, other.Tuple);
    }

    private bool InnerEquals(Structure other, bool thisIsBound, bool otherIsBound)
    {
      if (thisIsBound) {
        EnsureIsFetched(Field);
        if (Entity.IsRemoved && !Session.Configuration.Supports(SessionOptions.ReadRemovedObjects))
          throw new InvalidOperationException(Strings.ExEntityIsRemoved);
      }
      if (otherIsBound) {
        other.EnsureIsFetched(other.Field);
        if (other.Entity.IsRemoved && !Session.Configuration.Supports(SessionOptions.ReadRemovedObjects))
          throw new InvalidOperationException(Strings.ExEntityIsRemoved);
      }
      return Equals(Tuple, other.Tuple);
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

    internal override sealed ValidationResult GetValidationResult()
    {
      return Session.ValidationContext.ValidateOnceAndGetErrors(Entity).FirstOrDefault(r => r.Field.Equals(Field));
    }

    internal override sealed ValidationResult GetValidationResult(string fieldName)
    {
      return Session.ValidationContext.ValidateOnceAndGetErrors(Entity).FirstOrDefault(f => f.Field!=null && f.Field.Name==fieldName);
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
    /// Initializes a new instance of this class.
    /// </summary>
    protected Structure()
    {
      typeInfo = GetTypeInfo();
      tuple = typeInfo.TuplePrototype.Clone();
      SystemBeforeInitialize(false);
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected Structure(Session session)
      : base(session)
    {
      typeInfo = GetTypeInfo();
      tuple = typeInfo.TuplePrototype.Clone();
      SystemBeforeInitialize(false);
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="data">Underlying <see cref="Tuple"/> value.</param>
    protected Structure(Tuple data)
    {
      try {
        typeInfo = GetTypeInfo();
        tuple = data;
        SystemBeforeInitialize(false);
      }
      catch (Exception error) {
        InitializationError(GetType(), error); 
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="data">Underlying <see cref="Tuple"/> value.</param>
    protected Structure(Session session, Tuple data)
      : base(session)
    {
      try {
        typeInfo = GetTypeInfo();
        tuple = data;
        SystemBeforeInitialize(false);
        InitializeOnMaterialize();
      }
      catch (Exception error) {
        InitializationError(GetType(), error);
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// Used internally to initialize the structure on materialization.
    /// </summary>
    /// <param name="owner">The owner of this instance.</param>
    /// <param name="field">The owner field that describes this instance.</param>
    protected Structure(Persistent owner, FieldInfo field)
      : base(owner.Session)
    {
      try {
        typeInfo = GetTypeInfo();
        Owner = owner;
        Field = field;
        if (owner==null || field==null)
          tuple = typeInfo.TuplePrototype.Clone();
        else
          tuple = field.ExtractValue(
            new ReferencedTuple(() => Owner.Tuple));
        SystemBeforeInitialize(true);
        InitializeOnMaterialize();
      }
      catch (Exception error) {
        InitializationErrorOnMaterialize(error);
        throw;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Structure"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected Structure(SerializationInfo info, StreamingContext context)
    {
      DeserializationContext.Demand().SetObjectData(this, info, context);
    }
  }
}