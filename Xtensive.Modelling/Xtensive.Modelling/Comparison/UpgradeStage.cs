// Copyright (C) 2009 Xtensive LLC.
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
    /// Remove unnesessery structures.
    /// </summary>
    Prepare,
    /// <summary>
    /// Rename cycle participants.
    /// </summary>
    CyclicRename,
    /// <summary>
    /// Create and rename structures, change property values.
    /// </summary>
    Upgrade,
    /// <summary>
    /// Manipulate data.
    /// </summary>
    DataManipulate,
    /// <summary>
    /// Remove structures thats have not been 
    /// removed on <see cref="Prepare"/> stage.
    /// </summary>
    Cleanup
  }
}