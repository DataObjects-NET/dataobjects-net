// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="Entity"/>.
  /// </summary>
  [Serializable]
  public abstract class EntityOperation : KeyOperation
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    protected EntityOperation(Key key)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }

    /// <inheritdoc/>
    protected EntityOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}