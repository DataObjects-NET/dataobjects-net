﻿// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.07.18


using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using initialModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1;
using initialModelUpgrader = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel2;
using onlyCustomsTypeIdsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel3;
using partialSequencialTypeIdModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel4;
using partialRandomTypeIdModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel5;
using typeNotExistsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel6;
using typeIdBeyongTheLimitsModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel7;
using conflictModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel8;
using performWithNewType = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel9;
using performWithNewTypes = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel10;
using multyDatabaseModel = Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11;

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
  public class Book : Entity
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
  public class Store : Entity
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
  public class Upgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      base.OnPrepare();
      if (UpgradeContext.Configuration.UpgradeMode != DomainUpgradeMode.Recreate)
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName, 200);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Book).FullName, 201);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.BookInStore).FullName, 202);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Store).FullName, 203);
        UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book", 204);
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel3
{
  public class OnlyCustomIdsUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName, 200);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Book).FullName, 201);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.BookInStore).FullName, 202);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Store).FullName, 203);
      UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book", 204);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel4
{
  public class PartiallySequentialTypeIdsUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName, 200);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Book).FullName, 201);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel5
{
  public class PartiallyRandomTypeIdsUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName, 150);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Book).FullName, 149);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Store).FullName, 250);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.BookInStore).FullName, 110);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel6
{
  public class TypeIdForNotExistingTypeUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName + "1", 100);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel7
{
  public class TypeIdOutOfRangeUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.Author).FullName, 99);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(initialModel.BookInStore).FullName, 100);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel8
{
  [HierarchyRoot]
  public class CommentsOfBook : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public initialModel.Book Book { get; set; }

    [Field]
    public EntitySet<Comment> Comments { get; set; }
  }

  [HierarchyRoot]
  public class Comment : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public CommentsOfBook Book { get; set; }
  }

  public class ConflictOfTypeIdUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(Comment).FullName, 100);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel9
{
  [HierarchyRoot]
  public class CommentsOfBook : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public initialModel.Book Book { get; set; }

    [Field]
    public EntitySet<Comment> Comments { get; set; }
  }

  [HierarchyRoot]
  public class Comment : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public User User { get; set; }
  }

  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Login { get; set; }

    [Field]
    public string Password { get; set; }

    [Field]
    [Association(PairTo = "User")]
    public EntitySet<Comment> Comments { get; set; }
  }

  public class UpgradeWithNewTypeUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(Comment).FullName, 110);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel10
{
  [HierarchyRoot]
  public class CommentsOfBook : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public initialModel.Book Book { get; set; }

    [Field]
    public EntitySet<Comment> Comments { get; set; }
  }

  [HierarchyRoot]
  public class Comment : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public User User { get; set; }
  }

  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Login { get; set; }

    [Field]
    public string Password { get; set; }

    [Field]
    [Association(PairTo = "User")]
    public EntitySet<Comment> Comments { get; set; }
  }

  public class UpgradeWithNewTypesUpgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      UpgradeContext.UserDefinedTypeMap.Add(typeof(Comment).FullName, 110);
      UpgradeContext.UserDefinedTypeMap.Add(typeof(User).FullName, 112);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11
{
  namespace Database1
  {
    [HierarchyRoot]
    public class Book : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class Author : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class Magazine : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    public class Upgrader : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(Book).FullName, 100);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(Author).FullName, 101);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(Magazine).FullName, 102);
        if (UpgradeContext.Configuration.UpgradeMode.In(DomainUpgradeMode.LegacyValidate, DomainUpgradeMode.LegacySkip))
        {
          UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database2.Parent", 200);
          UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database2.Child", 201);

          UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database3.Car", 300);
          UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database3.Engine", 301);
        }
      }
    }
  }

  namespace Database2
  {
    [HierarchyRoot]
    public class Parent : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class Child : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    public class Upgrader : UpgradeHandler
    {
      public override bool IsEnabled
      {
        get { return UpgradeContext.Configuration.UpgradeMode == DomainUpgradeMode.Recreate; }
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database2.Parent", 200);
        UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database2.Child", 201);
      }
    }
  }

  namespace Database3
  {
    [HierarchyRoot]
    public class Car : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class Engine : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    public class Upgrader : UpgradeHandler
    {
      public override bool IsEnabled
      {
        get { return UpgradeContext.Configuration.UpgradeMode == DomainUpgradeMode.Recreate; }
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database3.Car", 300);
        UpgradeContext.UserDefinedTypeMap.Add("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel11.Database3.Engine", 301);
      }
    }
  }
}


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class CustomTypeIdMap : AutoBuildTest
  {
    [Test]
    public void RecreateTest()
    {
      var configuration = BuildConfiguration(typeof(initialModel.Author), typeof(onlyCustomsTypeIdsModel.OnlyCustomIdsUpgrader), DomainUpgradeMode.Recreate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialSequencialTypeIdModel.PartiallySequentialTypeIdsUpgrader), DomainUpgradeMode.Recreate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
      }

      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialRandomTypeIdModel.PartiallyRandomTypeIdsUpgrader), DomainUpgradeMode.Recreate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 110);
        Assert.AreEqual(150, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(149, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(250, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(251, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeNotExistsModel.TypeIdForNotExistingTypeUpgrader), DomainUpgradeMode.Recreate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeIdBeyongTheLimitsModel.TypeIdOutOfRangeUpgrader), DomainUpgradeMode.Recreate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
    }

    [Test]
    public void SkipTest()
    {
      BuildInitialDomain();
      var configuration = BuildConfiguration(typeof(initialModel.Author), DomainUpgradeMode.Skip);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeNotExistsModel.TypeIdForNotExistingTypeUpgrader), DomainUpgradeMode.Skip);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeIdBeyongTheLimitsModel.TypeIdOutOfRangeUpgrader), DomainUpgradeMode.Skip);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
    }

    [Test]
    public void ValidateTest()
    {
      BuildInitialDomain();
      var configuration = BuildConfiguration(typeof(initialModel.Author), DomainUpgradeMode.Validate);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeNotExistsModel.TypeIdForNotExistingTypeUpgrader), DomainUpgradeMode.Validate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeIdBeyongTheLimitsModel.TypeIdOutOfRangeUpgrader), DomainUpgradeMode.Validate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
    }

    [Test]
    public void PerformAndPerformSafelyTest()
    {
      BuildInitialDomain();
      var configuration = BuildConfiguration(typeof(initialModel.Author), DomainUpgradeMode.Perform);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), DomainUpgradeMode.PerformSafely);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(onlyCustomsTypeIdsModel.OnlyCustomIdsUpgrader), DomainUpgradeMode.Perform);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(onlyCustomsTypeIdsModel.OnlyCustomIdsUpgrader), DomainUpgradeMode.PerformSafely);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(conflictModel.ConflictOfTypeIdUpgrader), DomainUpgradeMode.Perform);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(conflictModel.ConflictOfTypeIdUpgrader), DomainUpgradeMode.PerformSafely);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(performWithNewType.UpgradeWithNewTypeUpgrader), DomainUpgradeMode.Perform);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(performWithNewType.Comment)].TypeId);
        Assert.AreEqual(113, domain.Model.Types[typeof(performWithNewType.User)].TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(performWithNewType.UpgradeWithNewTypeUpgrader), DomainUpgradeMode.PerformSafely);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(performWithNewType.Comment)].TypeId);
        Assert.AreEqual(113, domain.Model.Types[typeof(performWithNewType.User)].TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(performWithNewTypes.UpgradeWithNewTypesUpgrader), DomainUpgradeMode.Perform);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(performWithNewTypes.Comment)].TypeId);
        Assert.AreEqual(112, domain.Model.Types[typeof(performWithNewTypes.User)].TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(performWithNewTypes.UpgradeWithNewTypesUpgrader), DomainUpgradeMode.PerformSafely);
      using (var domain = BuildDomain(configuration))
      {
        Assert.AreNotEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreNotEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreNotEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreNotEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreNotEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(performWithNewTypes.Comment)].TypeId);
        Assert.AreEqual(112, domain.Model.Types[typeof(performWithNewTypes.User)].TypeId);
      }
    }

    [Test]
    public void LegacySkipTest()
    {
      BuildInitialDomain();
      var configuration = BuildConfiguration(typeof(initialModel.Author), typeof(onlyCustomsTypeIdsModel.OnlyCustomIdsUpgrader), DomainUpgradeMode.LegacySkip);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialSequencialTypeIdModel.PartiallySequentialTypeIdsUpgrader), DomainUpgradeMode.LegacySkip);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialRandomTypeIdModel.PartiallyRandomTypeIdsUpgrader), DomainUpgradeMode.LegacySkip);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 110);
        Assert.AreEqual(150, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(149, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(250, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(251, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeNotExistsModel.TypeIdForNotExistingTypeUpgrader), DomainUpgradeMode.LegacySkip);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeIdBeyongTheLimitsModel.TypeIdOutOfRangeUpgrader), DomainUpgradeMode.LegacySkip);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
    }

    [Test]
    public void LegacyValidateTest()
    {
      BuildInitialDomain();
      var configuration = BuildConfiguration(typeof(initialModel.Author), typeof(onlyCustomsTypeIdsModel.OnlyCustomIdsUpgrader), DomainUpgradeMode.LegacyValidate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(203, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(202, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(204, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialSequencialTypeIdModel.PartiallySequentialTypeIdsUpgrader), DomainUpgradeMode.LegacyValidate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 200);
        Assert.AreEqual(200, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(initialModel.Book)].TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(partialRandomTypeIdModel.PartiallyRandomTypeIdsUpgrader), DomainUpgradeMode.LegacyValidate);
      using (var domain = BuildDomain(configuration))
      {
        foreach (var type in domain.Model.Types.Where(type => type.IsEntity && !type.IsSystem))
          Assert.GreaterOrEqual(type.TypeId, 110);
        Assert.AreEqual(150, domain.Model.Types[typeof(initialModel.Author)].TypeId);
        Assert.AreEqual(149, domain.Model.Types[typeof(initialModel.Book)].TypeId);
        Assert.AreEqual(250, domain.Model.Types[typeof(initialModel.Store)].TypeId);
        Assert.AreEqual(110, domain.Model.Types[typeof(initialModel.BookInStore)].TypeId);
        Assert.AreEqual(251, domain.Model.Types.Find("Xtensive.Orm.Tests.Upgrade.CustomTypeIdModel1.EntitySetItems.Author-Books-Book").TypeId);
      }

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeNotExistsModel.TypeIdForNotExistingTypeUpgrader), DomainUpgradeMode.LegacyValidate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));

      BuildInitialDomain();
      configuration = BuildConfiguration(typeof(initialModel.Author), typeof(typeIdBeyongTheLimitsModel.TypeIdOutOfRangeUpgrader), DomainUpgradeMode.LegacyValidate);
      Assert.Throws<DomainBuilderException>(() => BuildDomain(configuration));
    }

    [Test]
    public void LegacySkipMultiDatabaseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase | ProviderFeatures.Multischema);
      var expectedMap = new Dictionary<string, int>();
      BuildInitialDomains();
      var firstConfiguration = DomainConfigurationFactory.Create();
      firstConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      firstConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      firstConfiguration.DefaultDatabase = "DO-Tests-2";
      firstConfiguration.DefaultSchema = "dbo";
      firstConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2"));
      using (var domain = BuildDomain(firstConfiguration))
      {
        expectedMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
      }

      var secondConfiguration = DomainConfigurationFactory.Create();
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      secondConfiguration.UpgradeMode = DomainUpgradeMode.LegacySkip;
      secondConfiguration.DefaultDatabase = "DO-Tests-2";
      secondConfiguration.DefaultSchema = "dbo";
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-1"));
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2"));
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-3"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));

      using (var domain = BuildDomain(secondConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        foreach (var map in expectedMap)
        {
          int typeId;
          currentMap.TryGetValue(map.Key, out typeId);
          Assert.AreEqual(map.Value, typeId);
        }
        Assert.AreEqual(200, domain.Model.Types[typeof(multyDatabaseModel.Database2.Parent)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(multyDatabaseModel.Database2.Child)].TypeId);
        Assert.AreEqual(300, domain.Model.Types[typeof(multyDatabaseModel.Database3.Car)].TypeId);
        Assert.AreEqual(301, domain.Model.Types[typeof(multyDatabaseModel.Database3.Engine)].TypeId);
      }

      using (var domain = BuildDomain(firstConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        Assert.AreEqual(expectedMap.Count, currentMap.Count);
        Assert.AreEqual(expectedMap, currentMap);
      }

      using (var domain = BuildDomain(secondConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        foreach (var map in expectedMap)
        {
          int typeId;
          currentMap.TryGetValue(map.Key, out typeId);
          Assert.AreEqual(map.Value, typeId);
        }
        Assert.AreEqual(200, domain.Model.Types[typeof(multyDatabaseModel.Database2.Parent)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(multyDatabaseModel.Database2.Child)].TypeId);
        Assert.AreEqual(300, domain.Model.Types[typeof(multyDatabaseModel.Database3.Car)].TypeId);
        Assert.AreEqual(301, domain.Model.Types[typeof(multyDatabaseModel.Database3.Engine)].TypeId);
      }

      using (var domain = BuildDomain(secondConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        foreach (var map in expectedMap)
        {
          int typeId;
          currentMap.TryGetValue(map.Key, out typeId);
          Assert.AreEqual(map.Value, typeId);
        }
        Assert.AreEqual(200, domain.Model.Types[typeof(multyDatabaseModel.Database2.Parent)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(multyDatabaseModel.Database2.Child)].TypeId);
        Assert.AreEqual(300, domain.Model.Types[typeof(multyDatabaseModel.Database3.Car)].TypeId);
        Assert.AreEqual(301, domain.Model.Types[typeof(multyDatabaseModel.Database3.Engine)].TypeId);
      }

      using (var domain = BuildDomain(firstConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        Assert.AreEqual(expectedMap.Count, currentMap.Count);
        Assert.AreEqual(expectedMap, currentMap);
      }

      using (var domain = BuildDomain(secondConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        foreach (var map in expectedMap)
        {
          int typeId;
          currentMap.TryGetValue(map.Key, out typeId);
          Assert.AreEqual(map.Value, typeId);
        }
        Assert.AreEqual(200, domain.Model.Types[typeof(multyDatabaseModel.Database2.Parent)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(multyDatabaseModel.Database2.Child)].TypeId);
        Assert.AreEqual(300, domain.Model.Types[typeof(multyDatabaseModel.Database3.Car)].TypeId);
        Assert.AreEqual(301, domain.Model.Types[typeof(multyDatabaseModel.Database3.Engine)].TypeId);
      }
    }

    [Test]
    public void LegacyValidateMultiDatabaseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase | ProviderFeatures.Multischema);
      var expectedMap = new Dictionary<string, int>();
      BuildInitialDomains();
      var firstConfiguration = DomainConfigurationFactory.Create();
      firstConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      firstConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      firstConfiguration.DefaultDatabase = "DO-Tests-2";
      firstConfiguration.DefaultSchema = "dbo";
      firstConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2"));
      using (var domain = BuildDomain(firstConfiguration))
      {
        expectedMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
      }

      var secondConfiguration = DomainConfigurationFactory.Create();
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      secondConfiguration.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      secondConfiguration.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      secondConfiguration.DefaultDatabase = "DO-Tests-2";
      secondConfiguration.DefaultSchema = "dbo";
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-1"));
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2"));
      secondConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-3"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      secondConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));

      using (var domain = BuildDomain(secondConfiguration))
      {
        var currentMap = domain.Model.Types.ToDictionary(key => key.UnderlyingType.FullName, value => value.TypeId);
        foreach (var map in expectedMap)
        {
          int typeId;
          currentMap.TryGetValue(map.Key, out typeId);
          Assert.AreEqual(map.Value, typeId);
        }
        Assert.AreEqual(200, domain.Model.Types[typeof(multyDatabaseModel.Database2.Parent)].TypeId);
        Assert.AreEqual(201, domain.Model.Types[typeof(multyDatabaseModel.Database2.Child)].TypeId);
        Assert.AreEqual(300, domain.Model.Types[typeof(multyDatabaseModel.Database3.Car)].TypeId);
        Assert.AreEqual(301, domain.Model.Types[typeof(multyDatabaseModel.Database3.Engine)].TypeId);
      }
    }

    [Test]
    public void UserTypeIdLimits()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase | ProviderFeatures.Multischema);
      var rightConfiguration = DomainConfigurationFactory.Create();
      rightConfiguration.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      rightConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      rightConfiguration.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      rightConfiguration.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      rightConfiguration.DefaultDatabase = "DO-Tests-2";
      rightConfiguration.DefaultSchema = "dbo";
      rightConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-1") { MinTypeId = 200, MaxTypeId = 299 });
      rightConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2") { MinTypeId = 100, MaxTypeId = 199 });
      rightConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-3") { MinTypeId = 300, MaxTypeId = 399 });
      rightConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      rightConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      rightConfiguration.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));

      using (var domain = BuildDomain(rightConfiguration)) { }

      var wrongConfiguration1 = DomainConfigurationFactory.Create();
      wrongConfiguration1.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      wrongConfiguration1.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      wrongConfiguration1.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      wrongConfiguration1.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      wrongConfiguration1.DefaultDatabase = "DO-Tests-2";
      wrongConfiguration1.DefaultSchema = "dbo";
      wrongConfiguration1.Databases.Add(new DatabaseConfiguration("DO-Tests-1") { MinTypeId = 500, MaxTypeId = 600 });
      wrongConfiguration1.Databases.Add(new DatabaseConfiguration("DO-Tests-2") { MinTypeId = 100, MaxTypeId = 199 });
      wrongConfiguration1.Databases.Add(new DatabaseConfiguration("DO-Tests-3") { MinTypeId = 300, MaxTypeId = 399 });
      wrongConfiguration1.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      wrongConfiguration1.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      wrongConfiguration1.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));
      Assert.Throws<DomainBuilderException>(() => BuildDomain(wrongConfiguration1));

      var wrongConfiguration2 = DomainConfigurationFactory.Create();
      wrongConfiguration2.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      wrongConfiguration2.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      wrongConfiguration2.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      wrongConfiguration2.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      wrongConfiguration2.DefaultDatabase = "DO-Tests-2";
      wrongConfiguration2.DefaultSchema = "dbo";
      wrongConfiguration2.Databases.Add(new DatabaseConfiguration("DO-Tests-1") { MinTypeId = 200, MaxTypeId = 299 });
      wrongConfiguration2.Databases.Add(new DatabaseConfiguration("DO-Tests-2") { MinTypeId = 500, MaxTypeId = 600 });
      wrongConfiguration2.Databases.Add(new DatabaseConfiguration("DO-Tests-3") { MinTypeId = 300, MaxTypeId = 399 });
      wrongConfiguration2.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      wrongConfiguration2.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      wrongConfiguration2.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));
      Assert.Throws<DomainBuilderException>(() => BuildDomain(wrongConfiguration2));


      var wrongConfiguration3 = DomainConfigurationFactory.Create();
      wrongConfiguration3.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      wrongConfiguration3.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      wrongConfiguration3.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      wrongConfiguration3.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      wrongConfiguration3.DefaultDatabase = "DO-Tests-2";
      wrongConfiguration3.DefaultSchema = "dbo";
      wrongConfiguration3.Databases.Add(new DatabaseConfiguration("DO-Tests-1") { MinTypeId = 200, MaxTypeId = 299 });
      wrongConfiguration3.Databases.Add(new DatabaseConfiguration("DO-Tests-2") { MinTypeId = 100, MaxTypeId = 199 });
      wrongConfiguration3.Databases.Add(new DatabaseConfiguration("DO-Tests-3") { MinTypeId = 500, MaxTypeId = 600 });
      wrongConfiguration3.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace, "DO-Tests-1", "dbo"));
      wrongConfiguration3.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace, "DO-Tests-2", "dbo"));
      wrongConfiguration3.MappingRules.Add(new MappingRule(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace, "DO-Tests-3", "dbo"));
      Assert.Throws<DomainBuilderException>(() => BuildDomain(wrongConfiguration3));
    }

    private void BuildInitialDomain()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(initialModel.Author).Assembly, typeof(initialModel.Author).Namespace);
      configuration.Types.Register(typeof(initialModelUpgrader.Upgrader));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      using (var domain = BuildDomain(configuration)) { }
    }

    private void BuildInitialDomains()
    {
      var firstConfiguration = DomainConfigurationFactory.Create();
      firstConfiguration.Types.Register(typeof(multyDatabaseModel.Database1.Book).Assembly, typeof(multyDatabaseModel.Database1.Book).Namespace);
      firstConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      firstConfiguration.DefaultDatabase = "DO-Tests-2";
      firstConfiguration.DefaultSchema = "dbo";
      firstConfiguration.Databases.Add(new DatabaseConfiguration("DO-Tests-2"));
      var domain1 = BuildDomain(firstConfiguration);
      domain1.Dispose();

      var secondConfig = DomainConfigurationFactory.Create();
      secondConfig.Types.Register(typeof(multyDatabaseModel.Database2.Child).Assembly, typeof(multyDatabaseModel.Database2.Child).Namespace);
      secondConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      secondConfig.DefaultDatabase = "DO-Tests-1";
      secondConfig.DefaultSchema = "dbo";
      secondConfig.Databases.Add(new DatabaseConfiguration("DO-Tests-1"));
      var domain2 = BuildDomain(secondConfig);
      domain2.Dispose();

      var thirdConfig = DomainConfigurationFactory.Create();
      thirdConfig.Types.Register(typeof(multyDatabaseModel.Database3.Car).Assembly, typeof(multyDatabaseModel.Database3.Car).Namespace);
      thirdConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      thirdConfig.DefaultDatabase = "DO-Tests-3";
      thirdConfig.DefaultSchema = "dbo";
      thirdConfig.Databases.Add(new DatabaseConfiguration("DO-Tests-3"));
      var domain3 = BuildDomain(thirdConfig);
      domain3.Dispose();
    }

    private DomainConfiguration BuildConfiguration(Type baseNamespaceType, DomainUpgradeMode mode)
    {
      return BuildConfiguration(baseNamespaceType, null, mode);
    }

    private DomainConfiguration BuildConfiguration(Type baseNamespaceType, Type additionalNamespaceType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(baseNamespaceType.Assembly, baseNamespaceType.Namespace);

      if (additionalNamespaceType != null)
        configuration.Types.Register(additionalNamespaceType.Assembly, additionalNamespaceType.Namespace);

      configuration.UpgradeMode = mode;
      return configuration;
    }
  }
}
