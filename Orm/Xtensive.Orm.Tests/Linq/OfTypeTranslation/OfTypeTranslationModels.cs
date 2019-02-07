// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.02.07

using Xtensive.Orm.Model;
using System.Linq.Expressions;
using System;

namespace Xtensive.Orm.Tests.Linq.OfTypeTranslation
{
  namespace Models
  {
      public interface IBaseEntityClassTable : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    long Field1 { get; set; }
  }

  public interface ITestEntityBaseClassTable : IEntity
  {
    [Field]
    ulong BaseField { get; set; }
  }

  public interface ITestEntity2ClassTable : ITestEntityBaseClassTable
  {
    [Field]
    TestStructure1ClassTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public interface ITestEntity3aClassTable : ITestEntityBaseClassTable
  {
    [Field]
    double Field5 { get; set; }

    [Field]
    EntitySet<TestEntity2aClassTable> Field6 { get; set; }
  }

  public interface ITestEntity2cClassTable : ITestEntityBaseClassTable
  {
    [Field]
    TestStructure1ClassTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public class BaseEntityClassTable : Entity, IBaseEntityClassTable
  {
    public int Id { get; private set; }

    public long Field1 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class TestEntity1ClassTable : BaseEntityClassTable
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class TestEntity2aClassTable : TestEntity1ClassTable, ITestEntity2ClassTable
  {
    public TestStructure1ClassTable Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class TestEntity3aClassTable : TestEntity2aClassTable, ITestEntity3aClassTable
  {
    public double Field5 { get; set; }

    public EntitySet<TestEntity2aClassTable> Field6 { get; set; }
  }

  public class TestEntity2bClassTable : TestEntity1ClassTable, ITestEntity2ClassTable
  {
    [Field]
    public EntitySet<TestEntity2bClassTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public TestStructure1ClassTable Field3 { get; set; }
  }

  public class TestEntity2cClassTable : TestEntity1ClassTable, ITestEntity2cClassTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1ClassTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class TestEntityHierarchy2ClassTable : BaseEntityClassTable, ITestEntity2ClassTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1ClassTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  public class TestStructure1ClassTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public TestStructure2ClassTable Value2 { get; set; }

    [Field]
    public string Value3 { get; set; }
  }

  public class TestStructure2ClassTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public DateTime Value2 { get; set; }
  }


  public static class ClassTableData
  {
    public static void PopulateData() 
    {
          new TestEntity3aClassTable()
    {
        BaseField = 1,
        Field1 = 10,
        Field2 = 1000,
        Field3 = new TestStructure1ClassTable
        {
        Value1 = 10000,
        Value2 = new TestStructure2ClassTable
        {
            Value1 = 100000,
            Value2 = DateTime.FromBinary(10)
        },
        Value3 = "StructureString1"
        },
        Field4 = "String1",
        Field5 = 2.5,
    }.Field6.Add(
        new TestEntity3aClassTable
        {
        BaseField = 2,
        Field1 = 20,
        Field2 = 2000,
        Field3 = new TestStructure1ClassTable
        {
            Value1 = 20000,
            Value2 = new TestStructure2ClassTable
            {
            Value1 = 200000,
            Value2 = DateTime.FromBinary(1000)
            },
            Value3 = "StructureString2"
        },
        Field4 = "String2",
        Field5 = 3.5,
        });
    new TestEntity2bClassTable()
    {
        BaseField = 3,
        Field1 = 30,
        Field2 = 3000,
        Field3 = new TestStructure1ClassTable
        {
        Value1 = 30000,
        Value2 = new TestStructure2ClassTable
        {
            Value1 = 300000,
            Value2 = DateTime.FromBinary(100000)
        },
        Value3 = "StructureString3"
        },
        Field4 = "String3"
    };
    new TestEntity2cClassTable()
    {
        BaseField = 4,
        Field1 = 40,
        Field2 = 4000,
        Field3 = new TestStructure1ClassTable
        {
        Value1 = 40000,
        Value2 = new TestStructure2ClassTable
        {
            Value1 = 400000,
            Value2 = DateTime.FromBinary(400000)
        },
        Value3 = "StructureString4"
        },
        Field4 = "String4"
    };
    new TestEntityHierarchy2ClassTable()
    {
        BaseField = 5,
        Field1 = 50,
        Field3 = new TestStructure1ClassTable
        {
        Value1 = 50000,
        Value2 = new TestStructure2ClassTable
        {
            Value1 = 500000,
            Value2 = DateTime.FromBinary(5000000)
        },
        Value3 = "StructureString5"
        },
        Field4 = "String5"
    };
    }
  }

  public interface IBaseEntitySingleTable : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    long Field1 { get; set; }
  }

  public interface ITestEntityBaseSingleTable : IEntity
  {
    [Field]
    ulong BaseField { get; set; }
  }

  public interface ITestEntity2SingleTable : ITestEntityBaseSingleTable
  {
    [Field]
    TestStructure1SingleTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public interface ITestEntity3aSingleTable : ITestEntityBaseSingleTable
  {
    [Field]
    double Field5 { get; set; }

    [Field]
    EntitySet<TestEntity2aSingleTable> Field6 { get; set; }
  }

  public interface ITestEntity2cSingleTable : ITestEntityBaseSingleTable
  {
    [Field]
    TestStructure1SingleTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public class BaseEntitySingleTable : Entity, IBaseEntitySingleTable
  {
    public int Id { get; private set; }

    public long Field1 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class TestEntity1SingleTable : BaseEntitySingleTable
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class TestEntity2aSingleTable : TestEntity1SingleTable, ITestEntity2SingleTable
  {
    public TestStructure1SingleTable Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class TestEntity3aSingleTable : TestEntity2aSingleTable, ITestEntity3aSingleTable
  {
    public double Field5 { get; set; }

    public EntitySet<TestEntity2aSingleTable> Field6 { get; set; }
  }

  public class TestEntity2bSingleTable : TestEntity1SingleTable, ITestEntity2SingleTable
  {
    [Field]
    public EntitySet<TestEntity2bSingleTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public TestStructure1SingleTable Field3 { get; set; }
  }

  public class TestEntity2cSingleTable : TestEntity1SingleTable, ITestEntity2cSingleTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1SingleTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class TestEntityHierarchy2SingleTable : BaseEntitySingleTable, ITestEntity2SingleTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1SingleTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  public class TestStructure1SingleTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public TestStructure2SingleTable Value2 { get; set; }

    [Field]
    public string Value3 { get; set; }
  }

  public class TestStructure2SingleTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public DateTime Value2 { get; set; }
  }


  public static class SingleTableData
  {
    public static void PopulateData() 
    {
          new TestEntity3aSingleTable()
    {
        BaseField = 1,
        Field1 = 10,
        Field2 = 1000,
        Field3 = new TestStructure1SingleTable
        {
        Value1 = 10000,
        Value2 = new TestStructure2SingleTable
        {
            Value1 = 100000,
            Value2 = DateTime.FromBinary(10)
        },
        Value3 = "StructureString1"
        },
        Field4 = "String1",
        Field5 = 2.5,
    }.Field6.Add(
        new TestEntity3aSingleTable
        {
        BaseField = 2,
        Field1 = 20,
        Field2 = 2000,
        Field3 = new TestStructure1SingleTable
        {
            Value1 = 20000,
            Value2 = new TestStructure2SingleTable
            {
            Value1 = 200000,
            Value2 = DateTime.FromBinary(1000)
            },
            Value3 = "StructureString2"
        },
        Field4 = "String2",
        Field5 = 3.5,
        });
    new TestEntity2bSingleTable()
    {
        BaseField = 3,
        Field1 = 30,
        Field2 = 3000,
        Field3 = new TestStructure1SingleTable
        {
        Value1 = 30000,
        Value2 = new TestStructure2SingleTable
        {
            Value1 = 300000,
            Value2 = DateTime.FromBinary(100000)
        },
        Value3 = "StructureString3"
        },
        Field4 = "String3"
    };
    new TestEntity2cSingleTable()
    {
        BaseField = 4,
        Field1 = 40,
        Field2 = 4000,
        Field3 = new TestStructure1SingleTable
        {
        Value1 = 40000,
        Value2 = new TestStructure2SingleTable
        {
            Value1 = 400000,
            Value2 = DateTime.FromBinary(400000)
        },
        Value3 = "StructureString4"
        },
        Field4 = "String4"
    };
    new TestEntityHierarchy2SingleTable()
    {
        BaseField = 5,
        Field1 = 50,
        Field3 = new TestStructure1SingleTable
        {
        Value1 = 50000,
        Value2 = new TestStructure2SingleTable
        {
            Value1 = 500000,
            Value2 = DateTime.FromBinary(5000000)
        },
        Value3 = "StructureString5"
        },
        Field4 = "String5"
    };
    }
  }

  public interface IBaseEntityConcreteTable : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    long Field1 { get; set; }
  }

  public interface ITestEntityBaseConcreteTable : IEntity
  {
    [Field]
    ulong BaseField { get; set; }
  }

  public interface ITestEntity2ConcreteTable : ITestEntityBaseConcreteTable
  {
    [Field]
    TestStructure1ConcreteTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public interface ITestEntity3aConcreteTable : ITestEntityBaseConcreteTable
  {
    [Field]
    double Field5 { get; set; }

    [Field]
    EntitySet<TestEntity2aConcreteTable> Field6 { get; set; }
  }

  public interface ITestEntity2cConcreteTable : ITestEntityBaseConcreteTable
  {
    [Field]
    TestStructure1ConcreteTable Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public class BaseEntityConcreteTable : Entity, IBaseEntityConcreteTable
  {
    public int Id { get; private set; }

    public long Field1 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class TestEntity1ConcreteTable : BaseEntityConcreteTable
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class TestEntity2aConcreteTable : TestEntity1ConcreteTable, ITestEntity2ConcreteTable
  {
    public TestStructure1ConcreteTable Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class TestEntity3aConcreteTable : TestEntity2aConcreteTable, ITestEntity3aConcreteTable
  {
    public double Field5 { get; set; }

    public EntitySet<TestEntity2aConcreteTable> Field6 { get; set; }
  }

  public class TestEntity2bConcreteTable : TestEntity1ConcreteTable, ITestEntity2ConcreteTable
  {
    [Field]
    public EntitySet<TestEntity2bConcreteTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public TestStructure1ConcreteTable Field3 { get; set; }
  }

  public class TestEntity2cConcreteTable : TestEntity1ConcreteTable, ITestEntity2cConcreteTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1ConcreteTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class TestEntityHierarchy2ConcreteTable : BaseEntityConcreteTable, ITestEntity2ConcreteTable
  {
    public ulong BaseField { get; set; }

    public TestStructure1ConcreteTable Field3 { get; set; }

    public string Field4 { get; set; }
  }

  public class TestStructure1ConcreteTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public TestStructure2ConcreteTable Value2 { get; set; }

    [Field]
    public string Value3 { get; set; }
  }

  public class TestStructure2ConcreteTable : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public DateTime Value2 { get; set; }
  }


  public static class ConcreteTableData
  {
    public static void PopulateData() 
    {
          new TestEntity3aConcreteTable()
    {
        BaseField = 1,
        Field1 = 10,
        Field2 = 1000,
        Field3 = new TestStructure1ConcreteTable
        {
        Value1 = 10000,
        Value2 = new TestStructure2ConcreteTable
        {
            Value1 = 100000,
            Value2 = DateTime.FromBinary(10)
        },
        Value3 = "StructureString1"
        },
        Field4 = "String1",
        Field5 = 2.5,
    }.Field6.Add(
        new TestEntity3aConcreteTable
        {
        BaseField = 2,
        Field1 = 20,
        Field2 = 2000,
        Field3 = new TestStructure1ConcreteTable
        {
            Value1 = 20000,
            Value2 = new TestStructure2ConcreteTable
            {
            Value1 = 200000,
            Value2 = DateTime.FromBinary(1000)
            },
            Value3 = "StructureString2"
        },
        Field4 = "String2",
        Field5 = 3.5,
        });
    new TestEntity2bConcreteTable()
    {
        BaseField = 3,
        Field1 = 30,
        Field2 = 3000,
        Field3 = new TestStructure1ConcreteTable
        {
        Value1 = 30000,
        Value2 = new TestStructure2ConcreteTable
        {
            Value1 = 300000,
            Value2 = DateTime.FromBinary(100000)
        },
        Value3 = "StructureString3"
        },
        Field4 = "String3"
    };
    new TestEntity2cConcreteTable()
    {
        BaseField = 4,
        Field1 = 40,
        Field2 = 4000,
        Field3 = new TestStructure1ConcreteTable
        {
        Value1 = 40000,
        Value2 = new TestStructure2ConcreteTable
        {
            Value1 = 400000,
            Value2 = DateTime.FromBinary(400000)
        },
        Value3 = "StructureString4"
        },
        Field4 = "String4"
    };
    new TestEntityHierarchy2ConcreteTable()
    {
        BaseField = 5,
        Field1 = 50,
        Field3 = new TestStructure1ConcreteTable
        {
        Value1 = 50000,
        Value2 = new TestStructure2ConcreteTable
        {
            Value1 = 500000,
            Value2 = DateTime.FromBinary(5000000)
        },
        Value3 = "StructureString5"
        },
        Field4 = "String5"
    };
    }
  }

  }
}