// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Modelling.Actions;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Upgrade;
using ModelTypeInfo = Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    private DomainHandler DomainHandler { get { return (DomainHandler) Handlers.DomainHandler; } }
    private SessionHandler SessionHandler { get { return (SessionHandler) BuildingContext.Current.SystemSessionHandler; } }
    private SqlConnection Connection { get { return ((SessionHandler) Handlers.SessionHandler).Connection; } }
    private Driver Driver { get { return DomainHandler.Driver; } }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();
      var translator = new SqlActionTranslator(
        upgradeActions,
        GetStorageSchema(),
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, Driver,
        Handlers.NameBuilder.TypeIdColumnName,
        enforceChangedColumns,
        SessionHandler.ExecuteScalar, SessionHandler.ExecuteNonQuery);

      translator.Translate();

      LogTranslatedStatements(translator);

      Execute(translator.PreUpgradeCommands);
      Execute(translator.UpgradeCommands);
      Execute(translator.DataManipulateCommands);
      Execute(translator.PostUpgradeCommands);

      var context = BuildingContext.Demand();
      context.NonTransactionalAction = () => Execute(translator.NonTransactionalCommands);
    }

    /// <inheritdoc/>
    public override StorageInfo GetExtractedSchema()
    {
      var schema = GetStorageSchema();
      var converter = new SqlModelConverter(schema, DomainHandler.ProviderInfo);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override ModelTypeInfo CreateTypeInfo(Type type, int? length, int? precision, int? scale)
    {
      var sqlValueType = DomainHandler.Driver.BuildValueType(type, length, precision, scale);
      return new ModelTypeInfo(sqlValueType.Type.ToClrType(), sqlValueType.Length, sqlValueType.Scale, sqlValueType.Precision);
    }

    private void Execute(IEnumerable<string> batch)
    {
      if (DomainHandler.ProviderInfo.Supports(ProviderFeatures.DdlBatches)) {
        var commandText = Driver.BuildBatch(batch.ToArray());
        if (string.IsNullOrEmpty(commandText))
          return;
        var command = Connection.CreateCommand(commandText);
        using (command) {
          Driver.ExecuteNonQuery(null, command);
        }
      }
      else {
        foreach (var commandText in batch) {
          if (string.IsNullOrEmpty(commandText))
            continue;
          var command = Connection.CreateCommand(commandText);
          using (command) {
            Driver.ExecuteNonQuery(null, command);
          }
        }
      }
    }

    private Schema GetStorageSchema()
    {
      var context = UpgradeContext.Demand();
      var schema = context.NativeExtractedSchema as Schema;
      if (schema == null) {
        schema = DomainHandler.Driver.ExtractSchema(SessionHandler.Connection);
        SaveSchemaInContext(schema);
      }
      return schema;
    }

    private void LogTranslatedStatements(SqlActionTranslator translator)
    {
      if (!Log.IsLogged(LogEventTypes.Info))
        return;

      var batch = 
        EnumerableUtils.One(Driver.BatchBegin)
        .Concat(translator.PreUpgradeCommands)
        .Concat(translator.UpgradeCommands)
        .Concat(translator.DataManipulateCommands)
        .Concat(translator.PostUpgradeCommands)
        .Concat(EnumerableUtils.One(Driver.BatchEnd))
        .ToArray();

      var session = SessionHandler!=null ? SessionHandler.Session : null;
      Log.Info(Strings.LogSessionXSchemaUpgradeScriptY,
        session.GetFullNameSafely(),
        Driver.BuildBatch(batch).Trim());
    }
  }
}