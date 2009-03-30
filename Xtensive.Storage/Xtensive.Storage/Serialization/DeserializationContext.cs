// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Serialization;

namespace Xtensive.Storage.Serialization
{
  /// <summary>
  /// Deserialization context.
  /// </summary>
  [Serializable]
  public class DeserializationContext: Context<DeserializationScope>
  {
    private Dictionary<Pair<Entity, FieldInfo>, object> fieldValues = 
      new Dictionary<Pair<Entity, FieldInfo>, object>();

    private Dictionary<Entity, SerializationInfo> deserializationData = 
      new Dictionary<Entity, SerializationInfo>();

    private bool isDeserialized = false;

    /// <summary>
    /// Gets the current <see cref="DeserializationContext"/>.
    /// </summary>
    public static DeserializationContext Current
    {
      get { return DeserializationScope.CurrentContext; }
    }

    /// <summary>
    /// Gets the current <see cref="DeserializationContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException">Active context is not found.</exception>
    public static DeserializationContext DemandCurrent()
    {
      var currentContext = Current;
      if (currentContext==null)
        throw new InvalidOperationException(
          string.Format(Strings.ActiveXIsNotFound, typeof(DeserializationContext)));
      return currentContext;
    }

    internal void RegisterFieldValue(Entity entity, FieldInfo field, object value)
    {
      fieldValues[new Pair<Entity, FieldInfo>(entity, field)] = value;
    }

    internal void OnEntityCreated(Entity entity, SerializationInfo serializationInfo)
    {
      deserializationData[entity] = serializationInfo;
    }    

    internal void OnDeserialized()
    {
      if (isDeserialized)
        return;

      InitializeEntities();
      DeserializeFieldValues();
      isDeserialized = true;
    }

    private void InitializeEntities()
    {
      foreach (var pair in deserializationData)
        EntitySerializer.DeserializeKey(pair.Key, pair.Value);
    }

    internal void InitializeEntity(Entity entity)
    {
      EntitySerializer.DeserializeKey(entity, deserializationData[entity]);
    }

    private void DeserializeFieldValues()
    {
      foreach (var pair in deserializationData) 
        EntitySerializer.DeserializeFieldValues(pair.Key, pair.Value);      
    }

    /// <inheritdoc/>
    protected override DeserializationScope CreateActiveScope()
    {
      return new DeserializationScope(this);
    }    

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return DeserializationScope.CurrentContext==this; }
    }
  }
}