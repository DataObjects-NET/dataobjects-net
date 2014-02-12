// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.07

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using database1 = Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel1;
using database2 = Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel2;

namespace Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTestModel1
{
  [HierarchyRoot]
  public class PassengerCar : Entity, database2.ICar
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int MaxSpeed { get; set; }
  }

  [HierarchyRoot]
  public class Truck : Entity, database2.ICar
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int MaxSpeed { get; set; }
  }

  [HierarchyRoot]
  public class VehiclePassport :Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public database2.ICar Vehicle { get; set; }
  }

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
  public interface ICar : IEntity
  {
    int Id { get; }
    
    int MaxSpeed { get; }
  }

  [HierarchyRoot]
  public class Base2MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string NameField { get; set; }
  }

  [HierarchyRoot]
  public class SportCar : Entity, ICar
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int MaxSpeed { get; set; }
  }
}


namespace Xtensive.Orm.Tests.Model.VariousKeyGeneratorsByOneKeyEqualityIdentifierTest
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

    protected override void PopulateData()
    {
      var aaa = new []{"a","b","c","d","e","f","g","h","i"}; 
      using (var session = Domain.OpenSession().Activate())
      using (var transaction = session.Session.OpenTransaction()) {
        foreach (var s in aaa) {
          new database1.Base1MyEntity {NameField = s};
          new database2.Base2MyEntity {NameField = s};
          new database1.Base1MyAnotherEntity {NameField = s};
        }
        for (var i = 0; i < 500; i++) {
          new database1.PassengerCar {MaxSpeed = i};
          new database1.Truck {MaxSpeed = i};
          new database2.SportCar {MaxSpeed = i * 2};
        }
        transaction.Complete();
      }
    }

    [Test]
    public void EqualityIdentifiersAreEqaulsTest()
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

    [Test]
    public void IntersectionOfGeneratedKeysTest()
    {
      using (var session = Domain.OpenSession().Activate())
      using (var transaction = session.Session.OpenTransaction()) {
        var allBase1Entities = session.Session.Query.All<database1.Base1MyEntity>().Select(el=>el.Id);
        var allBase1Ids = session.Session.Query.All<database1.Base1MyAnotherEntity>().Select(el => el.Id).Union(allBase1Entities);
        var allBase2Ids = session.Session.Query.All<database2.Base2MyEntity>().Select(el => el.Id);
        Assert.AreEqual(allBase1Ids.Intersect(allBase2Ids).Count(), 0);
      }
      using (var session = Domain.OpenSession().Activate())
      using (var transaction = session.Session.OpenTransaction()) {
        var grouping = session.Session.Query.All<database2.ICar>().GroupBy(el => el.Id);
        foreach (var group in grouping) 
          Assert.AreEqual(1, group.Count());
      }
    }

    [Test]
    public void SelectAllInstancesWhichImlementInterfaceTest()
    {
      using (var session = Domain.OpenSession().Activate())
      using (var transaction = session.Session.OpenTransaction()) {
        var query = session.Session.Query.All<database2.ICar>();
        Assert.AreEqual(query.Count(), 1500);
      }
    }

    [Test]
    [ExpectedException(typeof (DomainBuilderException))]
    public void SameSeedValueInDifferentKeyGeneratorsTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "VariousKeyGeneratorsByOneKeyEqualityTestDomain1");
      configuration.Types.Register(typeof (database1.Base1MyEntity).Assembly, typeof (database1.Base1MyEntity).Namespace);
      configuration.Types.Register(typeof (database2.Base2MyEntity).Assembly, typeof (database2.Base2MyEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      BuildDomain(configuration);
    }

    [Test]
    public void AdditionValueFromAnotheDatabaseTest()
    {
      using (var session = Domain.OpenSession().Activate())
      using (var transaction = session.Session.OpenTransaction()) {
        var sportCar = session.Session.Query.All<database2.ICar>().First(el => el is database2.SportCar);
        new database1.VehiclePassport {Vehicle = sportCar};
      }
    }
  }
}
