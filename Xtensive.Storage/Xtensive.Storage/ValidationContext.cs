// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.28

using Xtensive.Integrity.Validation;

namespace Xtensive.Storage
{
  /// <summary>
  /// Validation context.
  /// </summary>
  public sealed class ValidationContext : ValidationContextBase
  {
    /// <summary>
    /// Validates all instances registered in validation context of current session.
    /// <see cref="InconsistentRegion">Inconsistent regions</see> are ignored.
    /// </summary>
    public new static void Validate()
    {
      var session = Session.Demand();
      session.ValidationContext.ValidateAll();
    }

    /// <inheritdoc/>
    protected override InconsistentRegionBase CreateInconsistentRegion()
    {
      return new InconsistentRegion(this);
    }
  }
}