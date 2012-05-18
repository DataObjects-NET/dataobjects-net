// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.21

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Stage of <see cref="UpgradeActionSequence"/>.
  /// </summary>
  public enum SqlUpgradeStage
  {
    /// <summary>
    /// <see cref="UpgradeActionSequence.NonTransactionalPrologCommands"/>.
    /// </summary>
    NonTransactionalProlog,
    /// <summary>
    /// <see cref="UpgradeActionSequence.NonTransactionalEpilogCommands"/>.
    /// </summary>
    NonTransactionalEpilog,
    /// <summary>
    /// <see cref="UpgradeActionSequence.PreCleanupDataCommands"/>
    /// </summary>
    PreCleanupData,
    /// <summary>
    /// <see cref="UpgradeActionSequence.CleanupDataCommands"/>.
    /// </summary>
    CleanupData,
    /// <summary>
    /// <see cref="UpgradeActionSequence.PreUpgradeCommands"/>.
    /// </summary>
    PreUpgrade,
    /// <summary>
    /// <see cref="UpgradeActionSequence.UpgradeCommands"/>.
    /// </summary>
    Upgrade,
    /// <summary>
    /// <see cref="UpgradeActionSequence.CopyDataCommands"/>.
    /// </summary>
    CopyData,
    /// <summary>
    /// <see cref="UpgradeActionSequence.PostCopyDataCommands"/>.
    /// </summary>
    PostCopyData,
    /// <summary>
    /// <see cref="UpgradeActionSequence.CleanupCommands"/>.
    /// </summary>
    Cleanup,
  }
}