// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.31

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0430_PartialIndexOnBoolOrEnumFieldsModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0430_PartialIndexOnBoolOrEnumFieldsModel
  {
    [HierarchyRoot, Index("Value", Filter = "FilterExpression")]
    public class EntityWithFilteredIndexWithBooleanField : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set;  }

      [Field]
      public bool? IsActive { get; set; }

      private static Expression<Func<EntityWithFilteredIndexWithBooleanField, bool>> FilterExpression()
      {
        return e => e.IsActive==true;
      }
    }

    public enum ObjectState
    {
      Disabled,
      Active,
    }

    [HierarchyRoot, Index("Value", Filter = "FilterExpression")]
    public class EntityWithFilteredIndexWithEnumField : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }

      [Field]
      public ObjectState ObjState { get; set; }

      private static Expression<Func<EntityWithFilteredIndexWithEnumField, bool>> FilterExpression()
      {
        return e => e.ObjState==ObjectState.Active;
      }
    }
  }

  [TestFixture]
  public class IssueJira0430_PartialIndexOnBoolOrEnumFields
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.PartialIndexes);
    }

    [Test]
    public void BoolTest()
    {
      RunTest(typeof (EntityWithFilteredIndexWithBooleanField));
    }

    [Test]
    public void EnumTest()
    {
      RunTest(typeof (EntityWithFilteredIndexWithEnumField));
    }

    private void RunTest(Type type)
    {
      BuildDomain(type, DomainUpgradeMode.Recreate);
      BuildDomain(type, DomainUpgradeMode.Validate);
    }

    private void BuildDomain(Type type, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(type);
      configuration.UpgradeMode = upgradeMode;
      Domain.Build(configuration).Dispose();
    }
  }
}