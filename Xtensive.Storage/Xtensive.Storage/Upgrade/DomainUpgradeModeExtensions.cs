// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.05

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Extension method for <see cref="DomainUpgradeMode"/>.
  /// </summary>
  public static class DomainUpgradeModeExtensions
  {
    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Upgrading"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    public static bool RequiresUpgrade(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Perform:
        case DomainUpgradeMode.PerformSafely:
          return true;
        default:
          return false;
      }      
    }


    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Initializing"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    public static bool RequiresInitialization(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Perform:
        case DomainUpgradeMode.PerformSafely:
        case DomainUpgradeMode.Validate:
        case DomainUpgradeMode.Recreate:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the specified upgrade mode is legacy.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    /// <returns>
    /// <see langword="true"/> if the specified upgrade mode is legacy;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLegacy(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.LegacySkip:
      case DomainUpgradeMode.LegacyValidate:
        return true;
      default:
        return false;
      }
    }
  }
}