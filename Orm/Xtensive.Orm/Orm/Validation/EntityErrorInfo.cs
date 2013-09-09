// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.09

using System.Collections.Generic;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Entity validation error info.
  /// </summary>
  public class EntityErrorInfo
  {
    /// <summary>
    /// Gets or sets validated entity.
    /// </summary>
    public Entity Target { get; set; }

    /// <summary>
    /// Gets or sets validation errors.
    /// </summary>
    public ICollection<ValidationResult> Errors { get; set; }
  }
}