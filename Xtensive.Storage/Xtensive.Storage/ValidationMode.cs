// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.25

namespace Xtensive.Storage
{
  /// <summary>
  /// Validation mode.
  /// </summary>
  public enum ValidationMode
  {
    /// <summary>
    /// Default value.
    /// The same as <see cref="Continuous"/>.
    /// </summary>
    Default = Continuous,

    /// <summary>
    /// Validation works continuously and autotically validates entities after each operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To work with entities in inconsistent state open inconsistent region with <see cref="ValidationManager.Disable()"/>.
    /// Validation will be performed on disposing inconsistent region.
    /// </para>
    /// <code>
    /// using (var inconsistentRegion = ValidationManager.Disable()) {
    ///   // Perform operations here
    ///   inconsistentRegion.Complete();
    /// }
    /// </code>
    /// </remarks>
    Continuous = 0,

    /// <summary>
    /// Validation is performed automatically only on transaction commit and on explicit <see cref="ValidationManager.Enforce()"/> method call.
    /// </summary>
    OnDemand = 1
  }
}