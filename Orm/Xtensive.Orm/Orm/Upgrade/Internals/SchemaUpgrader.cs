// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Logging;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  internal sealed class SchemaUpgrader
  {
    private readonly UpgradeContext context;
    private readonly Session session;
    private readonly UpgradeServiceAccessor services;
    private readonly ISqlExecutor executor;
    private readonly Action<IEnumerable<string>> statementProcessor;
    private readonly StorageDriver driver;
    private readonly SqlConnection connection;

    public void UpgradeSchema(SchemaExtractionResult extractedSchema,
      StorageModel sourceModel, StorageModel targetModel, ActionSequence upgradeActions)
    {
      var enforceChangedColumns = context.Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();

      var skipConstraints = context.Stage==UpgradeStage.Upgrading;

      var translator = new SqlActionTranslator(
        session.Handlers, executor,
        upgradeActions, extractedSchema, sourceModel, targetModel,
        enforceChangedColumns, !skipConstraints);

      var result = translator.Translate();

      if (SqlLog.IsLogged(LogEventTypes.Info))
        LogStatements(result);

      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeExecuteActions(result);

      result.ProcessWith(statementProcessor, ExecuteNonTransactionally);
    }

    #region Private / internal methods

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      driver.CommitTransaction(null, connection);
      executor.ExecuteMany(batch);
      driver.BeginTransaction(null, connection, null);
    }

    private void ExecuteTransactionally(IEnumerable<string> batch)
    {
      executor.ExecuteMany(batch);
    }

    private void LogStatements(IEnumerable<string> statements)
    {
      SqlLog.Info(
        Strings.LogSessionXSchemaUpgradeScriptY,
        session.ToStringSafely(),
        driver.BuildBatch(statements.ToArray()).Trim());
    }

    #endregion

    // Constructors

    public SchemaUpgrader(UpgradeContext context, Session session)
    {
      this.context = context;
      this.session = session;

      services = context.Services;
      connection = context.Services.Connection;
      executor = session.Services.Demand<ISqlExecutor>();

      driver = services.Driver;
      if (driver.ProviderInfo.Supports(ProviderFeatures.TransactionalDdl))
        statementProcessor = ExecuteTransactionally;
      else
        statementProcessor = ExecuteNonTransactionally;
    }
  }
}