// Copyright (C) 2014-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2014.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Tests.Linq.DynamicallyDefinedFieldsModel;

namespace Xtensive.Orm.Tests.Linq.DynamicallyDefinedFieldsModel
{
  [HierarchyRoot]
  public class Area : Entity
  {
    [Field, Key]
    public long ID { get; set; }
 
    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field]
    public EntitySet<SomeClass> Somes { get; private set; }
  }

  [HierarchyRoot]
  public class Group : Entity
  {
    [Field, Key]
    public long ID { get; set; }
    
    [Field]
    public string Value { get; set; }
  }

  [HierarchyRoot]
  public class SomeClass : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Area Area { get; set; }
  }

  public class GeoLocation : Structure
  {
    [Field]
    public double Latitude { get; set; }

    [Field]
    public double Longitude { get; set; }
  }

  public interface ITestInterface : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    string InterfaseImplementorName { get; set; }
  }

  [HierarchyRoot]
  public class TestInterfaceImplementor : Entity, ITestInterface
  {
    public int Id { get; private set; }

    public string InterfaseImplementorName { get; set; }
  }

  [HierarchyRoot]
  public class Removable : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Area Area { get; set; }

    [Field]
    public decimal Decimal { get; set; }
  }

  public class Module : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      DefineDynamicFields(model);
    }

    private void DefineDynamicFields(DomainModelDef model)
    {
      var testData = TestData.CorrectData;
      var areaTypeInfo = model.Types[typeof (Area)];
      areaTypeInfo.DefineField(testData.GroupFieldName, typeof (Group));
      areaTypeInfo.DefineField(testData.GeoLocationFieldName, typeof (GeoLocation));
      areaTypeInfo.DefineField(testData.SomeClassesFieldName, typeof (EntitySet<SomeClass>));
      areaTypeInfo.DefineField(testData.IntFieldName, typeof (int));
      areaTypeInfo.DefineField(testData.StringFieldName, typeof (string));
      areaTypeInfo.DefineField(testData.DoubleFieldName, typeof (double));
      areaTypeInfo.DefineField(testData.DateTimeFieldName, typeof (DateTime));
      areaTypeInfo.DefineField(testData.ByteArrayFieldName, typeof (byte[]));
      areaTypeInfo.DefineField(testData.ByteFieldName, typeof (byte));
      areaTypeInfo.DefineField(testData.DecimalFieldName, typeof (decimal));
      areaTypeInfo.DefineField(testData.LongFieldName, typeof (long));
      areaTypeInfo.DefineField(testData.FloatFieldName, typeof (float));
      areaTypeInfo.DefineField(testData.InterfaceImplementorFieldName, typeof (ITestInterface));
      areaTypeInfo.DefineField(testData.BooleanFieldName, typeof (bool));

      var structureTypeInfo = model.Types[typeof (GeoLocation)];
      structureTypeInfo.DefineField(testData.GeoLocationDynamicFieldName, typeof (int));

      var someClass = model.Types[typeof (SomeClass)];
      someClass.DefineField("DynamicField", typeof (int));
    }
  }

  public class TestData
  {
    public static TestData CorrectData { get; private set; }

    public string AreaName { get; set; }

    public string GroupValue { get; set; }
    public Key Group { get; set; }
    public string GroupFieldName { get; set; }

    public Key SomeClassKey { get; set; }
    public string SomeClassesFieldName { get; set; }

    public GeoLocation GeoLocation { get; set; }
    public string GeoLocationFieldName { get; set; }
    public int GeoLocationDynamicFieldValue { get; set; }
    public string GeoLocationDynamicFieldName { get; set; }

    public bool BooleanFieldValue { get; set; }
    public string BooleanFieldName { get; set; }

    public byte ByteFieldValue { get; set; }
    public string ByteFieldName { get; set; }

    public int IntFieldValue { get; set; }
    public string IntFieldName { get; set; }

    public long LongFieldValue { get; set; }
    public string LongFieldName { get; set; }

    public float FloatFieldValue { get; set; }
    public string FloatFieldName { get; set; }

    public double DoubleFieldValue { get; set; }
    public string DoubleFieldName { get; set; }

    public decimal DecimalFieldValue { get; set; }
    public string DecimalFieldName { get; set; }

    public byte[] ByteArrayFieldValue { get; set; }
    public string ByteArrayFieldName { get; set; }

    public DateTime DateTimeFieldValue { get; set; }
    public string DateTimeFieldName { get; set; }

    public string StringFieldValue { get; set; }
    public string StringFieldName { get; set; }

    public Key InterfaceImplementorKey { get; set; }
    public string InterfaceImplementorFieldName { get; set; }

    static TestData()
    {
      CorrectData = new TestData();
    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class DynamicallyDefinedFields : AutoBuildTest
  {
    private readonly TestData testData = TestData.CorrectData;

    [Test]
    public void WhereTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);
        
        var storedArea = session.Query.All<Area>()
          .Where(el => (Group)el[testData.GroupFieldName]==group)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group ,someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass))
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (int)el[testData.IntFieldName]==testData.IntFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (long)el[testData.LongFieldName]==testData.LongFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
        
        storedArea = session.Query.All<Area>()
          .Where(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .Where(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          storedArea = session.Query.All<Area>()
            .Where(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue)
            .FirstOrDefault();
          Assert.That(storedArea, Is.Not.Null);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        storedArea = session.Query.All<Area>()
          .Where(el => (string)el[testData.StringFieldName]==testData.StringFieldValue)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
        
        storedArea = session.Query.All<Area>()
          .Where(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor)
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void AllTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        Assert.That(session.Query.All<Area>().All(el => (Group)el[testData.GroupFieldName]==group), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass)), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (int)el[testData.IntFieldName]==testData.IntFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (long)el[testData.LongFieldName]==testData.LongFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue), Is.True);
        if (!IsOracle())
          Assert.That(session.Query.All<Area>().All(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (string)el[testData.StringFieldName]==testData.StringFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().All(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor), Is.True);
      }
    }

    [Test]
    public void AnyTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        Assert.That(session.Query.All<Area>().Any(el => (Group)el[testData.GroupFieldName]==group), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass)), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (int)el[testData.IntFieldName]==testData.IntFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (long)el[testData.LongFieldName]==testData.LongFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue), Is.True);
        if (!IsOracle())
          Assert.That(session.Query.All<Area>().Any(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (string)el[testData.StringFieldName]==testData.StringFieldValue), Is.True);
        Assert.That(session.Query.All<Area>().Any(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor), Is.True);
      }
    }

    [Test]
    public void CountTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        Assert.That(session.Query.All<Area>().Count(el => (Group)el[testData.GroupFieldName]==group), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass)), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (int)el[testData.IntFieldName]==testData.IntFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (long)el[testData.LongFieldName]==testData.LongFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue), Is.EqualTo(1));
        if (!IsOracle())
          Assert.That(session.Query.All<Area>().Count(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (string)el[testData.StringFieldName]==testData.StringFieldValue), Is.EqualTo(1));
        Assert.That(session.Query.All<Area>().Count(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor), Is.EqualTo(1));
      }
    }


    [Test]
    public void FirstTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (Group)el[testData.GroupFieldName]==group));
        var storedArea = session.Query.All<Area>().First(el => (Group)el[testData.GroupFieldName]==group);
        TestFields(storedArea,group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation));
        storedArea = session.Query.All<Area>().First(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue));
        storedArea = session.Query.All<Area>().First(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass)));
        storedArea = session.Query.All<Area>().First(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass));
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue));
        storedArea = session.Query.All<Area>().First(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (int)el[testData.IntFieldName]==testData.IntFieldValue));
        storedArea = session.Query.All<Area>().First(el => (int)el[testData.IntFieldName]==testData.IntFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (long)el[testData.LongFieldName]==testData.LongFieldValue));
        storedArea = session.Query.All<Area>().First(el => (long)el[testData.LongFieldName]==testData.LongFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue));
        storedArea = session.Query.All<Area>().First(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue));
        storedArea = session.Query.All<Area>().First(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue));
        storedArea = session.Query.All<Area>().First(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue));
        storedArea = session.Query.All<Area>().First(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue));
          storedArea = session.Query.All<Area>().First(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (string)el[testData.StringFieldName]==testData.StringFieldValue));
        storedArea = session.Query.All<Area>().First(el => (string)el[testData.StringFieldName]==testData.StringFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().First(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor));
        storedArea = session.Query.All<Area>().First(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var storedArea = session.Query.All<Area>().FirstOrDefault(el => (Group)el[testData.GroupFieldName]==group);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass));
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (int)el[testData.IntFieldName]==testData.IntFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (long)el[testData.LongFieldName]==testData.LongFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          storedArea = session.Query.All<Area>().FirstOrDefault(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue);
          Assert.That(storedArea, Is.Not.Null);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (string)el[testData.StringFieldName]==testData.StringFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().FirstOrDefault(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void GroupByTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var storedArea = session.Query.All<Area>().GroupBy(el => (Group)el[testData.GroupFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea1 = session.Query.All<Area>().GroupBy(el => (GeoLocation)el[testData.GeoLocationFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea2 = session.Query.All<Area>().GroupBy(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));
        
        var storedArea3 = session.Query.All<Area>().GroupBy(el => (byte)el[testData.ByteFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea4 = session.Query.All<Area>().GroupBy(el => (int)el[testData.IntFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea5 = session.Query.All<Area>().GroupBy(el => (long)el[testData.LongFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea6 = session.Query.All<Area>().GroupBy(el => (float)el[testData.FloatFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea7 = session.Query.All<Area>().GroupBy(el => (double)el[testData.DoubleFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea8 = session.Query.All<Area>().GroupBy(el => (bool)el[testData.BooleanFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea9 = session.Query.All<Area>().GroupBy(el => (decimal)el[testData.DecimalFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        if (!IsOracle()) {
          var storedArea10 = session.Query.All<Area>().GroupBy(el => (byte[])el[testData.ByteArrayFieldName]).ToArray();
          Assert.That(storedArea, Is.Not.Null);
          Assert.That(storedArea.Length, Is.EqualTo(1));
        }

        var storedArea11 = session.Query.All<Area>().GroupBy(el => (string)el[testData.StringFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));

        var storedArea12 = session.Query.All<Area>().GroupBy(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]).ToArray();
        Assert.That(storedArea, Is.Not.Null);
        Assert.That(storedArea.Length, Is.EqualTo(1));
      }
    }

    [Test]
    public void OrderByTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var storedArea = session.Query.All<Area>()
          .OrderBy(el => (Group)el[testData.GroupFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (GeoLocation)el[testData.GeoLocationFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (int) ((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]))
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (byte)el[testData.ByteFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (int)el[testData.IntFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (long)el[testData.LongFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (float)el[testData.FloatFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (double)el[testData.DoubleFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (bool)el[testData.BooleanFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (decimal)el[testData.DecimalFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          storedArea = session.Query.All<Area>()
            .OrderBy(el => (byte[])el[testData.ByteArrayFieldName])
            .FirstOrDefault();

          Assert.That(storedArea, Is.Not.Null);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (string)el[testData.StringFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>()
          .OrderBy(el => (ITestInterface)el[testData.InterfaceImplementorFieldName])
          .FirstOrDefault();
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void SelectTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var groups = session.Query.All<Area>()
          .Select(el => (Group)el[testData.GroupFieldName])
          .ToArray();
        Assert.That(groups, Is.Not.Null);
        Assert.That(groups.Length, Is.EqualTo(1));
        Assert.That(groups[0].Value, Is.EqualTo(testData.GroupValue));

        var locations = session.Query.All<Area>()
          .Select(el => (GeoLocation)el[testData.GeoLocationFieldName])
          .ToArray();
        Assert.That(locations, Is.Not.Null);
        Assert.That(locations.Length, Is.EqualTo(1));
        Assert.That(locations[0].Latitude, Is.EqualTo(testData.GeoLocation.Latitude));
        Assert.That(locations[0].Longitude, Is.EqualTo(testData.GeoLocation.Longitude));

        var locInts = session.Query.All<Area>()
          .Select(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName])
          .ToArray();
        Assert.That(locInts, Is.Not.Null);
        Assert.That(locations.Length, Is.EqualTo(1));
        Assert.That(locInts[0], Is.EqualTo(testData.GeoLocationDynamicFieldValue));

        var entitySets = session.Query.All<Area>()
          .Select(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]))
          .ToArray();
        Assert.That(entitySets, Is.Not.Null);
        Assert.That(entitySets.Length, Is.EqualTo(1));
        Assert.That(entitySets[0].Count, Is.EqualTo(1));
        Assert.That(entitySets[0].Contains(someClass), Is.True);
        Assert.That(entitySets[0].FirstOrDefault(), Is.EqualTo(someClass));

        var byteValues = session.Query.All<Area>()
          .Select(el => (byte)el[testData.ByteFieldName])
          .ToArray();
        Assert.That(byteValues, Is.Not.Null);
        Assert.That(byteValues.Length, Is.EqualTo(1));
        Assert.That(byteValues[0], Is.EqualTo(testData.ByteFieldValue));

        var intValues = session.Query.All<Area>()
          .Select(el => (int)el[testData.IntFieldName])
          .ToArray();
        Assert.That(intValues, Is.Not.Null);
        Assert.That(intValues.Length, Is.EqualTo(1));
        Assert.That(intValues[0], Is.EqualTo(testData.IntFieldValue));

        var longs = session.Query.All<Area>()
          .Select(el => (long)el[testData.LongFieldName])
          .ToArray();
        Assert.That(longs, Is.Not.Null);
        Assert.That(longs.Length, Is.EqualTo(1));
        Assert.That(longs[0], Is.EqualTo(testData.LongFieldValue));

        var floats = session.Query.All<Area>()
          .Select(el => (float)el[testData.FloatFieldName])
          .ToArray();
        Assert.That(floats, Is.Not.Null);
        Assert.That(floats.Length, Is.EqualTo(1));
        Assert.That(floats[0], Is.EqualTo(testData.FloatFieldValue));

        var doubles = session.Query.All<Area>()
          .Select(el => (double)el[testData.DoubleFieldName])
          .ToArray();
        Assert.That(doubles, Is.Not.Null);
        Assert.That(doubles.Length, Is.EqualTo(1));
        Assert.That(doubles[0], Is.EqualTo(testData.DoubleFieldValue));

        var booleans = session.Query.All<Area>()
          .Select(el => (bool)el[testData.BooleanFieldName])
          .ToArray();
        Assert.That(booleans, Is.Not.Null);
        Assert.That(booleans.Length, Is.EqualTo(1));
        Assert.That(booleans[0], Is.EqualTo(testData.BooleanFieldValue));

        var decimals = session.Query.All<Area>()
          .Select(el => (decimal)el[testData.DecimalFieldName])
          .ToArray();
        Assert.That(decimals, Is.Not.Null);
        Assert.That(decimals.Length, Is.EqualTo(1));
        Assert.That(decimals[0], Is.EqualTo(testData.DecimalFieldValue));

        var byteArrays = session.Query.All<Area>()
          .Select(el => (byte[])el[testData.ByteArrayFieldName])
          .ToArray();
        Assert.That(byteArrays, Is.Not.Null);
        Assert.That(byteArrays.Length, Is.EqualTo(1));
        Assert.That(byteArrays[0].Length, Is.EqualTo(testData.ByteArrayFieldValue.Length));
        for (int i = 0; i < testData.ByteArrayFieldValue.Length; i++)
          Assert.That(byteArrays[0][i], Is.EqualTo(testData.ByteArrayFieldValue[i]));

        var stringValues = session.Query.All<Area>()
          .Select(el => (string)el[testData.StringFieldName])
          .ToArray();
        Assert.That(stringValues, Is.Not.Null);
        Assert.That(stringValues.Length, Is.EqualTo(1));
        Assert.That(string.IsNullOrEmpty(stringValues[0]), Is.False);
        Assert.That(stringValues[0], Is.EqualTo(testData.StringFieldValue));

        var implementations = session.Query.All<Area>()
          .Select(el => (ITestInterface)el[testData.InterfaceImplementorFieldName])
          .ToArray();
        Assert.That(implementations, Is.Not.Null);
        Assert.That(implementations.Length, Is.EqualTo(1));
        var implementation = implementations[0];
        Assert.That(implementation.GetType(), Is.EqualTo(interfaceImplementor.GetType()));
        Assert.That(implementation, Is.EqualTo(interfaceImplementor));
      }
    }

    public void SelectManyTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        SomeClass[] someClasses;
        Assert.DoesNotThrow(
          ()=> {
            someClasses = session.Query.All<Area>()
              .SelectMany(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]))
              .ToArray();
          });
        someClasses = session.Query.All<Area>()
              .SelectMany(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]))
              .ToArray();
        Assert.That(someClasses, Is.Not.Null);
        Assert.That(someClasses.Length, Is.EqualTo(1));
        Assert.That(someClasses[0], Is.EqualTo(someClass));
      }
    }

    [Test]
    public void SingeTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (Group)el[testData.GroupFieldName]==group));
        var storedArea = session.Query.All<Area>().Single(el => (Group)el[testData.GroupFieldName]==group);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation));
        storedArea = session.Query.All<Area>().Single(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass)));
        storedArea = session.Query.All<Area>().Single(el => ((EntitySet<SomeClass>) el[testData.SomeClassesFieldName]).Contains(someClass));
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (int)el[testData.IntFieldName]==testData.IntFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (int)el[testData.IntFieldName]==testData.IntFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (long)el[testData.LongFieldName]==testData.LongFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (long)el[testData.LongFieldName]==testData.LongFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue));
          storedArea = session.Query.All<Area>().Single(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (string)el[testData.StringFieldName]==testData.StringFieldValue));
        storedArea = session.Query.All<Area>().Single(el => (string)el[testData.StringFieldName]==testData.StringFieldValue);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        Assert.DoesNotThrow(() => session.Query.All<Area>().Single(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor));
        storedArea = session.Query.All<Area>().Single(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void SingleOrDefault()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);
        
        var storedArea = session.Query.All<Area>().SingleOrDefault(el => (Group)el[testData.GroupFieldName]==group);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (GeoLocation)el[testData.GeoLocationFieldName]==testData.GeoLocation);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (int)((GeoLocation)el[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName]==testData.GeoLocationDynamicFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName]).Contains(someClass));
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (byte)el[testData.ByteFieldName]==testData.ByteFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (int)el[testData.IntFieldName]==testData.IntFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (long)el[testData.LongFieldName]==testData.LongFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (float)el[testData.FloatFieldName]==testData.FloatFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (double)el[testData.DoubleFieldName]==testData.DoubleFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (bool)el[testData.BooleanFieldName]==testData.BooleanFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (decimal)el[testData.DecimalFieldName]==testData.DecimalFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        if (!IsOracle()) {
          storedArea = session.Query.All<Area>().SingleOrDefault(el => (byte[])el[testData.ByteArrayFieldName]==testData.ByteArrayFieldValue);
          Assert.That(storedArea, Is.Not.Null);
          TestFields(storedArea, group, someClass, interfaceImplementor);
        }

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (string)el[testData.StringFieldName]==testData.StringFieldValue);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);

        storedArea = session.Query.All<Area>().SingleOrDefault(el => (ITestInterface)el[testData.InterfaceImplementorFieldName]==interfaceImplementor);
        Assert.That(storedArea, Is.Not.Null);
        TestFields(storedArea, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void IndirectAppearToDynamicallyDefinedField()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var allRemovable = session.Query.All<Removable>();
        var array = allRemovable.Select(el=> new {Area = el.Area, Decimal = el.Decimal})
          .GroupBy(f => f.Area)
          .Select(el => new { Area = el.Key, Sum = el.Sum(c => c.Decimal) }).ToArray();
        Assert.That(array, Is.Not.Null);
        Assert.That(array.Length, Is.EqualTo(1));
      }
    }

    [Test]
    public void MethodResultAsIndexedInstanceTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<Area>()
              .Select(el => (((EntitySet<SomeClass>)el["SomeClasses"]).FirstOrDefault())["DynamicField"])
              .ToArray();
        Assert.That(queryResult.Length, Is.EqualTo(1));
        Assert.That(queryResult[0], Is.Not.Null);
        Assert.That(queryResult[0], Is.EqualTo(12));
      }
    }

    [Test]
    public void MethodResultAsIndexerTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<Area>()
              .SelectMany(el => ((EntitySet<SomeClass>)el[testData.SomeClassesFieldName.Substring(0,4) + testData.SomeClassesFieldName.Substring(4)]))
              .ToArray();
        Assert.That(queryResult.Length, Is.EqualTo(1));
        Assert.That(queryResult[0], Is.Not.Null);
      }
    }

    [Test]
    public void SimpleMaxTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var maxDecimal = session.Query.All<Area>().Max(area => (decimal)area[testData.DecimalFieldName]);
        Assert.That(maxDecimal, Is.EqualTo(testData.DecimalFieldValue));

        var maxByte = session.Query.All<Area>().Max(area => (byte)area[testData.ByteFieldName]);
        Assert.That(maxByte, Is.EqualTo(testData.ByteFieldValue));

        var maxInt = session.Query.All<Area>().Max(area => (int)area[testData.IntFieldName]);
        Assert.That(maxInt, Is.EqualTo(testData.IntFieldValue));

        var maxLong = session.Query.All<Area>().Max(area => (long)area[testData.LongFieldName]);
        Assert.That(maxLong, Is.EqualTo(testData.LongFieldValue));

        var maxFloat = session.Query.All<Area>().Max(area => (float)area[testData.FloatFieldName]);
        Assert.That(maxFloat, Is.EqualTo(testData.FloatFieldValue));

        var maxDouble = session.Query.All<Area>().Max(area => (double)area[testData.DoubleFieldName]);
        Assert.That(maxDouble, Is.EqualTo(testData.DoubleFieldValue));

        var maxDateTime = session.Query.All<Area>().Max(area => (DateTime)area[testData.DateTimeFieldName]);
        Assert.That(maxDateTime, Is.Not.EqualTo(default(DateTime)));
        Assert.That(maxDateTime, Is.EqualTo(testData.DateTimeFieldValue));

        var maxString = session.Query.All<Area>().Max(area => (string)area[testData.StringFieldName]);
        Assert.That(maxString, Is.Not.Null);
        Assert.That(maxString, Is.EqualTo(testData.StringFieldValue));
      }
    }

    [Test]
    public void ComplexMaxTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var area = session.Query.All<Area>().First(p => (decimal)p[testData.DecimalFieldName]==session.Query.All<Area>().Max(el => (decimal)el[testData.DecimalFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (byte)p[testData.ByteFieldName]==session.Query.All<Area>().Max(el => (byte)el[testData.ByteFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (int)p[testData.IntFieldName]==session.Query.All<Area>().Max(el => (int)el[testData.IntFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (long)p[testData.LongFieldName]==session.Query.All<Area>().Max(el => (long)el[testData.LongFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (float)p[testData.FloatFieldName]==session.Query.All<Area>().Max(el => (float)el[testData.FloatFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (double)p[testData.DoubleFieldName]==session.Query.All<Area>().Max(el => (double)el[testData.DoubleFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void MinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var minDecimal = session.Query.All<Area>().Min(area => (decimal)area[testData.DecimalFieldName]);
        Assert.That(minDecimal, Is.EqualTo(testData.DecimalFieldValue));

        var minByte = session.Query.All<Area>().Min(area => (byte)area[testData.ByteFieldName]);
        Assert.That(minByte, Is.EqualTo(testData.ByteFieldValue));

        var minInt = session.Query.All<Area>().Min(area => (int)area[testData.IntFieldName]);
        Assert.That(minInt, Is.EqualTo(testData.IntFieldValue));

        var minLong = session.Query.All<Area>().Min(area => (long)area[testData.LongFieldName]);
        Assert.That(minLong, Is.EqualTo(testData.LongFieldValue));

        var minFloat = session.Query.All<Area>().Min(area => (float)area[testData.FloatFieldName]);
        Assert.That(minFloat, Is.EqualTo(testData.FloatFieldValue));

        var minDouble = session.Query.All<Area>().Min(area => (double)area[testData.DoubleFieldName]);
        Assert.That(minDouble, Is.EqualTo(testData.DoubleFieldValue));

        var minDateTime = session.Query.All<Area>().Min(area => (DateTime)area[testData.DateTimeFieldName]);
        Assert.That(minDateTime, Is.Not.EqualTo(default(DateTime)));
        Assert.That(minDateTime, Is.EqualTo(testData.DateTimeFieldValue));

        var minString = session.Query.All<Area>().Min(area => (string)area[testData.StringFieldName]);
        Assert.That(minString, Is.Not.Null);
        Assert.That(minString, Is.EqualTo(testData.StringFieldValue));
      }
    }

    [Test]
    public void ComplexMinTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);

      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var area = session.Query.All<Area>().First(p => (decimal)p[testData.DecimalFieldName]==session.Query.All<Area>().Min(el => (decimal)el[testData.DecimalFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (byte)p[testData.ByteFieldName]==session.Query.All<Area>().Min(el => (byte)el[testData.ByteFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (int)p[testData.IntFieldName]==session.Query.All<Area>().Min(el => (int)el[testData.IntFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (long)p[testData.LongFieldName]==session.Query.All<Area>().Min(el => (long)el[testData.LongFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (float)p[testData.FloatFieldName]==session.Query.All<Area>().Min(el => (float)el[testData.FloatFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(p => (double)p[testData.DoubleFieldName]==session.Query.All<Area>().Min(el => (double)el[testData.DoubleFieldName]));
        TestFields(area, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void InTest()
    {
      var localDecimalCollection = new decimal[] { 0m, testData.DecimalFieldValue, 100.1m };
      var localIntCollection = new int[] {0, testData.IntFieldValue, 122};
      var localLongCollection = new long[] {2, testData.LongFieldValue, 245};
      var localByteCollection = new byte[] {8, testData.ByteFieldValue, 120};
      var localFloatCollection = new float[] {0.5f, testData.FloatFieldValue, 1.25f};
      var localDoubleCollection = new double[] {0.5, testData.DoubleFieldValue, 3};
      var localStringCollection = new[] {"aaa", testData.StringFieldValue, "bbb"};

      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var group = session.Query.Single<Group>(testData.Group);
        var someClass = session.Query.Single<SomeClass>(testData.SomeClassKey);
        var interfaceImplementor = session.Query.Single<TestInterfaceImplementor>(testData.InterfaceImplementorKey);

        var area = session.Query.All<Area>().First(el => ((byte)el[testData.ByteFieldName]).In(localByteCollection));
        TestFields(area ,group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((int)el[testData.IntFieldName]).In(localIntCollection));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((decimal)el[testData.DecimalFieldName]).In(localDecimalCollection));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((long)el[testData.LongFieldName]).In(localLongCollection));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((float)el[testData.FloatFieldName]).In(localFloatCollection));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((double)el[testData.DoubleFieldName]).In(localDoubleCollection));
        TestFields(area, group, someClass, interfaceImplementor);

        area = session.Query.All<Area>().First(el => ((string)el[testData.StringFieldName]).In(localStringCollection));
        TestFields(area, group, someClass, interfaceImplementor);
      }
    }

    [Test]
    public void SumTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var byteSum = session.Query.All<Area>().Sum(area => (byte)area[testData.ByteFieldName]);
        Assert.That(byteSum, Is.EqualTo(testData.ByteFieldValue));

        var intSum = session.Query.All<Area>().Sum(area => (int)area[testData.IntFieldName]);
        Assert.That(intSum, Is.EqualTo(testData.IntFieldValue));

        var longSum = session.Query.All<Area>().Sum(area => (long)area[testData.LongFieldName]);
        Assert.That(longSum, Is.EqualTo(testData.LongFieldValue));
        
        var floatSum = session.Query.All<Area>().Sum(area => (float)area[testData.FloatFieldName]);
        Assert.That(floatSum, Is.EqualTo(testData.FloatFieldValue));
        
        var doubleSum = session.Query.All<Area>().Sum(area => (double)area[testData.DoubleFieldName]);
        Assert.That(doubleSum, Is.EqualTo(testData.DoubleFieldValue));
        
        var decimalSum = session.Query.All<Area>().Sum(area => (decimal)area[testData.DecimalFieldName]);
        Assert.That(decimalSum, Is.EqualTo(testData.DecimalFieldValue));
      }
    }
    
    [Test]
    public void AverageTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transction = session.OpenTransaction()) {
        var byteAvg = session.Query.All<Area>().Average(area => (byte)area[testData.ByteFieldName]);
        Assert.That(byteAvg, Is.EqualTo(testData.ByteFieldValue));

        var intAvg = session.Query.All<Area>().Average(area => (int)area[testData.IntFieldName]);
        Assert.That(intAvg, Is.EqualTo(testData.IntFieldValue));

        var longAvg = session.Query.All<Area>().Average(area => (long)area[testData.LongFieldName]);
        Assert.That(longAvg, Is.EqualTo(testData.LongFieldValue));

        var floatAvg = session.Query.All<Area>().Average(area => (float)area[testData.FloatFieldName]);
        Assert.That(floatAvg, Is.EqualTo(testData.FloatFieldValue));

        var doubleAvg = session.Query.All<Area>().Average(area => (double)area[testData.DoubleFieldName]);
        Assert.That(doubleAvg, Is.EqualTo(testData.DoubleFieldValue));

        var decimalAvg = session.Query.All<Area>().Average(area => (decimal)area[testData.DecimalFieldName]);
        Assert.That(decimalAvg, Is.EqualTo(testData.DecimalFieldValue));
      }
    }

    [Test]
    public void GroupJoinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transction = session.OpenTransaction()) {
        Assert.DoesNotThrow(()=> {
          session.Query.All<Area>()
            .GroupJoin(session.Query.All<Group>(),
              area => (Group)area[testData.GroupFieldName],
              group => group,
              (area, groups) => new {
                Area = area,
                Group = groups.Single()
              });
        });
      }
    }

    [Test]
    public void JoinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          session.Query.All<Area>().Join(session.Query.All<Group>(), area => (Group)area[testData.GroupFieldName],
            group => group, (area, @group) => new {Area = area, Group = group});
        });
      }
    }

#if NET10_0_OR_GREATER
    [Test]
    public void LeftJoinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          session.Query.All<Area>().LeftJoin(session.Query.All<Group>(), area => (Group) area[testData.GroupFieldName],
            group => group, (area, @group) => new { Area = area, Group = group });
        });
      }
    }
#else
    [Test]
    public void LeftJoinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          session.Query.All<Area>().LeftJoinEx(session.Query.All<Group>(), area => (Group)area[testData.GroupFieldName],
            group => group, (area, @group) => new { Area = area, Group = group });
        });
      }
    }
#endif

    private void TestFields(Area area, Group group, SomeClass someClass, ITestInterface implementor)
    {
      Assert.That((Group)area[testData.GroupFieldName], Is.EqualTo(group));
      Assert.That(((GeoLocation)area[testData.GeoLocationFieldName]).Latitude, Is.EqualTo(testData.GeoLocation.Latitude));
      Assert.That(((GeoLocation)area[testData.GeoLocationFieldName]).Longitude, Is.EqualTo(testData.GeoLocation.Longitude));
      Assert.That((int)((GeoLocation)area[testData.GeoLocationFieldName])[testData.GeoLocationDynamicFieldName], Is.EqualTo(testData.GeoLocationDynamicFieldValue));
      Assert.That(((EntitySet<SomeClass>)area[testData.SomeClassesFieldName]).Count, Is.EqualTo(1));
      Assert.That(((EntitySet<SomeClass>)area[testData.SomeClassesFieldName]).FirstOrDefault(), Is.EqualTo(someClass));
      Assert.That(area[testData.IntFieldName], Is.EqualTo(testData.IntFieldValue));
      Assert.That(area[testData.StringFieldName], Is.EqualTo(testData.StringFieldValue));
      Assert.That((double)area[testData.DoubleFieldName], Is.EqualTo(testData.DoubleFieldValue));
      Assert.That((DateTime)area[testData.DateTimeFieldName], Is.EqualTo(testData.DateTimeFieldValue));
      Assert.That((byte[])area[testData.ByteArrayFieldName], Is.EqualTo(testData.ByteArrayFieldValue));
      Assert.That((byte)area[testData.ByteFieldName], Is.EqualTo(testData.ByteFieldValue));
      Assert.That((decimal)area[testData.DecimalFieldName], Is.EqualTo(testData.DecimalFieldValue));
      Assert.That((long)area[testData.LongFieldName], Is.EqualTo(testData.LongFieldValue));
      Assert.That((float)area[testData.FloatFieldName], Is.EqualTo(testData.FloatFieldValue));
      Assert.That((bool)area[testData.BooleanFieldName], Is.EqualTo(testData.BooleanFieldValue));
      Assert.That((ITestInterface)area[testData.InterfaceImplementorFieldName], Is.EqualTo((ITestInterface)implementor));
    }

    private bool IsOracle()
    {
      var info = StorageProviderInfo.Instance;
      return info.CheckProviderIs(StorageProvider.Oracle);
    }

    public override void TestFixtureSetUp()
    {
      var testData = TestData.CorrectData;

      testData.AreaName = "Name";
      testData.BooleanFieldName = "BoleanField";
      testData.BooleanFieldValue = false;
      testData.ByteArrayFieldName = "ByteArrayField";
      testData.ByteArrayFieldValue = new byte[] { 12, 12, 12, 12, 12, 12, 12 };
      testData.ByteFieldName = "ByteField";
      testData.ByteFieldValue = 10;
      testData.DateTimeFieldName = "DateTimeField";
      testData.DateTimeFieldValue = new DateTime(2012, 12, 12, 12, 12, 12);
      testData.DecimalFieldName = "DecimalField";
      testData.DecimalFieldValue = new decimal(10);
      testData.DoubleFieldName = "DoubleField";
      testData.DoubleFieldValue = 10;
      testData.FloatFieldName = "FloatField";
      testData.FloatFieldValue = 10;
      testData.GeoLocationFieldName = "LocationField";
      testData.GeoLocationDynamicFieldName = "Dynamic";
      testData.GeoLocationDynamicFieldValue = 1555;
      testData.GroupFieldName = "Group";
      testData.GroupValue = "Group";
      testData.IntFieldName = "IntField";
      testData.IntFieldValue = 10;
      testData.LongFieldName = "LongField";
      testData.LongFieldValue = 10;
      testData.InterfaceImplementorFieldName = "ImplementorField";
      testData.SomeClassesFieldName = "SomeClasses";
      testData.StringFieldName = "StringField";
      testData.StringFieldValue = "String";

      base.TestFixtureSetUp();
    }

    protected override void PopulateData()
    {
      var testData = TestData.CorrectData;
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var area = new Area {Name = testData.AreaName};
        var group = new Group {Value = testData.GroupValue};
        testData.Group = group.Key;
        var someClass = new SomeClass();
        decimal fDecimal = new decimal((double)10);
        someClass["DynamicField"] = (int)12;
        testData.SomeClassKey = someClass.Key;
        var geolocation = new GeoLocation {Latitude = 10, Longitude = 100};
        geolocation[testData.GeoLocationDynamicFieldName] = (int)testData.GeoLocationDynamicFieldValue;
        testData.GeoLocation = geolocation;
        var interfaceImplementor = new TestInterfaceImplementor {InterfaseImplementorName = typeof (TestInterfaceImplementor).AssemblyQualifiedName};
        testData.InterfaceImplementorKey = interfaceImplementor.Key;

        area[testData.GroupFieldName] = group;
        area[testData.GeoLocationFieldName] = testData.GeoLocation;
        area[testData.IntFieldName] = testData.IntFieldValue;
        area[testData.StringFieldName] = testData.StringFieldValue;
        area[testData.DoubleFieldName] = testData.DoubleFieldValue;
        area[testData.DateTimeFieldName] = testData.DateTimeFieldValue;
        area[testData.ByteArrayFieldName] = testData.ByteArrayFieldValue;
        area[testData.ByteFieldName] = testData.ByteFieldValue;
        area[testData.DecimalFieldName] = testData.DecimalFieldValue;
        area[testData.LongFieldName] = testData.LongFieldValue;
        area[testData.FloatFieldName] = testData.FloatFieldValue;
        area[testData.BooleanFieldName] = testData.BooleanFieldValue;
        area[testData.InterfaceImplementorFieldName] = interfaceImplementor;

        ((EntitySet<SomeClass>)area[testData.SomeClassesFieldName]).Add(someClass);
        area.Somes.Add(someClass);
        someClass.Area = area;
        new Removable {Area = area, Decimal = testData.DecimalFieldValue};
        transaction.Complete();
      }
    }

    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Area).Assembly, typeof (Area).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
