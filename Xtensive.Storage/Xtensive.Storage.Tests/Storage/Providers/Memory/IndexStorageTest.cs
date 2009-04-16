// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Index;

namespace Xtensive.Storage.Tests.Storage.Providers.Memory
{
  using Model;
  using Building;
  using DomainHandler = Xtensive.Storage.Providers.Memory.DomainHandler;
  
  
  [TestFixture]
  public class IndexStorageTest
  {
    private Domain domain;
    
    [SetUp]
    public void BuildTest()
    {
      var config = new DomainConfiguration("memory://localhost/DO40-Tests");
      config.Types.Register(typeof(A).Assembly, typeof(A).Namespace);
      config.BuildMode = DomainBuildMode.Recreate;
      domain = Domain.Build(config);
    }

    [Test]
    public void CreateStorageSchemaTest()
    {
      using (var sc =domain.OpenSession()) {
        using (sc.Session.OpenTransaction()) {
          var model = ((SessionHandler) Session.Current.Handler).StorageView.Model;
          Assert.IsNotNull(model);
        }
      }
    }

    [Test]
    public void CommandsTest()
    {
      Key a;
      using (var sc = domain.OpenSession()) {
        using (var tc = sc.Session.OpenTransaction()) {
          a = new A {Col1 = "A"}.Key;
          tc.Complete();
        }
      }

      using (var sc = domain.OpenSession()) {
        using (var tc = sc.Session.OpenTransaction()) {
          a.Resolve<A>().Col1 = "B";
          tc.Complete();
        }
      }

      using (var sc = domain.OpenSession()) {
        using (var tc = sc.Session.OpenTransaction()) {
          a.Resolve().Remove();
          tc.Complete();
        }
      }
    }


  }
}

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