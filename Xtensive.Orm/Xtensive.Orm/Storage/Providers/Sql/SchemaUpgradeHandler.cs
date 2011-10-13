// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Building;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Orm.Upgrade;
using ModelTypeInfo = Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    protected DomainHandler DomainHandler { get { return (DomainHandler) Handlers.DomainHandler; } }
    protected SessionHandler SessionHandler { get { return (SessionHandler) BuildingContext.Demand().SystemSessionHandler; } }
    protected Driver Driver { get { return DomainHandler.Driver; } }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var queryExecutor = SessionHandler.GetService<IQueryExecutor>(true);

      var cachingKeyGeneratorService = (CachingKeyGeneratorService)DomainHandler.Domain.Services.Get<ICachingKeyGeneratorService>();
      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();
      var translator = new SqlActionTranslator(
        upgradeActions,
        (Schema) GetNativeExtractedSchema(),
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, Driver,
        Handlers.NameBuilder.TypeIdColumnName,
        enforceChangedColumns,
        queryExecutor.ExecuteScalar, 
        queryExecutor.ExecuteNonQuery,
        cachingKeyGeneratorService.GetCurrentValueImplementation);

      var result = translator.Translate();

      LogTranslatedStatements(translator);

        var upgradeContext = UpgradeContext.Current;
        if (upgradeContext!=null)
          foreach (var pair in upgradeContext.UpgradeHandlers)
            pair.Value.OnBeforeExecuteActions(result);

      var context = UpgradeContext.Demand();
      if (result.NonTransactionalPrologCommands.Count > 0) {
        context.TransactionScope.Complete();
        context.TransactionScope.Dispose();
        Execute(result.NonTransactionalPrologCommands);
        context.TransactionScope = SessionHandler.Session.OpenTransaction();
      }

      Execute(result.CleanupDataCommands);
      Execute(result.PreUpgradeCommands);
      Execute(result.UpgradeCommands);
      Execute(result.CopyDataCommands);
      Execute(result.PostCopyDataCommands);
      Execute(result.CleanupCommands);

      if (result.NonTransactionalEpilogCommands.Count > 0) {
        context.TransactionScope.Complete();
        context.TransactionScope.Dispose();
        Execute(result.NonTransactionalEpilogCommands);
        context.TransactionScope = SessionHandler.Session.OpenTransaction();
      }
    }

    /// <inheritdoc/>
    protected override StorageInfo ExtractSchema()
    {
      var schema = (Schema) GetNativeExtractedSchema(); // Must rely on this method to avoid multiple extractions
      var converter = new SqlModelConverter(schema, DomainHandler.ProviderInfo);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override object ExtractNativeSchema()
    {
      return DomainHandler.Driver.ExtractSchema(SessionHandler.Connection);
    }

    /// <inheritdoc/>
    protected override Orm.Upgrade.StorageModelBuilder GetStorageModelBuilder()
    {
      return new StorageModelBuilder(DomainHandler);
    }

    protected virtual void Execute(IEnumerable<string> batch)
    {
      if (DomainHandler.ProviderInfo.Supports(ProviderFeatures.DdlBatches)) {
        IEnumerable<IEnumerable<string>> subbatches = SplitToSubbatches(batch);
        foreach (var subbatch in subbatches) {
          var commandText = Driver.BuildBatch(subbatch.ToArray());
          if (string.IsNullOrEmpty(commandText))
            return;
          var command = SessionHandler.Connection.CreateCommand(commandText);
          using (command) {
            Driver.ExecuteNonQuery(null, command);
          }
        }
      }
      else {
        foreach (var commandText in batch) {
          if (string.IsNullOrEmpty(commandText))
            continue;
          var command = SessionHandler.Connection.CreateCommand(commandText);
          using (command) {
            Driver.ExecuteNonQuery(null, command);
          }
        }
      }
    }

    private static IEnumerable<IEnumerable<string>> SplitToSubbatches(IEnumerable<string> batch)
    {
      var subbatch = new List<string>();
      foreach (string item in batch) {
        if (item.IsNullOrEmpty()) {
          if (subbatch.Count==0)
            continue;
          yield return subbatch;
          subbatch = new List<string>();
        }
        else {
          subbatch.Add(item);
        }
      }
      if (subbatch.Count!=0)
        yield return subbatch;
    }

    private void LogTranslatedStatements(SqlActionTranslator translator)
    {
      if (!Log.IsLogged(LogEventTypes.Info))
        return;

      var batch = 
        EnumerableUtils.One(Driver.BatchBegin)
        .Concat(translator.CleanupDataCommands)
        .Concat(translator.PreUpgradeCommands)
        .Concat(translator.UpgradeCommands)
        .Concat(translator.CopyDataCommands)
        .Concat(translator.PostCopyDataCommands)
        .Concat(translator.CleanupCommands)
        .Concat(EnumerableUtils.One(Driver.BatchEnd))
        .ToArray();

      var session = SessionHandler!=null ? SessionHandler.Session : null;
      Log.Info(Strings.LogSessionXSchemaUpgradeScriptY,
        session.ToStringSafely(),
        Driver.BuildBatch(batch).Trim());
    }
  }
}