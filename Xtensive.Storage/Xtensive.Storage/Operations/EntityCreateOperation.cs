// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> creation operation.
  /// </summary>
  [Serializable]
  public class EntityCreateOperation : EntityOperation
  {
    /// <summary>
    /// Gets the type name of the entity.
    /// </summary>
    public string TypeName { get; private set; }

    /// <inheritdoc/>
    public override string Title {
      get { return "Create entity"; }
    }

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, TypeName = {1}, Key = {2}".FormatWith(Title, TypeName, Key);
      }
    }

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      // There should be no base method call here!
      context.RegisterKey(context.TryRemapKey(Key), true);
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var domain = session.Domain;
      var key = context.TryRemapKey(Key);
      var type = domain.Model.Types[TypeName];
      key = Key.Create(domain, type, TypeReferenceAccuracy.ExactType, key.Value);
      session.CreateOrInitializeExistingEntity(type.UnderlyingType, key);
    }

    
    // Constructors

    /// <inheritdoc/>
    public EntityCreateOperation(Key key)
      : base(key)
    {
      if (!key.HasExactType)
        throw Exceptions.InternalError(Strings.ExKeyMustHaveExactType, Log.Instance);
      TypeName = key.Type.Name;
    }

    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("TypeName", TypeName);
    }

    /// <inheritdoc/>
    protected EntityCreateOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      TypeName = info.GetString("TypeName");
    }
  }
}