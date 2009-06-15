// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;
using TypeInfo=Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.PgSql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    
  }
}