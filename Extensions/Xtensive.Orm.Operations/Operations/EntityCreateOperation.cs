// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Text.Json.Serialization;
using Xtensive.Orm.Services;


namespace Xtensive.Orm.Operations
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
    [JsonInclude]
    public string TypeName { get; private set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Title {
      get { return "Create entity"; }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get {
        return $"{Title}, TypeName = {TypeName}, Key = {Key}";
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      // There should be no base method call here!
      context.RegisterKey(context.TryRemapKey(Key), true);
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var domain = session.Domain;
      var key = context.TryRemapKey(Key);
      var type = domain.Model.Types[TypeName];
      key = Key.Create(domain, session.StorageNodeId, type.UnderlyingType, TypeReferenceAccuracy.ExactType, key.Value);
      var persistentAccessor = session.Services.GetService<DirectPersistentAccessor>();
      _ = persistentAccessor.CreateEntity(key);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntityCreateOperation(Key);
      return clone;
    }

    
    // Constructors

    /// <inheritdoc/>
    public EntityCreateOperation(Key key)
      : base(key)
    {
      if (!key.HasExactType)
        throw new ArgumentException("Key must have exact type here.", nameof(key));
      TypeName = key.TypeInfo.Name;
    }
  }
}