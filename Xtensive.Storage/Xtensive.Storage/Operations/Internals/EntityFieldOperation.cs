// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal abstract class EntityFieldOperation : EntityOperation
  {
    protected FieldInfo Field { get; private set; }


    // Constructors

    protected EntityFieldOperation(Key key, OperationType type, FieldInfo fieldInfo)
      : base(key, type)
    {
      Field = fieldInfo;
    }

    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("field", new FieldInfoRef(Field), typeof(FieldInfoRef));
    }

    protected EntityFieldOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      var session = Session.Demand();
      var fieldRef = (FieldInfoRef)info.GetValue("field", typeof(FieldInfoRef));
      Field = fieldRef.Resolve(session.Domain.Model);
    }
  }
}