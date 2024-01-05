// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.09.09

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