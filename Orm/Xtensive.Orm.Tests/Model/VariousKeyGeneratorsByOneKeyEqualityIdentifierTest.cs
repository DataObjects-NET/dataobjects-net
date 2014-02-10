// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.07

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using database1 = Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel1;
using database2 = Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel2;

namespace Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel1
{
  [HierarchyRoot]
  public class Base1MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string NameField { get; set; }
  }

  [HierarchyRoot]
  public class Base1MyAnotherEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string NameField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel2
{
  [HierarchyRoot]
  public class Base2MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string NameField { get; set; }
  }
}


namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class VariousKeyGeneratorsByOneKeyEqualityIdentifierTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "VariousKeyGeneratorsByOneKeyEqualityTestDomain");
      configuration.Types.Register(typeof (database1.Base1MyEntity).Assembly, typeof (database1.Base1MyEntity).Namespace);
      configuration.Types.Register(typeof (database2.Base2MyEntity).Assembly, typeof (database2.Base2MyEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    [Test]
    public void Test01()
    {
      TypeInfo base1MyEntity;
      Domain.Model.Types.TryGetValue(typeof (database1.Base1MyEntity), out base1MyEntity);
      TypeInfo base1AnotherEntity;
      Domain.Model.Types.TryGetValue(typeof (database1.Base1MyAnotherEntity), out base1AnotherEntity);
      TypeInfo base2MyEntity;
      Domain.Model.Types.TryGetValue(typeof (database2.Base2MyEntity), out base2MyEntity);

      Assert.AreEqual(base1MyEntity.Key.EqualityIdentifier.Equals(base2MyEntity.Key.EqualityIdentifier), true);
      Assert.AreEqual(base1MyEntity.Key.EqualityIdentifier.Equals(base1AnotherEntity.Key.EqualityIdentifier), true);
    }
  }
}
