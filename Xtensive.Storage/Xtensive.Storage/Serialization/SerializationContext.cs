// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Serialization
{
  /// <summary>
  /// Serialization context.
  /// </summary>
  public class SerializationContext : Context<SerializationScope>
  {
    private readonly Func<Entity, SerializationKind> getSerializationKindDelegate;

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
    /// <exception cref="InvalidOperationException">Active context is not found.</exception>
    public static SerializationContext DemandCurrent()
    {
      var currentContext = Current;
      if (currentContext==null)        
        throw new InvalidOperationException(
          string.Format(Strings.ActiveXIsNotFound, typeof(SerializationContext)));
      return currentContext;
    }

    /// <inheritdoc/>
    protected override SerializationScope CreateActiveScope()
    {
      return new SerializationScope(this);
    }

    /// <summary>
    /// Gets the kind of serialization for the given entity.
    /// </summary>
    /// <param name="entity">The entity to be serialized.</param>
    /// <returns>Serialization kind.</returns>
    public SerializationKind GetSerializationKind(Entity entity)
    {
      return getSerializationKindDelegate(entity);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return SerializationScope.CurrentContext==this; }
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="getSerializationKindDelegate">The get serialization kind delegate.</param>
    public SerializationContext(Func<Entity, SerializationKind> getSerializationKindDelegate)
    {
      this.getSerializationKindDelegate = getSerializationKindDelegate;
    }
  }
}