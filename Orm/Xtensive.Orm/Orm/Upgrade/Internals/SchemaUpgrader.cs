// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ISqlExecutor executor;
    private readonly Action<IEnumerable<string>> statementProcessor;
    private readonly Func<IEnumerable<string>, CancellationToken, Task> asyncStatementProcessor;
    private readonly StorageDriver driver;
    private readonly SqlConnection connection;

    public void UpgradeSchema(SchemaExtractionResult extractedSchema,
      StorageModel sourceModel, StorageModel targetModel, ActionSequence upgradeActions)
    {
      var result = TranslateActions(extractedSchema, sourceModel, targetModel, upgradeActions);

      foreach (var handler in context.OrderedUpgradeHandlers) {
        handler.OnBeforeExecuteActions(result);
      }

      result.ProcessWith(statementProcessor, ExecuteNonTransactionally);
    }

    public async Task UpgradeSchemaAsync(SchemaExtractionResult extractedSchema,
      StorageModel sourceModel, StorageModel targetModel, ActionSequence upgradeActions, CancellationToken token)
    {
      var result = TranslateActions(extractedSchema, sourceModel, targetModel, upgradeActions);

      foreach (var handler in context.OrderedUpgradeHandlers) {
        await handler.OnBeforeExecuteActionsAsync(result, token).ConfigureAwaitFalse();
      }

      await result.ProcessWithAsync(asyncStatementProcessor, ExecuteNonTransactionallyAsync, token)
        .ConfigureAwaitFalse();
    }

    private UpgradeActionSequence TranslateActions(SchemaExtractionResult extractedSchema, StorageModel sourceModel,
      StorageModel targetModel, ActionSequence upgradeActions)
    {
      var enforceChangedColumns = context.Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();

      var skipConstraints = context.Stage == UpgradeStage.Upgrading;

      var translator = new SqlActionTranslator(
        session.Handlers, executor, context.Services.MappingResolver,
        upgradeActions, extractedSchema, sourceModel, targetModel,
        enforceChangedColumns, !skipConstraints);

      var result = translator.Translate();

      if (SqlLog.IsLogged(LogLevel.Info)) {
        LogStatements(result);
      }

      return result;
    }

    #region Private / internal methods

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      driver.CommitTransaction(null, connection);
      executor.ExecuteMany(batch);
      driver.BeginTransaction(null, connection, null);
    }

    private async Task ExecuteNonTransactionallyAsync(IEnumerable<string> batch, CancellationToken token)
    {
      await driver.CommitTransactionAsync(null, connection, token).ConfigureAwaitFalse();
      await executor.ExecuteManyAsync(batch, token).ConfigureAwaitFalse();
      await driver.BeginTransactionAsync(null, connection, null, token).ConfigureAwaitFalse();
    }

    private void ExecuteTransactionally(IEnumerable<string> batch) => executor.ExecuteMany(batch);

    private Task ExecuteTransactionallyAsync(IEnumerable<string> batch, CancellationToken token) =>
      executor.ExecuteManyAsync(batch, token);

    private void LogStatements(IEnumerable<string> statements)
    {
      SqlLog.Info(
        nameof(Strings.LogSessionXSchemaUpgradeScriptY),
        session.ToStringSafely(),
        driver.BuildBatch(statements.ToArray()).Trim());
    }

    #endregion

    // Constructors

    public SchemaUpgrader(UpgradeContext context, Session session)
    {
      this.context = context;
      this.session = session;

      connection = context.Services.Connection;
      executor = session.Services.Demand<ISqlExecutor>();

      var services = context.Services;
      driver = services.StorageDriver;
      if (driver.ProviderInfo.Supports(ProviderFeatures.TransactionalDdl)) {
        statementProcessor = ExecuteTransactionally;
        asyncStatementProcessor = ExecuteTransactionallyAsync;
      }
      else {
        statementProcessor = ExecuteNonTransactionally;
        asyncStatementProcessor = ExecuteNonTransactionallyAsync;
      }
    }
  }
}