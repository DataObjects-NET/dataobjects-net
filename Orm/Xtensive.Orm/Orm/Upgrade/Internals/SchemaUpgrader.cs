// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  internal sealed class SchemaUpgrader : IDisposable
  {
    private readonly UpgradeContext context;
    private readonly Session session;
    private readonly UpgradeServiceAccessor services;
    private readonly ISqlExecutor executor;
    private readonly Action<IEnumerable<string>> statementProcessor;

    private TransactionScope transactionScope;

    public void UpgradeSchema(SqlExtractionResult extractedSchema,
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

      if (Providers.Log.IsLogged(LogEventTypes.Info))
        LogStatements(result);

      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeExecuteActions(result);

      result.ProcessWith(statementProcessor, ExecuteNonTransactionally);
    }

    #region Private / internal methods

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      Complete();
      executor.ExecuteDdl(batch);
      Start();
    }

    private void ExecuteTransactionally(IEnumerable<string> batch)
    {
      executor.ExecuteDdl(batch);
    }

    private void LogStatements(IEnumerable<string> statements)
    {
      Providers.Log.Info(
        Strings.LogSessionXSchemaUpgradeScriptY,
        session.ToStringSafely(),
        services.Driver.BuildBatch(statements.ToArray()).Trim());
    }

    public void Dispose()
    {
      if (transactionScope==null)
        return;
      try {
        transactionScope.Dispose();
      }
      finally {
        transactionScope = null;
      }
    }

    private void Start()
    {
      transactionScope = session.OpenTransaction();
    }

    public void Complete()
    {
      transactionScope.Complete();
      transactionScope.Dispose();
      transactionScope = null;
    }

    #endregion

    // Constructors

    public SchemaUpgrader(UpgradeContext context, Session session)
    {
      this.context = context;
      this.session = session;

      services = context.Services;
      executor = session.Services.Demand<ISqlExecutor>();

      if (services.Driver.ProviderInfo.Supports(ProviderFeatures.TransactionalDdl))
        statementProcessor = ExecuteTransactionally;
      else
        statementProcessor = ExecuteNonTransactionally;

      Start();
    }
  }
}