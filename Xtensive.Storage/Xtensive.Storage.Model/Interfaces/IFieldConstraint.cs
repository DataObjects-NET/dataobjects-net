// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.06.24

using System;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Value constraint.
  /// </summary>
  public interface IFieldConstraint
  {
    /// <summary>
    /// Determines whether this constraint is applicable for specified <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns>
    /// <see langword="True"/> if this constraint is applicable for specified <paramref name="field"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsApplicableFor(FieldInfo field);

    /// <summary>
    /// Determines whether the specified value is value correct.
    /// </summary>
    /// <param name="owner">The owner of changing field.</param>
    /// <param name="field">The changing field.</param>
    /// <param name="value">The new value of the field, that is about to be assigned.</param>
    void Check(object owner, FieldInfo field, object value);
  }
}