// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.20

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  internal class EntitySetOperation : EntityFieldOperation
  {

    // Constructors

    /// <inheritdoc/>
    protected EntitySetOperation(Key key, OperationType type, FieldInfo fieldInfo)
      : base(key, type, fieldInfo)
    {}

    // Serializable

    /// <inheritdoc/>
    protected EntitySetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {}
  }
}