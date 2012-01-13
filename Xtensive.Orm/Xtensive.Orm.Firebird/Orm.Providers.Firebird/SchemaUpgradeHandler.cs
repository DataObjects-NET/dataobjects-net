// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.28

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Building;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Providers.Firebird
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    protected override void Execute(IEnumerable<string> batch)
    {
      var context = UpgradeContext.Demand();
      context.TransactionScope.Complete();
      context.TransactionScope.Dispose();

      foreach (var commandText in batch) {
        if (string.IsNullOrEmpty(commandText))
          continue;
        var command = SessionHandler.Connection.CreateCommand(commandText);
        using (command) {
          context.TransactionScope = SessionHandler.Session.OpenTransaction();
          command.Transaction = SessionHandler.Connection.ActiveTransaction;
          Driver.ExecuteNonQuery(null, command);
          context.TransactionScope.Complete();
          context.TransactionScope.Dispose();
        }
      }
      context.TransactionScope = SessionHandler.Session.OpenTransaction();
    }
  }
}