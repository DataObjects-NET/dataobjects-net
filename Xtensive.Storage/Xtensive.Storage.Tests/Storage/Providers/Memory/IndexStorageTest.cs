// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Memory;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage.Providers.Memory
{
  using Building;
  using DomainHandler = Xtensive.Storage.Providers.Memory.DomainHandler;
  using Xtensive.Storage.Model.Convert;
  using System;
  
  [TestFixture]
  public class IndexStorageTest
  {
    private IndexStorage storage;
    private Domain domain;
    
    [SetUp]
    public void BuildStorage()
    {
      var config = new DomainConfiguration("memory://localhost/DO40-Tests");
      config.Types.Register(typeof(Address).Assembly, typeof(Address).Namespace);
      config.BuildMode = DomainBuildMode.Recreate;
      
      domain = Domain.Build(config);
      storage = ((DomainHandler) domain.Handler).GetIndexStorage() as IndexStorage;
    }
    
    public void DestroyStorage()
    {
      if (domain != null)
        domain.DisposeSafely();
    }

    [Test]
    public void FillTest()
    {
      DataBaseFiller.Fill(domain);
    }
    
    [Test]
    public void CommandsTest()
    {
      Key order1;
      Key order2;

      // Insert
      using (var s = domain.OpenSession())
      using (var t = Transaction.Open()) {
        order1 = new Order
          {
            ProcessingTime = TimeSpan.FromDays(2),
            Freight = 123
          }.Key;
        order2 = new Order
          {
            ProcessingTime = TimeSpan.FromDays(3),
            Freight = 456
          }.Key;
        t.Complete();
      }
      Dump(order1.EntityType);

      // Update
      using (var s = domain.OpenSession())
      using (var t = Transaction.Open()) {
        ((Order) order1.Resolve()).Freight = 321;
        ((Order) order1.Resolve()).ProcessingTime = TimeSpan.FromDays(4);
        ((Order) order2.Resolve()).Freight = 654;
        ((Order) order2.Resolve()).ProcessingTime = TimeSpan.FromDays(6);
        t.Complete();
      }
      Dump(order1.EntityType);

      // Remove
      using (var s = domain.OpenSession())
      using (var t = Transaction.Open()) {
        order1.Resolve().Remove();
        t.Complete();
      }
      Dump(order1.EntityType);
    }

    public void Dump(TypeInfo type)
    {
      foreach (var indexInfo in type.AffectedIndexes.Where(index => index.IsPrimary)) {
        DumpDomainIndexData(indexInfo.MappingName);
      }
      foreach (var indexInfo in type.AffectedIndexes.Where(index => index.IsPrimary)) {
        DumpStorageIndexData(indexInfo.MappingName);
      }
    }

    public void DumpStorageIndexData(string name)
    {
      var indexTable = storage.Model.Tables.Single(table => table.PrimaryIndex.Name==name);
      var index = storage.GetRealIndex(indexTable.PrimaryIndex);
      foreach (var tuple in index) {
        Log.Info("Storage: {0}", tuple.ToString());
      }
    }

    public void DumpDomainIndexData(string name)
    {
      var indexInfo = domain.Model.RealIndexes.Single(i => i.MappingName==name);
      var index = ((DomainHandler) domain.Handler).GetRealIndex(indexInfo);
      foreach (var tuple in index) {
        Log.Info("Domain:  {0}", tuple.ToString());
      }
    }

  }
}

# region Test model

namespace Xtensive.Storage.Tests.Storage.Providers.Memory.Model
{

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class A : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Col1 { get; set; }
  }
}

#endregion