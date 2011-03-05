// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Serialization
{
  /// <summary>
  /// Serialization context.
  /// </summary>
  public class SerializationContext : Context<SerializationScope>
  {
    private readonly Func<Entity, SerializationKind> serializationKindGetter;

    /// <summary>
    /// Gets the current <see cref="SerializationContext"/>.
    /// </summary>
    public static SerializationContext Current
    {
      get { return SerializationScope.CurrentContext; }
    }

    /// <summary>
    /// Gets the current <see cref="SerializationContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException"><see cref="SerializationContext.Current"/> <see cref="SerializationContext"/> is <see langword="null" />.</exception>
    public static SerializationContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)        
        throw Exceptions.ContextRequired<SerializationContext,SerializationScope>();
      return currentContext;
    }

    /// <inheritdoc/>
    protected override SerializationScope CreateActiveScope()
    {
      return new SerializationScope(this);
    }

    internal void GetEntityData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      var serializationKind = GetSerializationKind(entity);
      if (serializationKind==SerializationKind.ByReference)
        GetEntityReferenceData(entity, info, context);
      else
        GetEntityValueData(entity, info, context);
    }

    /// <summary>
    /// Gets the entity value data, i.e. data which will be deserialized as a new <see cref="Entity"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to serialize.</param>
    /// <param name="info">The object to be populated with serialization information.</param>
    /// <param name="context">The destination context of the serialization.</param>
    protected virtual void GetEntityValueData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      SerializationHelper.GetEntityValueData(entity, info, context);
    }

    /// <summary>
    /// Gets the entity reference data, i.e. data which will be deserialized as a reference to existing <see cref="Entity"/>
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to serialize.</param>
    /// <param name="info">The object to be populated with serialization information.</param>
    /// <param name="context">The destination context of the serialization.</param>
    protected virtual void GetEntityReferenceData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      SerializationHelper.GetEntityReferenceData(entity, info, context);
    }

    /// <summary>
    /// Gets the kind of serialization for the specified <see cref="Entity"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to be serialized.</param>
    /// <returns>Serialization kind.</returns>
    protected virtual SerializationKind GetSerializationKind(Entity entity)
    {
      return serializationKindGetter.Invoke(entity);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return SerializationScope.CurrentContext==this; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serializationKind">Default <see cref="SerializationKind"/>.</param>
    public SerializationContext(SerializationKind serializationKind)
    {
      serializationKindGetter = (entity => serializationKind);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serializationKindGetter">The <see cref="SerializationKind"/> getter.</param>
    public SerializationContext(Func<Entity, SerializationKind> serializationKindGetter)
    {
      ArgumentValidator.EnsureArgumentNotNull(serializationKindGetter, "serializationKindGetter");
      this.serializationKindGetter = serializationKindGetter;
    }
  }
}