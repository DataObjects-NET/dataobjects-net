// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.09

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Entity validation error info.
  /// </summary>
  public sealed class EntityErrorInfo
  {
    /// <summary>
    /// Gets validated entity.
    /// </summary>
    public Entity Target { get; private set; }

    /// <summary>
    /// Gets or sets validation errors.
    /// </summary>
    public ICollection<ValidationResult> Errors { get; set; }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="target">Validated entity.</param>
    public EntityErrorInfo(Entity target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      Target = target;
      Errors = new List<ValidationResult>();
    }
  }
}