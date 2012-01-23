// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.12.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Storage.Tests.Storage.ByteFieldStorageTestModel;

namespace Xtensive.Storage.Tests.Storage.ByteFieldStorageTestModel
{
  [HierarchyRoot]
  public class BlobEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public byte[] BlobData { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class ByteFieldStorageTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BlobEntity).Assembly, typeof (BlobEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var blobData = new byte[] {0, 1};

      using (Domain.OpenSession()) {
        using (var transactionScope = Session.Current.OpenTransaction()) {
          var blob = new BlobEntity();

          blob.BlobData = blobData;

          transactionScope.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var transactionScope = Session.Current.OpenTransaction()) {
          var blob = Query.All<BlobEntity>().Single();
          Assert.AreEqual(blobData.Length, blob.BlobData.Length);
          Assert.AreEqual(blobData, blob.BlobData);
          transactionScope.Complete();
        }
      }
    }
  }
}