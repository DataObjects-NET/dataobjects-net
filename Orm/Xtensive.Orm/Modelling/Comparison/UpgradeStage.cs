// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.25

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Enumeration of possible upgrade stages.
  /// </summary>
  public enum UpgradeStage
  {
    /// <summary>
    /// Record-level cleanup.
    /// </summary>
    CleanupData,
    /// <summary>
    /// Remove unnecesery structures.
    /// </summary>
    Prepare,
    /// <summary>
    /// Rename cycle participants.
    /// </summary>
    TemporaryRename,
    /// <summary>
    /// Create and rename structures, change property values.
    /// </summary>
    Upgrade,
    /// <summary>
    /// Copy data.
    /// </summary>
    CopyData,
    /// <summary>
    /// Post copy data.
    /// </summary>
    PostCopyData,
    /// <summary>
    /// Remove structures thats have not been 
    /// removed on <see cref="UpgradeStage.Prepare"/> stage.
    /// </summary>
    Cleanup,
  }
}