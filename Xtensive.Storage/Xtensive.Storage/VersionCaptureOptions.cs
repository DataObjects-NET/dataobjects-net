// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Enumerates version capture options for <see cref="VersionCapturer"/>.
  /// </summary>
  [Flags]
  public enum VersionCaptureOptions
  {
    /// <summary>
    /// Default version capture options.
    /// Value is <c>OnMaterialize | OnChange | OnRemove | OverwriteExisting</c>.
    /// </summary>
    Default = OnMaterialize | OnChange | OnRemove | OverwriteExisting,
    /// <summary>
    /// Indicates that version must be captured on entity materialization.
    /// Value is <see langword="0x1" />.
    /// </summary>
    OnMaterialize = 0x1,
    /// <summary>
    /// Indicates that version must be captured on entity creation.
    /// Normally you don't need this flag, since such entities are normally changed later.
    /// Value is <see langword="0x2" />.
    /// </summary>
    OnCreate = 0x2,
    /// <summary>
    /// Indicates that version must be captured on entity change.
    /// Value is <see langword="0x4" />.
    /// </summary>
    OnChange = 0x4,
    /// <summary>
    /// Indicates that version must be captured on entity removal.
    /// Value is <see langword="0x8" />.
    /// </summary>
    OnRemove = 0x8,
    /// <summary>
    /// Indicates that already existing versions must be overwritten by new ones
    /// (i.e. "last version wins" rule is on).
    /// Value is <see langword="0x100" />.
    /// </summary>
    OverwriteExisting = 0x100,
  }
}