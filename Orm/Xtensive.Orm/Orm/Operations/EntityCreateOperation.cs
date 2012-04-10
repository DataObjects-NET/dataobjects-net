// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Core;


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
    public string TypeName { get; private set; }


    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public override string Title {
      get { return "Create entity"; }
    }


    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description {
      get {
        return "{0}, TypeName = {1}, Key = {2}".FormatWith(Title, TypeName, Key);
      }
    }


    /// <summary>
    /// Prepares the self.
    /// </summary>
    /// <param name="context">The context.</param>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      // There should be no base method call here!
      context.RegisterKey(context.TryRemapKey(Key), true);
    }


    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var domain = session.Domain;
      var key = context.TryRemapKey(Key);
      var type = domain.Model.Types[TypeName];
      key = Key.Create(domain, type, TypeReferenceAccuracy.ExactType, key.Value);
      session.CreateOrInitializeExistingEntity(type.UnderlyingType, key);
    }


    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    /// <param name="clone"></param>
    /// <returns></returns>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntityCreateOperation(Key);
      return clone;
    }

    
    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateOperation"/> class.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    public EntityCreateOperation(Key key)
      : base(key)
    {
      if (!key.HasExactType)
        throw Exceptions.InternalError(Strings.ExKeyMustHaveExactType, Log.Instance);
      TypeName = key.TypeInfo.Name;
    }

    // Serialization


    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("TypeName", TypeName);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntityCreateOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      TypeName = info.GetString("TypeName");
    }
  }
}