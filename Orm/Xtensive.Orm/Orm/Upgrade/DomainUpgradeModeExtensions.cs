// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.05

using System;

namespace Xtensive.Orm.Upgrade
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
    [Obsolete("Use IsMultistage() instead")]
    public static bool RequiresUpgradingStage(this DomainUpgradeMode upgradeMode)
    {
      return upgradeMode.IsMultistage();
    }

    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Initializing"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    [Obsolete("Use IsMultistage() instead")]
    public static bool RequiresInitializingStage(this DomainUpgradeMode upgradeMode)
    {
      return upgradeMode.IsMultistage();
    }

    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Upgrading"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    public static bool IsMultistage(this DomainUpgradeMode upgradeMode)
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

    /// <summary>
    /// Determines whether the specified upgrade mode changes database schema.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    /// <returns>
    /// <see langword="true"/> if the specified upgrade mode changes database schema;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsUpgrading(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.Perform:
      case DomainUpgradeMode.PerformSafely:
      case DomainUpgradeMode.Recreate:
        return true;
      default:
        return false;
      }
    }

    internal static SqlWorkerTask GetSqlWorkerTask(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.Skip:
        return SqlWorkerTask.ExtractMetadataTypes;
      case DomainUpgradeMode.Validate:
      case DomainUpgradeMode.Perform:
      case DomainUpgradeMode.PerformSafely:
        return SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata;
      case DomainUpgradeMode.Recreate:
        return SqlWorkerTask.ExtractSchema | SqlWorkerTask.DropSchema;
      case DomainUpgradeMode.LegacySkip:
      case DomainUpgradeMode.LegacyValidate:
        return SqlWorkerTask.ExtractSchema;
      default:
        throw new ArgumentOutOfRangeException("upgradeMode");
      }
    }

    internal static SchemaUpgradeMode GetUpgradingStageUpgradeMode(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.PerformSafely:
        return SchemaUpgradeMode.PerformSafely;
      case DomainUpgradeMode.Perform:
        return SchemaUpgradeMode.Perform;
      default:
        throw new ArgumentOutOfRangeException("upgradeMode");
      }
    }

    internal static SchemaUpgradeMode GetFinalStageUpgradeMode(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.Skip:
      case DomainUpgradeMode.LegacySkip:
        return SchemaUpgradeMode.Skip;
      case DomainUpgradeMode.Validate:
        return SchemaUpgradeMode.ValidateExact;
      case DomainUpgradeMode.LegacyValidate:
        return SchemaUpgradeMode.ValidateLegacy;
      case DomainUpgradeMode.Recreate:
        return SchemaUpgradeMode.Recreate;
      case DomainUpgradeMode.Perform:
      case DomainUpgradeMode.PerformSafely:
        // We need Perform here because after Upgrading stage
        // there may be some recycled columns/tables.
        // Perform will wipe them out.
        return SchemaUpgradeMode.Perform;
      default:
        throw new ArgumentOutOfRangeException("upgradeMode");
      }
    }
  }
}