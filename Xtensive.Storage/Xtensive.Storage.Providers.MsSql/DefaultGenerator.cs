// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.29

using System;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  public class DefaultGenerator : Storage.DefaultGenerator
  {
    private Table generatorTable;

    public override Tuple Next()
    {
      throw new System.NotImplementedException();
    }

    public override void Initialize()
    {
      base.Initialize();
      var keyColumn = Hierarchy.Columns[0];
      var domainHandler = (DomainHandler)Handlers.DomainHandler;
      generatorTable = domainHandler.Catalog.DefaultSchema.CreateTable(Hierarchy.MappingName);


      SqlBatch batch = SqlFactory.Batch();


//      SqlFactory.Create()
//      Table table =  catalog.DefaultSchema.CreateTable(primaryIndex.ReflectedType.Name);
//      SqlFactory.Create()
    }
  }
}