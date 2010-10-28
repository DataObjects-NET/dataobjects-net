// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.IoC;

namespace Xtensive.Orm.Serialization
{
  /// <summary>
  /// Deserialization context.
  /// </summary>
  [Serializable]
  public class DeserializationContext: Context<DeserializationScope>
  {
    private readonly Dictionary<Persistent, SerializationInfo> serializationInfos = 
      new Dictionary<Persistent, SerializationInfo>();
    private bool isDeserialized = false;
    private StreamingContext? streamingContext;

    #region Current, Demand & activation-related methods

    /// <summary>
    /// Gets the current <see cref="DeserializationContext"/>.
    /// </summary>
    public static DeserializationContext Current
    {
      get { return DeserializationScope.CurrentContext; }
    }

    /// <summary>
    /// Gets the current <see cref="DeserializationContext"/>, 
    /// or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="DeserializationContext.Current"/> <see cref="DeserializationContext"/> is <see langword="null" />.</exception>
    public static DeserializationContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)
        throw Exceptions.ContextRequired<DeserializationContext,DeserializationScope>();
      return currentContext;
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

    #endregion

    internal void SetObjectData(Persistent persistent, SerializationInfo info, StreamingContext context)
    {
      serializationInfos[persistent] = info;
      if (streamingContext==null)
        streamingContext = context;
    }

    internal void OnDeserialization()
    {
      if (isDeserialized)
        return;

      using (var region = ValidationManager.Disable()) {
        InitializeEntities();
        DeserializeEntities();
        region.Complete();
      }
      isDeserialized = true;
    }

    /// <summary>
    /// Initializes the <see cref="Entity"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to initialize.</param>
    internal protected void InitializeEntity(Entity entity)
    {
      InitializeEntity(entity, serializationInfos[entity], streamingContext.Value);
    }


    /// <summary>
    /// Initializes the entity, i.e. deserializes or generates its <see cref="Key"/> and creates its <see cref="EntityState"/>.
    /// </summary>    
    /// <param name="entity">The entity to initialize.</param>
    /// <param name="serializationInfo">The information to populate the <see cref="Entity.Key"/>.</param>
    /// <param name="context">The source from which the object is deserialized.</param>
    /// <remarks>
    /// Target <see cref="Entity"/> is not initialized on this step, therefore it is unable to get or set its field values.
    /// </remarks>
    protected virtual void InitializeEntity(Entity entity, SerializationInfo serializationInfo, StreamingContext context)
    {
      SerializationHelper.InitializeEntity(entity, serializationInfo, streamingContext.Value);
    }

    /// <summary>
    /// Deserializes the <see cref="Entity"/>'s field values.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to deserialize.</param>
    /// <param name="serializationInfo">The information to populate the <see cref="Entity"/>'s field values.</param>
    /// <param name="context">The source from which the object is deserialized.</param>
    /// <remarks>
    /// <see cref="Entity.Key"/> is already deserialized and all another <see cref="Entity">Entities</see> is already initialized on this step.
    /// </remarks>
    protected virtual void DeserializeEntities(Entity entity, SerializationInfo serializationInfo, StreamingContext context)
    {
      SerializationHelper.DeserializeEntityFields(entity, serializationInfo, context);
    }

    private void InitializeEntities()
    {
      foreach (var pair in serializationInfos) {
        var entity = pair.Key as Entity;
        var info = pair.Value;
        if (entity!=null) {
          InitializeEntity(entity, info, streamingContext.Value);
        }
      }
    }

    private void DeserializeEntities()
    {
      foreach (var pair in serializationInfos) {
        var entity = pair.Key as Entity;
        var info = pair.Value;
        if (entity!=null)
          DeserializeEntities(entity, info, streamingContext.Value);
      }
    }
  } 
}