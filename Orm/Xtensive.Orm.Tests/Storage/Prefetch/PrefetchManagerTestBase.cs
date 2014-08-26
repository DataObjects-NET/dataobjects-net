// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  public class PrefetchManagerTestBase : AutoBuildTest
  {
    protected TypeInfo CustomerType;
    protected TypeInfo OrderType;
    protected TypeInfo ProductType;
    protected TypeInfo BookType;
    protected TypeInfo TitleType;
    protected TypeInfo ITitleType;
    protected TypeInfo OfferContainerType;
    protected FieldInfo PersonIdField;
    protected FieldInfo AgeField;
    protected FieldInfo CityField;
    protected FieldInfo CustomerField;
    protected FieldInfo EmployeeField;
    protected FieldInfo OrderIdField;
    protected FieldInfo DetailsField;
    protected FieldInfo BooksField;
    protected FieldInfo BookTitleField;
    protected FieldInfo TitleBookField;
    protected FieldInfo LanguageField;
    protected FieldInfo TextField;
    protected System.Reflection.FieldInfo GraphContainersField;
    protected System.Reflection.FieldInfo PrefetchProcessorField;

    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CustomerType = Domain.Model.Types[typeof (Customer)];
      OrderType = Domain.Model.Types[typeof (Order)];
      ProductType = Domain.Model.Types[typeof (Product)];
      BookType = Domain.Model.Types[typeof (Book)];
      TitleType = Domain.Model.Types[typeof (Title)];
      ITitleType = Domain.Model.Types[typeof (ITitle)];
      OfferContainerType = Domain.Model.Types[typeof (OfferContainer)];
      PersonIdField = Domain.Model.Types[typeof (Person)].Fields["Id"];
      OrderIdField = Domain.Model.Types[typeof (Order)].Fields["Id"];
      CityField = CustomerType.Fields["City"];
      AgeField = Domain.Model.Types[typeof (AdvancedPerson)].Fields["Age"];
      CustomerField = OrderType.Fields["Customer"];
      EmployeeField = OrderType.Fields["Employee"];
      DetailsField = OrderType.Fields["Details"];
      BooksField = Domain.Model.Types[typeof (Author)].Fields["Books"];
      BookTitleField = BookType.Fields["Title"];
      TitleBookField = TitleType.Fields["Book"];
      LanguageField = TitleType.Fields["Language"];
      TextField = TitleType.Fields["Text"];
      GraphContainersField = typeof (PrefetchManager).GetField("graphContainers",
        BindingFlags.NonPublic | BindingFlags.Instance);
      PrefetchProcessorField = typeof (SqlSessionHandler).GetField("prefetchManager",
        BindingFlags.NonPublic | BindingFlags.Instance);
      PrefetchTestHelper.FillDataBase(Domain);
    }

    protected Key GetFirstKey<T>()
      where T : Entity
    {
      Key result;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
        result = session.Query.All<T>().OrderBy(o => o.Key).First().Key;
      return result;
    }

    internal static void ValidateLoadedEntitySet(Key key, FieldInfo field, int count, bool isFullyLoaded,
      Session session)
    {
      EntitySetState state;
      Assert.IsTrue(session.Handler.LookupState(key, field, out state));
      Assert.AreEqual(count, state.Count());
      Assert.AreEqual(isFullyLoaded, state.IsFullyLoaded);
    }

    internal GraphContainer GetSingleGraphContainer(PrefetchManager prefetchManager)
    {
      return ((IEnumerable<GraphContainer>) GraphContainersField.GetValue(prefetchManager)).Single();
    }

    internal static bool IsFieldKeyOrSystem(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem;
    }
  }
}