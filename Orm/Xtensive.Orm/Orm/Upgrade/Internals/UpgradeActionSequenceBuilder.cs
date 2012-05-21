// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.18

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class UpgradeActionSequenceBuilder
  {
    private readonly StorageDriver driver;
    private readonly List<string> output;

    public SqlUpgradeStage Stage { get; private set; }

    public UpgradeActionSequence Result { get; private set; }

    public void BreakBatch()
    {
      output.Add(string.Empty);
    }

    public void RegisterCommand(ISqlCompileUnit command)
    {
      output.Add(driver.Compile(command).GetCommandText());
    }

    public UpgradeActionSequenceBuilder ForStage(SqlUpgradeStage stage)
    {
      return stage==Stage ? this : new UpgradeActionSequenceBuilder(driver, Result, stage);
    }

    private static List<string> SelectOutput(UpgradeActionSequence result, SqlUpgradeStage stage)
    {
      switch (stage) {
      case SqlUpgradeStage.NonTransactionalProlog:
        return result.NonTransactionalPrologCommands;
      case SqlUpgradeStage.NonTransactionalEpilog:
        return result.NonTransactionalEpilogCommands;
      case SqlUpgradeStage.PreCleanupData:
        return result.PreCleanupDataCommands;
      case SqlUpgradeStage.CleanupData:
        return result.CleanupDataCommands;
      case SqlUpgradeStage.PreUpgrade:
        return result.PreUpgradeCommands;
      case SqlUpgradeStage.Upgrade:
        return result.UpgradeCommands;
      case SqlUpgradeStage.CopyData:
        return result.CopyDataCommands;
      case SqlUpgradeStage.PostCopyData:
        return result.PostCopyDataCommands;
      case SqlUpgradeStage.Cleanup:
        return result.CleanupCommands;
      default:
        throw new ArgumentOutOfRangeException("stage");
      }
    }

    // Constructors

    public UpgradeActionSequenceBuilder(StorageDriver driver, UpgradeActionSequence result, SqlUpgradeStage stage)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(result, "result");

      this.driver = driver;

      output = SelectOutput(result, stage);

      Result = result;
      Stage = stage;
    }
  }
}