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
    public IList<ValidationResult> Errors { get; private set; }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="target">Validated entity.</param>
    /// <param name="errors">A collection of <see cref="ValidationResult"/>s for an errors discovered.</param>
    public EntityErrorInfo(Entity target, IList<ValidationResult> errors)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(errors, "errors");

      Target = target;
      Errors = errors;
    }
  }
}