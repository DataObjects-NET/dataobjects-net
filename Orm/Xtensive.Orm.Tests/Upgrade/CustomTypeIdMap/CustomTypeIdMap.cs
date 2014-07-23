// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.07.18


using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using initialModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1;
using onlyCustomsTypeIdsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2;
using partialSequencialTypeIdModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel3;
using partialRandomTypeIdModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel4;
using typeNotExistsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel5;
using typeIdBeyongTheLimitsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel6;

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Book")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Book")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }

  public class Upgrader: UpgradeHandler
  {
    public override bool  CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void  OnPrepare()
    {
      UpgradeContext.CustomTypeIdMap.Add(typeof (Author).FullName, 200);
      UpgradeContext.CustomTypeIdMap.Add(typeof (Book).FullName, 201);
      UpgradeContext.CustomTypeIdMap.Add(typeof (BookInStore).FullName, 202);
      UpgradeContext.CustomTypeIdMap.Add(typeof (Store).FullName, 203);
      UpgradeContext.CustomTypeIdMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2.EntitySetItems.Author-Books-Book", 204);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel3
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }

  public class Upgrader: UpgradeHandler
  {
    public override bool  CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void  OnPrepare()
    {
      UpgradeContext.CustomTypeIdMap.Add(typeof (Author).FullName, 200);
      UpgradeContext.CustomTypeIdMap.Add(typeof (Book).FullName, 201);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel4
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }

  public class Upgrader: UpgradeHandler
  {
    public override bool  CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void  OnPrepare()
    {
      UpgradeContext.CustomTypeIdMap.Add(typeof (Author).FullName, 150);
      UpgradeContext.CustomTypeIdMap.Add(typeof (BookInStore).FullName, 149);
      UpgradeContext.CustomTypeIdMap.Add(typeof (Store).FullName, 250);
      UpgradeContext.CustomTypeIdMap.Add(typeof (BookInStore).FullName, 110);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel5
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }

  public class Upgrader: UpgradeHandler
  {
    public override bool  CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void  OnPrepare()
    {
      UpgradeContext.CustomTypeIdMap.Add(typeof(Author).FullName + "1", 100);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel6
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public string Somestring { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; } 
  }

  [HierarchyRoot]
  public class Book: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> Stores { get; set; } 
  }

  [HierarchyRoot]
  public class Store: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Store")]
    public EntitySet<BookInStore> BooksInStore { get; set; }
  }

  [HierarchyRoot]
  public class BookInStore : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Store Store { get; set; }

    [Field]
    public int Count { get; set; }
  }

  public class Upgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.CustomTypeIdMap.Add(typeof(Author).FullName, 99);
      UpgradeContext.CustomTypeIdMap.Add(typeof(BookInStore).FullName, 100);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{

  [TestFixture]
  public class CustomTypeIdMap : AutoBuildTest
  {
    private IEnumerable<DomainUpgradeMode> upgradeModes  = Enum.GetValues(typeof(DomainUpgradeMode)).Cast<DomainUpgradeMode>();

    [Test]
    public void OnlyCustomTypeIdMap()
    {
      foreach (var domainUpgradeMode in upgradeModes) {
        if (domainUpgradeMode==DomainUpgradeMode.Perform || domainUpgradeMode==DomainUpgradeMode.PerformSafely)
          continue;
        if (domainUpgradeMode!=DomainUpgradeMode.Recreate)
          BuildInitialDomain();

        var configuration = BuildConfiguration(typeof (onlyCustomsTypeIdsModel.Author), domainUpgradeMode);
        var domain = BuildDomain(configuration);
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof (onlyCustomsTypeIdsModel.Author)]);
        Assert.AreEqual(201, domain.Model.Types[typeof (onlyCustomsTypeIdsModel.Book)]);
        Assert.AreEqual(203, domain.Model.Types[typeof (onlyCustomsTypeIdsModel.Store)]);
        Assert.AreEqual(202, domain.Model.Types[typeof (onlyCustomsTypeIdsModel.BookInStore)]);
        Assert.AreEqual(204, domain.Model.Types["Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2.EntitySetItems.Author-Books-Book"]);
      }
    }

    [Test]
    public void PartialSequencialCustomTypeIdMap()
    {
      foreach (var domainUpgradeMode in upgradeModes) {
        if (domainUpgradeMode==DomainUpgradeMode.Perform || domainUpgradeMode==DomainUpgradeMode.PerformSafely)
          continue;
        if (domainUpgradeMode!=DomainUpgradeMode.Recreate)
          BuildInitialDomain();
        var configuration = BuildConfiguration(typeof (partialSequencialTypeIdModel.Author), domainUpgradeMode);
        var domain = BuildDomain(configuration);
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof (partialSequencialTypeIdModel.Author)]);
        Assert.AreEqual(201, domain.Model.Types[typeof (partialSequencialTypeIdModel.Book)]);
      }
    }

    [Test]
    public void PartialRandomCustomTypeIdMap()
    {
      foreach (var domainUpgradeMode in upgradeModes) {
        if (domainUpgradeMode==DomainUpgradeMode.Perform || domainUpgradeMode==DomainUpgradeMode.PerformSafely)
          continue;
        if (domainUpgradeMode==DomainUpgradeMode.Recreate) 
          BuildInitialDomain();
        var configuration = BuildConfiguration(typeof (partialRandomTypeIdModel.Author), domainUpgradeMode);
        var domain = BuildDomain(configuration);
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 110);
        Assert.AreEqual(150, domain.Model.Types[typeof (partialRandomTypeIdModel.Author)]);
        Assert.AreEqual(149, domain.Model.Types[typeof (partialRandomTypeIdModel.BookInStore)]);
        Assert.AreEqual(250, domain.Model.Types[typeof (partialRandomTypeIdModel.Store)]);
        Assert.AreEqual(110, domain.Model.Types[typeof (partialRandomTypeIdModel.BookInStore)]);
        Assert.AreEqual(251, domain.Model.Types["Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2.EntitySetItems.Author-Books-Book"]);
      }
    }

    [Test]
    public void TypeInCustomTypeTdMapNotExistsInDomain()
    {
      foreach (var domainUpgradeMode in upgradeModes) {
        if (domainUpgradeMode==DomainUpgradeMode.Perform || domainUpgradeMode==DomainUpgradeMode.PerformSafely)
          continue;
        if (domainUpgradeMode==DomainUpgradeMode.Recreate)
          BuildInitialDomain();
        var configuration = BuildConfiguration(typeof(typeNotExistsModel.Author), domainUpgradeMode);
        Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
      }
    }

    [Test]
    public void TypeIdBeyongTheLimits()
    {
      foreach (var domainUpgradeMode in upgradeModes) {
        if (domainUpgradeMode==DomainUpgradeMode.Perform || domainUpgradeMode==DomainUpgradeMode.PerformSafely)
          continue;
        if (domainUpgradeMode!=DomainUpgradeMode.Recreate)
          BuildInitialDomain();
        var configuration = BuildConfiguration(typeof (typeIdBeyongTheLimitsModel.Author), domainUpgradeMode);
        Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
      }
    }

    [Test]
    public void PerformAndPerformSafelyTest()
    {
      
    }

    private void BuildInitialDomain()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (initialModel.Author).Assembly, typeof (initialModel.Author).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      var domain = BuildDomain(configuration);
    }

    private DomainConfiguration BuildConfiguration(Type type, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(type.Assembly, type.Namespace);
      configuration.UpgradeMode = mode;
      return configuration;
    }
  }
}
