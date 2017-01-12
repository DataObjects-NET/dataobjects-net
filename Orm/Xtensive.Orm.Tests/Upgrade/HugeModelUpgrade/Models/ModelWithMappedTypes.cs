
using System;
using System.Linq;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.ModelWithMappings
{
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity0")]
  public class TestEntity0 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity1")]
  public class TestEntity1 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity2")]
  public class TestEntity2 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity3")]
  public class TestEntity3 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity4")]
  public class TestEntity4 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity5")]
  public class TestEntity5 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity6")]
  public class TestEntity6 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity7")]
  public class TestEntity7 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity8")]
  public class TestEntity8 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity9")]
  public class TestEntity9 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity10")]
  public class TestEntity10 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}


  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity11")]
  public class TestEntity11 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

    [Field]
    public TestEntity0 TestEntity0{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity12")]
  public class TestEntity12 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

    [Field]
    public TestEntity1 TestEntity1{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity13")]
  public class TestEntity13 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

    [Field]
    public TestEntity2 TestEntity2{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity14")]
  public class TestEntity14 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

    [Field]
    public TestEntity3 TestEntity3{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity15")]
  public class TestEntity15 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

    [Field]
    public TestEntity4 TestEntity4{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity16")]
  public class TestEntity16 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

    [Field]
    public TestEntity5 TestEntity5{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity17")]
  public class TestEntity17 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

    [Field]
    public TestEntity6 TestEntity6{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity18")]
  public class TestEntity18 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

    [Field]
    public TestEntity7 TestEntity7{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity19")]
  public class TestEntity19 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

    [Field]
    public TestEntity8 TestEntity8{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity20")]
  public class TestEntity20 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

    [Field]
    public TestEntity9 TestEntity9{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity21")]
  public class TestEntity21 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

    [Field]
    public TestEntity10 TestEntity10{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity22")]
  public class TestEntity22 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

    [Field]
    public TestEntity11 TestEntity11{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity23")]
  public class TestEntity23 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

    [Field]
    public TestEntity12 TestEntity12{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity24")]
  public class TestEntity24 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

    [Field]
    public TestEntity13 TestEntity13{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity25")]
  public class TestEntity25 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

    [Field]
    public TestEntity14 TestEntity14{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity26")]
  public class TestEntity26 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

    [Field]
    public TestEntity15 TestEntity15{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity27")]
  public class TestEntity27 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

    [Field]
    public TestEntity16 TestEntity16{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity28")]
  public class TestEntity28 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

    [Field]
    public TestEntity17 TestEntity17{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity29")]
  public class TestEntity29 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

    [Field]
    public TestEntity18 TestEntity18{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity30")]
  public class TestEntity30 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

    [Field]
    public TestEntity19 TestEntity19{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity31")]
  public class TestEntity31 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

    [Field]
    public TestEntity20 TestEntity20{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity32")]
  public class TestEntity32 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

    [Field]
    public TestEntity21 TestEntity21{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity33")]
  public class TestEntity33 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

    [Field]
    public TestEntity22 TestEntity22{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity34")]
  public class TestEntity34 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

    [Field]
    public TestEntity23 TestEntity23{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity35")]
  public class TestEntity35 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

    [Field]
    public TestEntity24 TestEntity24{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity36")]
  public class TestEntity36 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

    [Field]
    public TestEntity25 TestEntity25{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity37")]
  public class TestEntity37 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

    [Field]
    public TestEntity26 TestEntity26{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity38")]
  public class TestEntity38 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

    [Field]
    public TestEntity27 TestEntity27{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity39")]
  public class TestEntity39 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

    [Field]
    public TestEntity28 TestEntity28{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity40")]
  public class TestEntity40 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

    [Field]
    public TestEntity29 TestEntity29{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity41")]
  public class TestEntity41 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

    [Field]
    public TestEntity30 TestEntity30{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity42")]
  public class TestEntity42 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

    [Field]
    public TestEntity31 TestEntity31{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity43")]
  public class TestEntity43 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

    [Field]
    public TestEntity32 TestEntity32{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity44")]
  public class TestEntity44 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

    [Field]
    public TestEntity33 TestEntity33{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity45")]
  public class TestEntity45 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

    [Field]
    public TestEntity34 TestEntity34{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity46")]
  public class TestEntity46 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

    [Field]
    public TestEntity35 TestEntity35{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity47")]
  public class TestEntity47 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

    [Field]
    public TestEntity36 TestEntity36{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity48")]
  public class TestEntity48 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

    [Field]
    public TestEntity37 TestEntity37{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity49")]
  public class TestEntity49 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

    [Field]
    public TestEntity38 TestEntity38{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity50")]
  public class TestEntity50 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

    [Field]
    public TestEntity39 TestEntity39{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity51")]
  public class TestEntity51 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

    [Field]
    public TestEntity40 TestEntity40{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity52")]
  public class TestEntity52 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

    [Field]
    public TestEntity41 TestEntity41{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity53")]
  public class TestEntity53 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

    [Field]
    public TestEntity42 TestEntity42{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity54")]
  public class TestEntity54 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

    [Field]
    public TestEntity43 TestEntity43{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity55")]
  public class TestEntity55 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

    [Field]
    public TestEntity44 TestEntity44{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity56")]
  public class TestEntity56 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

    [Field]
    public TestEntity45 TestEntity45{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity57")]
  public class TestEntity57 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

    [Field]
    public TestEntity46 TestEntity46{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity58")]
  public class TestEntity58 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

    [Field]
    public TestEntity47 TestEntity47{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity59")]
  public class TestEntity59 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

    [Field]
    public TestEntity48 TestEntity48{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity60")]
  public class TestEntity60 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

    [Field]
    public TestEntity49 TestEntity49{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity61")]
  public class TestEntity61 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

    [Field]
    public TestEntity50 TestEntity50{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity62")]
  public class TestEntity62 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

    [Field]
    public TestEntity51 TestEntity51{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity63")]
  public class TestEntity63 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

    [Field]
    public TestEntity52 TestEntity52{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity64")]
  public class TestEntity64 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

    [Field]
    public TestEntity53 TestEntity53{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity65")]
  public class TestEntity65 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

    [Field]
    public TestEntity54 TestEntity54{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity66")]
  public class TestEntity66 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

    [Field]
    public TestEntity55 TestEntity55{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity67")]
  public class TestEntity67 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

    [Field]
    public TestEntity56 TestEntity56{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity68")]
  public class TestEntity68 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

    [Field]
    public TestEntity57 TestEntity57{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity69")]
  public class TestEntity69 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

    [Field]
    public TestEntity58 TestEntity58{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity70")]
  public class TestEntity70 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

    [Field]
    public TestEntity59 TestEntity59{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity71")]
  public class TestEntity71 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

    [Field]
    public TestEntity60 TestEntity60{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity72")]
  public class TestEntity72 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

    [Field]
    public TestEntity61 TestEntity61{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity73")]
  public class TestEntity73 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

    [Field]
    public TestEntity62 TestEntity62{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity74")]
  public class TestEntity74 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

    [Field]
    public TestEntity63 TestEntity63{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity75")]
  public class TestEntity75 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

    [Field]
    public TestEntity64 TestEntity64{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity76")]
  public class TestEntity76 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

    [Field]
    public TestEntity65 TestEntity65{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity77")]
  public class TestEntity77 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

    [Field]
    public TestEntity66 TestEntity66{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity78")]
  public class TestEntity78 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

    [Field]
    public TestEntity67 TestEntity67{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity79")]
  public class TestEntity79 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

    [Field]
    public TestEntity68 TestEntity68{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity80")]
  public class TestEntity80 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

    [Field]
    public TestEntity69 TestEntity69{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity81")]
  public class TestEntity81 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

    [Field]
    public TestEntity70 TestEntity70{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity82")]
  public class TestEntity82 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

    [Field]
    public TestEntity71 TestEntity71{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity83")]
  public class TestEntity83 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

    [Field]
    public TestEntity72 TestEntity72{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity84")]
  public class TestEntity84 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

    [Field]
    public TestEntity73 TestEntity73{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity85")]
  public class TestEntity85 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

    [Field]
    public TestEntity74 TestEntity74{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity86")]
  public class TestEntity86 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

    [Field]
    public TestEntity75 TestEntity75{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity87")]
  public class TestEntity87 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

    [Field]
    public TestEntity76 TestEntity76{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity88")]
  public class TestEntity88 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

    [Field]
    public TestEntity77 TestEntity77{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity89")]
  public class TestEntity89 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

    [Field]
    public TestEntity78 TestEntity78{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity90")]
  public class TestEntity90 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

    [Field]
    public TestEntity79 TestEntity79{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity91")]
  public class TestEntity91 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

    [Field]
    public TestEntity80 TestEntity80{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity92")]
  public class TestEntity92 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

    [Field]
    public TestEntity81 TestEntity81{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity93")]
  public class TestEntity93 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

    [Field]
    public TestEntity82 TestEntity82{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity94")]
  public class TestEntity94 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

    [Field]
    public TestEntity83 TestEntity83{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity95")]
  public class TestEntity95 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

    [Field]
    public TestEntity84 TestEntity84{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity96")]
  public class TestEntity96 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

    [Field]
    public TestEntity85 TestEntity85{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity97")]
  public class TestEntity97 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

    [Field]
    public TestEntity86 TestEntity86{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity98")]
  public class TestEntity98 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

    [Field]
    public TestEntity87 TestEntity87{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity99")]
  public class TestEntity99 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

    [Field]
    public TestEntity88 TestEntity88{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity100")]
  public class TestEntity100 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

    [Field]
    public TestEntity89 TestEntity89{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity101")]
  public class TestEntity101 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

    [Field]
    public TestEntity90 TestEntity90{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity102")]
  public class TestEntity102 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

    [Field]
    public TestEntity91 TestEntity91{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity103")]
  public class TestEntity103 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

    [Field]
    public TestEntity92 TestEntity92{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity104")]
  public class TestEntity104 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

    [Field]
    public TestEntity93 TestEntity93{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity105")]
  public class TestEntity105 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

    [Field]
    public TestEntity94 TestEntity94{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity106")]
  public class TestEntity106 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

    [Field]
    public TestEntity95 TestEntity95{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity107")]
  public class TestEntity107 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

    [Field]
    public TestEntity96 TestEntity96{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity108")]
  public class TestEntity108 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

    [Field]
    public TestEntity97 TestEntity97{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity109")]
  public class TestEntity109 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

    [Field]
    public TestEntity98 TestEntity98{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity110")]
  public class TestEntity110 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

    [Field]
    public TestEntity99 TestEntity99{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity111")]
  public class TestEntity111 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

    [Field]
    public TestEntity100 TestEntity100{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity112")]
  public class TestEntity112 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

    [Field]
    public TestEntity101 TestEntity101{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity113")]
  public class TestEntity113 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

    [Field]
    public TestEntity102 TestEntity102{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity114")]
  public class TestEntity114 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

    [Field]
    public TestEntity103 TestEntity103{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity115")]
  public class TestEntity115 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

    [Field]
    public TestEntity104 TestEntity104{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity116")]
  public class TestEntity116 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

    [Field]
    public TestEntity105 TestEntity105{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity117")]
  public class TestEntity117 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

    [Field]
    public TestEntity106 TestEntity106{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity118")]
  public class TestEntity118 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

    [Field]
    public TestEntity107 TestEntity107{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity119")]
  public class TestEntity119 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

    [Field]
    public TestEntity108 TestEntity108{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity120")]
  public class TestEntity120 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

    [Field]
    public TestEntity109 TestEntity109{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity121")]
  public class TestEntity121 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

    [Field]
    public TestEntity110 TestEntity110{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity122")]
  public class TestEntity122 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

    [Field]
    public TestEntity111 TestEntity111{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity123")]
  public class TestEntity123 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

    [Field]
    public TestEntity112 TestEntity112{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity124")]
  public class TestEntity124 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

    [Field]
    public TestEntity113 TestEntity113{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity125")]
  public class TestEntity125 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

    [Field]
    public TestEntity114 TestEntity114{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity126")]
  public class TestEntity126 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

    [Field]
    public TestEntity115 TestEntity115{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity127")]
  public class TestEntity127 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

    [Field]
    public TestEntity116 TestEntity116{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity128")]
  public class TestEntity128 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

    [Field]
    public TestEntity117 TestEntity117{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity129")]
  public class TestEntity129 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

    [Field]
    public TestEntity118 TestEntity118{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity130")]
  public class TestEntity130 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

    [Field]
    public TestEntity119 TestEntity119{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity131")]
  public class TestEntity131 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

    [Field]
    public TestEntity120 TestEntity120{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity132")]
  public class TestEntity132 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

    [Field]
    public TestEntity121 TestEntity121{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity133")]
  public class TestEntity133 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

    [Field]
    public TestEntity122 TestEntity122{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity134")]
  public class TestEntity134 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

    [Field]
    public TestEntity123 TestEntity123{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity135")]
  public class TestEntity135 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

    [Field]
    public TestEntity124 TestEntity124{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity136")]
  public class TestEntity136 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

    [Field]
    public TestEntity125 TestEntity125{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity137")]
  public class TestEntity137 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

    [Field]
    public TestEntity126 TestEntity126{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity138")]
  public class TestEntity138 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

    [Field]
    public TestEntity127 TestEntity127{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity139")]
  public class TestEntity139 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

    [Field]
    public TestEntity128 TestEntity128{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity140")]
  public class TestEntity140 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

    [Field]
    public TestEntity129 TestEntity129{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity141")]
  public class TestEntity141 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

    [Field]
    public TestEntity130 TestEntity130{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity142")]
  public class TestEntity142 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

    [Field]
    public TestEntity131 TestEntity131{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity143")]
  public class TestEntity143 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

    [Field]
    public TestEntity132 TestEntity132{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity144")]
  public class TestEntity144 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

    [Field]
    public TestEntity133 TestEntity133{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity145")]
  public class TestEntity145 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

    [Field]
    public TestEntity134 TestEntity134{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity146")]
  public class TestEntity146 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

    [Field]
    public TestEntity135 TestEntity135{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity147")]
  public class TestEntity147 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

    [Field]
    public TestEntity136 TestEntity136{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity148")]
  public class TestEntity148 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

    [Field]
    public TestEntity137 TestEntity137{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity149")]
  public class TestEntity149 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

    [Field]
    public TestEntity138 TestEntity138{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity150")]
  public class TestEntity150 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

    [Field]
    public TestEntity139 TestEntity139{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity151")]
  public class TestEntity151 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

    [Field]
    public TestEntity140 TestEntity140{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity152")]
  public class TestEntity152 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

    [Field]
    public TestEntity141 TestEntity141{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity153")]
  public class TestEntity153 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

    [Field]
    public TestEntity142 TestEntity142{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity154")]
  public class TestEntity154 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

    [Field]
    public TestEntity143 TestEntity143{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity155")]
  public class TestEntity155 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

    [Field]
    public TestEntity144 TestEntity144{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity156")]
  public class TestEntity156 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

    [Field]
    public TestEntity145 TestEntity145{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity157")]
  public class TestEntity157 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

    [Field]
    public TestEntity146 TestEntity146{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity158")]
  public class TestEntity158 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

    [Field]
    public TestEntity147 TestEntity147{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity159")]
  public class TestEntity159 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

    [Field]
    public TestEntity148 TestEntity148{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity160")]
  public class TestEntity160 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

    [Field]
    public TestEntity149 TestEntity149{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity161")]
  public class TestEntity161 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

    [Field]
    public TestEntity150 TestEntity150{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity162")]
  public class TestEntity162 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

    [Field]
    public TestEntity151 TestEntity151{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity163")]
  public class TestEntity163 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

    [Field]
    public TestEntity152 TestEntity152{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity164")]
  public class TestEntity164 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

    [Field]
    public TestEntity153 TestEntity153{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity165")]
  public class TestEntity165 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

    [Field]
    public TestEntity154 TestEntity154{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity166")]
  public class TestEntity166 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

    [Field]
    public TestEntity155 TestEntity155{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity167")]
  public class TestEntity167 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

    [Field]
    public TestEntity156 TestEntity156{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity168")]
  public class TestEntity168 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

    [Field]
    public TestEntity157 TestEntity157{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity169")]
  public class TestEntity169 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

    [Field]
    public TestEntity158 TestEntity158{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity170")]
  public class TestEntity170 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

    [Field]
    public TestEntity159 TestEntity159{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity171")]
  public class TestEntity171 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

    [Field]
    public TestEntity160 TestEntity160{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity172")]
  public class TestEntity172 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

    [Field]
    public TestEntity161 TestEntity161{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity173")]
  public class TestEntity173 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

    [Field]
    public TestEntity162 TestEntity162{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity174")]
  public class TestEntity174 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

    [Field]
    public TestEntity163 TestEntity163{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity175")]
  public class TestEntity175 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

    [Field]
    public TestEntity164 TestEntity164{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity176")]
  public class TestEntity176 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

    [Field]
    public TestEntity165 TestEntity165{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity177")]
  public class TestEntity177 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

    [Field]
    public TestEntity166 TestEntity166{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity178")]
  public class TestEntity178 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

    [Field]
    public TestEntity167 TestEntity167{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity179")]
  public class TestEntity179 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

    [Field]
    public TestEntity168 TestEntity168{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity180")]
  public class TestEntity180 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

    [Field]
    public TestEntity169 TestEntity169{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity181")]
  public class TestEntity181 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

    [Field]
    public TestEntity170 TestEntity170{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity182")]
  public class TestEntity182 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

    [Field]
    public TestEntity171 TestEntity171{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity183")]
  public class TestEntity183 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

    [Field]
    public TestEntity172 TestEntity172{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity184")]
  public class TestEntity184 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

    [Field]
    public TestEntity173 TestEntity173{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity185")]
  public class TestEntity185 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

    [Field]
    public TestEntity174 TestEntity174{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity186")]
  public class TestEntity186 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

    [Field]
    public TestEntity175 TestEntity175{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity187")]
  public class TestEntity187 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

    [Field]
    public TestEntity176 TestEntity176{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity188")]
  public class TestEntity188 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

    [Field]
    public TestEntity177 TestEntity177{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity189")]
  public class TestEntity189 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

    [Field]
    public TestEntity178 TestEntity178{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity190")]
  public class TestEntity190 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

    [Field]
    public TestEntity179 TestEntity179{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity191")]
  public class TestEntity191 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

    [Field]
    public TestEntity180 TestEntity180{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity192")]
  public class TestEntity192 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

    [Field]
    public TestEntity181 TestEntity181{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity193")]
  public class TestEntity193 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

    [Field]
    public TestEntity182 TestEntity182{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity194")]
  public class TestEntity194 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

    [Field]
    public TestEntity183 TestEntity183{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity195")]
  public class TestEntity195 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity194 TestEntity194{get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

    [Field]
    public TestEntity184 TestEntity184{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity196")]
  public class TestEntity196 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity195 TestEntity195{get;set;}

    [Field]
    public TestEntity194 TestEntity194{get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

    [Field]
    public TestEntity185 TestEntity185{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity197")]
  public class TestEntity197 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity196 TestEntity196{get;set;}

    [Field]
    public TestEntity195 TestEntity195{get;set;}

    [Field]
    public TestEntity194 TestEntity194{get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

    [Field]
    public TestEntity186 TestEntity186{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity198")]
  public class TestEntity198 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity197 TestEntity197{get;set;}

    [Field]
    public TestEntity196 TestEntity196{get;set;}

    [Field]
    public TestEntity195 TestEntity195{get;set;}

    [Field]
    public TestEntity194 TestEntity194{get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

    [Field]
    public TestEntity187 TestEntity187{get;set;}

  }
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  [TableMapping("MappedEntity199")]
  public class TestEntity199 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    [FieldMapping("MappedBooleanField")]
    public bool BooleanField {get;set;}

    [Field]
    [FieldMapping("MappedInt16Field")]
    public Int16 Int16Field {get;set;}

    [Field]
    [FieldMapping("MappedInt32Field")]
    public Int32 Int32Field {get;set;}

    [Field]
    [FieldMapping("MappedInt64Field")]
    public Int64 Int64Field {get;set;}

    [Field]
    [FieldMapping("MappedFloatField")]
    public float FloatField {get;set;}

    [Field]
    [FieldMapping("MappedDoubleField")]
    public double DoubleField {get;set;}

    [Field]
    [FieldMapping("MappedDateTimeField")]
    public DateTime DateTimeField {get;set;}

    [Field]
    [FieldMapping("MappedStringField")]
    public string StringField {get;set;}

    [Field]
    [FullText("English")]
    public string Text {get;set;}

    [Field]
    public TestEntity198 TestEntity198{get;set;}

    [Field]
    public TestEntity197 TestEntity197{get;set;}

    [Field]
    public TestEntity196 TestEntity196{get;set;}

    [Field]
    public TestEntity195 TestEntity195{get;set;}

    [Field]
    public TestEntity194 TestEntity194{get;set;}

    [Field]
    public TestEntity193 TestEntity193{get;set;}

    [Field]
    public TestEntity192 TestEntity192{get;set;}

    [Field]
    public TestEntity191 TestEntity191{get;set;}

    [Field]
    public TestEntity190 TestEntity190{get;set;}

    [Field]
    public TestEntity189 TestEntity189{get;set;}

    [Field]
    public TestEntity188 TestEntity188{get;set;}

  }

  public class ModelPopulator
  {
    public void Run()
    {
      new TestEntity0 {
          BooleanField = true,
          Int16Field = 0,
          Int32Field = 0,
          Int64Field = 0,
          FloatField = 0,
          DoubleField = 0,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity0",
          Text = "This is an instance of TestEntity0",
      };
      new TestEntity1 {
          BooleanField = true,
          Int16Field = 1,
          Int32Field = 1,
          Int64Field = 1,
          FloatField = 1,
          DoubleField = 1,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity1",
          Text = "This is an instance of TestEntity1",
      };
      new TestEntity2 {
          BooleanField = true,
          Int16Field = 2,
          Int32Field = 2,
          Int64Field = 2,
          FloatField = 2,
          DoubleField = 2,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity2",
          Text = "This is an instance of TestEntity2",
      };
      new TestEntity3 {
          BooleanField = true,
          Int16Field = 3,
          Int32Field = 3,
          Int64Field = 3,
          FloatField = 3,
          DoubleField = 3,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity3",
          Text = "This is an instance of TestEntity3",
      };
      new TestEntity4 {
          BooleanField = true,
          Int16Field = 4,
          Int32Field = 4,
          Int64Field = 4,
          FloatField = 4,
          DoubleField = 4,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity4",
          Text = "This is an instance of TestEntity4",
      };
      new TestEntity5 {
          BooleanField = true,
          Int16Field = 5,
          Int32Field = 5,
          Int64Field = 5,
          FloatField = 5,
          DoubleField = 5,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity5",
          Text = "This is an instance of TestEntity5",
      };
      new TestEntity6 {
          BooleanField = true,
          Int16Field = 6,
          Int32Field = 6,
          Int64Field = 6,
          FloatField = 6,
          DoubleField = 6,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity6",
          Text = "This is an instance of TestEntity6",
      };
      new TestEntity7 {
          BooleanField = true,
          Int16Field = 7,
          Int32Field = 7,
          Int64Field = 7,
          FloatField = 7,
          DoubleField = 7,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity7",
          Text = "This is an instance of TestEntity7",
      };
      new TestEntity8 {
          BooleanField = true,
          Int16Field = 8,
          Int32Field = 8,
          Int64Field = 8,
          FloatField = 8,
          DoubleField = 8,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity8",
          Text = "This is an instance of TestEntity8",
      };
      new TestEntity9 {
          BooleanField = true,
          Int16Field = 9,
          Int32Field = 9,
          Int64Field = 9,
          FloatField = 9,
          DoubleField = 9,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity9",
          Text = "This is an instance of TestEntity9",
      };
      new TestEntity10 {
          BooleanField = true,
          Int16Field = 10,
          Int32Field = 10,
          Int64Field = 10,
          FloatField = 10,
          DoubleField = 10,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity10",
          Text = "This is an instance of TestEntity10",
      };
      new TestEntity11 {
          BooleanField = true,
          Int16Field = 11,
          Int32Field = 11,
          Int64Field = 11,
          FloatField = 11,
          DoubleField = 11,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity11",
          Text = "This is an instance of TestEntity11",
      };
      new TestEntity12 {
          BooleanField = true,
          Int16Field = 12,
          Int32Field = 12,
          Int64Field = 12,
          FloatField = 12,
          DoubleField = 12,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity12",
          Text = "This is an instance of TestEntity12",
      };
      new TestEntity13 {
          BooleanField = true,
          Int16Field = 13,
          Int32Field = 13,
          Int64Field = 13,
          FloatField = 13,
          DoubleField = 13,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity13",
          Text = "This is an instance of TestEntity13",
      };
      new TestEntity14 {
          BooleanField = true,
          Int16Field = 14,
          Int32Field = 14,
          Int64Field = 14,
          FloatField = 14,
          DoubleField = 14,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity14",
          Text = "This is an instance of TestEntity14",
      };
      new TestEntity15 {
          BooleanField = true,
          Int16Field = 15,
          Int32Field = 15,
          Int64Field = 15,
          FloatField = 15,
          DoubleField = 15,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity15",
          Text = "This is an instance of TestEntity15",
      };
      new TestEntity16 {
          BooleanField = true,
          Int16Field = 16,
          Int32Field = 16,
          Int64Field = 16,
          FloatField = 16,
          DoubleField = 16,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity16",
          Text = "This is an instance of TestEntity16",
      };
      new TestEntity17 {
          BooleanField = true,
          Int16Field = 17,
          Int32Field = 17,
          Int64Field = 17,
          FloatField = 17,
          DoubleField = 17,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity17",
          Text = "This is an instance of TestEntity17",
      };
      new TestEntity18 {
          BooleanField = true,
          Int16Field = 18,
          Int32Field = 18,
          Int64Field = 18,
          FloatField = 18,
          DoubleField = 18,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity18",
          Text = "This is an instance of TestEntity18",
      };
      new TestEntity19 {
          BooleanField = true,
          Int16Field = 19,
          Int32Field = 19,
          Int64Field = 19,
          FloatField = 19,
          DoubleField = 19,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity19",
          Text = "This is an instance of TestEntity19",
      };
      new TestEntity20 {
          BooleanField = true,
          Int16Field = 20,
          Int32Field = 20,
          Int64Field = 20,
          FloatField = 20,
          DoubleField = 20,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity20",
          Text = "This is an instance of TestEntity20",
      };
      new TestEntity21 {
          BooleanField = true,
          Int16Field = 21,
          Int32Field = 21,
          Int64Field = 21,
          FloatField = 21,
          DoubleField = 21,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity21",
          Text = "This is an instance of TestEntity21",
      };
      new TestEntity22 {
          BooleanField = true,
          Int16Field = 22,
          Int32Field = 22,
          Int64Field = 22,
          FloatField = 22,
          DoubleField = 22,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity22",
          Text = "This is an instance of TestEntity22",
      };
      new TestEntity23 {
          BooleanField = true,
          Int16Field = 23,
          Int32Field = 23,
          Int64Field = 23,
          FloatField = 23,
          DoubleField = 23,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity23",
          Text = "This is an instance of TestEntity23",
      };
      new TestEntity24 {
          BooleanField = true,
          Int16Field = 24,
          Int32Field = 24,
          Int64Field = 24,
          FloatField = 24,
          DoubleField = 24,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity24",
          Text = "This is an instance of TestEntity24",
      };
      new TestEntity25 {
          BooleanField = true,
          Int16Field = 25,
          Int32Field = 25,
          Int64Field = 25,
          FloatField = 25,
          DoubleField = 25,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity25",
          Text = "This is an instance of TestEntity25",
      };
      new TestEntity26 {
          BooleanField = true,
          Int16Field = 26,
          Int32Field = 26,
          Int64Field = 26,
          FloatField = 26,
          DoubleField = 26,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity26",
          Text = "This is an instance of TestEntity26",
      };
      new TestEntity27 {
          BooleanField = true,
          Int16Field = 27,
          Int32Field = 27,
          Int64Field = 27,
          FloatField = 27,
          DoubleField = 27,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity27",
          Text = "This is an instance of TestEntity27",
      };
      new TestEntity28 {
          BooleanField = true,
          Int16Field = 28,
          Int32Field = 28,
          Int64Field = 28,
          FloatField = 28,
          DoubleField = 28,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity28",
          Text = "This is an instance of TestEntity28",
      };
      new TestEntity29 {
          BooleanField = true,
          Int16Field = 29,
          Int32Field = 29,
          Int64Field = 29,
          FloatField = 29,
          DoubleField = 29,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity29",
          Text = "This is an instance of TestEntity29",
      };
      new TestEntity30 {
          BooleanField = true,
          Int16Field = 30,
          Int32Field = 30,
          Int64Field = 30,
          FloatField = 30,
          DoubleField = 30,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity30",
          Text = "This is an instance of TestEntity30",
      };
      new TestEntity31 {
          BooleanField = true,
          Int16Field = 31,
          Int32Field = 31,
          Int64Field = 31,
          FloatField = 31,
          DoubleField = 31,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity31",
          Text = "This is an instance of TestEntity31",
      };
      new TestEntity32 {
          BooleanField = true,
          Int16Field = 32,
          Int32Field = 32,
          Int64Field = 32,
          FloatField = 32,
          DoubleField = 32,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity32",
          Text = "This is an instance of TestEntity32",
      };
      new TestEntity33 {
          BooleanField = true,
          Int16Field = 33,
          Int32Field = 33,
          Int64Field = 33,
          FloatField = 33,
          DoubleField = 33,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity33",
          Text = "This is an instance of TestEntity33",
      };
      new TestEntity34 {
          BooleanField = true,
          Int16Field = 34,
          Int32Field = 34,
          Int64Field = 34,
          FloatField = 34,
          DoubleField = 34,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity34",
          Text = "This is an instance of TestEntity34",
      };
      new TestEntity35 {
          BooleanField = true,
          Int16Field = 35,
          Int32Field = 35,
          Int64Field = 35,
          FloatField = 35,
          DoubleField = 35,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity35",
          Text = "This is an instance of TestEntity35",
      };
      new TestEntity36 {
          BooleanField = true,
          Int16Field = 36,
          Int32Field = 36,
          Int64Field = 36,
          FloatField = 36,
          DoubleField = 36,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity36",
          Text = "This is an instance of TestEntity36",
      };
      new TestEntity37 {
          BooleanField = true,
          Int16Field = 37,
          Int32Field = 37,
          Int64Field = 37,
          FloatField = 37,
          DoubleField = 37,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity37",
          Text = "This is an instance of TestEntity37",
      };
      new TestEntity38 {
          BooleanField = true,
          Int16Field = 38,
          Int32Field = 38,
          Int64Field = 38,
          FloatField = 38,
          DoubleField = 38,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity38",
          Text = "This is an instance of TestEntity38",
      };
      new TestEntity39 {
          BooleanField = true,
          Int16Field = 39,
          Int32Field = 39,
          Int64Field = 39,
          FloatField = 39,
          DoubleField = 39,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity39",
          Text = "This is an instance of TestEntity39",
      };
      new TestEntity40 {
          BooleanField = true,
          Int16Field = 40,
          Int32Field = 40,
          Int64Field = 40,
          FloatField = 40,
          DoubleField = 40,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity40",
          Text = "This is an instance of TestEntity40",
      };
      new TestEntity41 {
          BooleanField = true,
          Int16Field = 41,
          Int32Field = 41,
          Int64Field = 41,
          FloatField = 41,
          DoubleField = 41,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity41",
          Text = "This is an instance of TestEntity41",
      };
      new TestEntity42 {
          BooleanField = true,
          Int16Field = 42,
          Int32Field = 42,
          Int64Field = 42,
          FloatField = 42,
          DoubleField = 42,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity42",
          Text = "This is an instance of TestEntity42",
      };
      new TestEntity43 {
          BooleanField = true,
          Int16Field = 43,
          Int32Field = 43,
          Int64Field = 43,
          FloatField = 43,
          DoubleField = 43,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity43",
          Text = "This is an instance of TestEntity43",
      };
      new TestEntity44 {
          BooleanField = true,
          Int16Field = 44,
          Int32Field = 44,
          Int64Field = 44,
          FloatField = 44,
          DoubleField = 44,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity44",
          Text = "This is an instance of TestEntity44",
      };
      new TestEntity45 {
          BooleanField = true,
          Int16Field = 45,
          Int32Field = 45,
          Int64Field = 45,
          FloatField = 45,
          DoubleField = 45,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity45",
          Text = "This is an instance of TestEntity45",
      };
      new TestEntity46 {
          BooleanField = true,
          Int16Field = 46,
          Int32Field = 46,
          Int64Field = 46,
          FloatField = 46,
          DoubleField = 46,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity46",
          Text = "This is an instance of TestEntity46",
      };
      new TestEntity47 {
          BooleanField = true,
          Int16Field = 47,
          Int32Field = 47,
          Int64Field = 47,
          FloatField = 47,
          DoubleField = 47,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity47",
          Text = "This is an instance of TestEntity47",
      };
      new TestEntity48 {
          BooleanField = true,
          Int16Field = 48,
          Int32Field = 48,
          Int64Field = 48,
          FloatField = 48,
          DoubleField = 48,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity48",
          Text = "This is an instance of TestEntity48",
      };
      new TestEntity49 {
          BooleanField = true,
          Int16Field = 49,
          Int32Field = 49,
          Int64Field = 49,
          FloatField = 49,
          DoubleField = 49,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity49",
          Text = "This is an instance of TestEntity49",
      };
      new TestEntity50 {
          BooleanField = true,
          Int16Field = 50,
          Int32Field = 50,
          Int64Field = 50,
          FloatField = 50,
          DoubleField = 50,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity50",
          Text = "This is an instance of TestEntity50",
      };
      new TestEntity51 {
          BooleanField = true,
          Int16Field = 51,
          Int32Field = 51,
          Int64Field = 51,
          FloatField = 51,
          DoubleField = 51,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity51",
          Text = "This is an instance of TestEntity51",
      };
      new TestEntity52 {
          BooleanField = true,
          Int16Field = 52,
          Int32Field = 52,
          Int64Field = 52,
          FloatField = 52,
          DoubleField = 52,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity52",
          Text = "This is an instance of TestEntity52",
      };
      new TestEntity53 {
          BooleanField = true,
          Int16Field = 53,
          Int32Field = 53,
          Int64Field = 53,
          FloatField = 53,
          DoubleField = 53,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity53",
          Text = "This is an instance of TestEntity53",
      };
      new TestEntity54 {
          BooleanField = true,
          Int16Field = 54,
          Int32Field = 54,
          Int64Field = 54,
          FloatField = 54,
          DoubleField = 54,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity54",
          Text = "This is an instance of TestEntity54",
      };
      new TestEntity55 {
          BooleanField = true,
          Int16Field = 55,
          Int32Field = 55,
          Int64Field = 55,
          FloatField = 55,
          DoubleField = 55,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity55",
          Text = "This is an instance of TestEntity55",
      };
      new TestEntity56 {
          BooleanField = true,
          Int16Field = 56,
          Int32Field = 56,
          Int64Field = 56,
          FloatField = 56,
          DoubleField = 56,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity56",
          Text = "This is an instance of TestEntity56",
      };
      new TestEntity57 {
          BooleanField = true,
          Int16Field = 57,
          Int32Field = 57,
          Int64Field = 57,
          FloatField = 57,
          DoubleField = 57,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity57",
          Text = "This is an instance of TestEntity57",
      };
      new TestEntity58 {
          BooleanField = true,
          Int16Field = 58,
          Int32Field = 58,
          Int64Field = 58,
          FloatField = 58,
          DoubleField = 58,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity58",
          Text = "This is an instance of TestEntity58",
      };
      new TestEntity59 {
          BooleanField = true,
          Int16Field = 59,
          Int32Field = 59,
          Int64Field = 59,
          FloatField = 59,
          DoubleField = 59,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity59",
          Text = "This is an instance of TestEntity59",
      };
      new TestEntity60 {
          BooleanField = true,
          Int16Field = 60,
          Int32Field = 60,
          Int64Field = 60,
          FloatField = 60,
          DoubleField = 60,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity60",
          Text = "This is an instance of TestEntity60",
      };
      new TestEntity61 {
          BooleanField = true,
          Int16Field = 61,
          Int32Field = 61,
          Int64Field = 61,
          FloatField = 61,
          DoubleField = 61,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity61",
          Text = "This is an instance of TestEntity61",
      };
      new TestEntity62 {
          BooleanField = true,
          Int16Field = 62,
          Int32Field = 62,
          Int64Field = 62,
          FloatField = 62,
          DoubleField = 62,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity62",
          Text = "This is an instance of TestEntity62",
      };
      new TestEntity63 {
          BooleanField = true,
          Int16Field = 63,
          Int32Field = 63,
          Int64Field = 63,
          FloatField = 63,
          DoubleField = 63,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity63",
          Text = "This is an instance of TestEntity63",
      };
      new TestEntity64 {
          BooleanField = true,
          Int16Field = 64,
          Int32Field = 64,
          Int64Field = 64,
          FloatField = 64,
          DoubleField = 64,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity64",
          Text = "This is an instance of TestEntity64",
      };
      new TestEntity65 {
          BooleanField = true,
          Int16Field = 65,
          Int32Field = 65,
          Int64Field = 65,
          FloatField = 65,
          DoubleField = 65,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity65",
          Text = "This is an instance of TestEntity65",
      };
      new TestEntity66 {
          BooleanField = true,
          Int16Field = 66,
          Int32Field = 66,
          Int64Field = 66,
          FloatField = 66,
          DoubleField = 66,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity66",
          Text = "This is an instance of TestEntity66",
      };
      new TestEntity67 {
          BooleanField = true,
          Int16Field = 67,
          Int32Field = 67,
          Int64Field = 67,
          FloatField = 67,
          DoubleField = 67,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity67",
          Text = "This is an instance of TestEntity67",
      };
      new TestEntity68 {
          BooleanField = true,
          Int16Field = 68,
          Int32Field = 68,
          Int64Field = 68,
          FloatField = 68,
          DoubleField = 68,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity68",
          Text = "This is an instance of TestEntity68",
      };
      new TestEntity69 {
          BooleanField = true,
          Int16Field = 69,
          Int32Field = 69,
          Int64Field = 69,
          FloatField = 69,
          DoubleField = 69,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity69",
          Text = "This is an instance of TestEntity69",
      };
      new TestEntity70 {
          BooleanField = true,
          Int16Field = 70,
          Int32Field = 70,
          Int64Field = 70,
          FloatField = 70,
          DoubleField = 70,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity70",
          Text = "This is an instance of TestEntity70",
      };
      new TestEntity71 {
          BooleanField = true,
          Int16Field = 71,
          Int32Field = 71,
          Int64Field = 71,
          FloatField = 71,
          DoubleField = 71,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity71",
          Text = "This is an instance of TestEntity71",
      };
      new TestEntity72 {
          BooleanField = true,
          Int16Field = 72,
          Int32Field = 72,
          Int64Field = 72,
          FloatField = 72,
          DoubleField = 72,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity72",
          Text = "This is an instance of TestEntity72",
      };
      new TestEntity73 {
          BooleanField = true,
          Int16Field = 73,
          Int32Field = 73,
          Int64Field = 73,
          FloatField = 73,
          DoubleField = 73,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity73",
          Text = "This is an instance of TestEntity73",
      };
      new TestEntity74 {
          BooleanField = true,
          Int16Field = 74,
          Int32Field = 74,
          Int64Field = 74,
          FloatField = 74,
          DoubleField = 74,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity74",
          Text = "This is an instance of TestEntity74",
      };
      new TestEntity75 {
          BooleanField = true,
          Int16Field = 75,
          Int32Field = 75,
          Int64Field = 75,
          FloatField = 75,
          DoubleField = 75,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity75",
          Text = "This is an instance of TestEntity75",
      };
      new TestEntity76 {
          BooleanField = true,
          Int16Field = 76,
          Int32Field = 76,
          Int64Field = 76,
          FloatField = 76,
          DoubleField = 76,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity76",
          Text = "This is an instance of TestEntity76",
      };
      new TestEntity77 {
          BooleanField = true,
          Int16Field = 77,
          Int32Field = 77,
          Int64Field = 77,
          FloatField = 77,
          DoubleField = 77,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity77",
          Text = "This is an instance of TestEntity77",
      };
      new TestEntity78 {
          BooleanField = true,
          Int16Field = 78,
          Int32Field = 78,
          Int64Field = 78,
          FloatField = 78,
          DoubleField = 78,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity78",
          Text = "This is an instance of TestEntity78",
      };
      new TestEntity79 {
          BooleanField = true,
          Int16Field = 79,
          Int32Field = 79,
          Int64Field = 79,
          FloatField = 79,
          DoubleField = 79,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity79",
          Text = "This is an instance of TestEntity79",
      };
      new TestEntity80 {
          BooleanField = true,
          Int16Field = 80,
          Int32Field = 80,
          Int64Field = 80,
          FloatField = 80,
          DoubleField = 80,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity80",
          Text = "This is an instance of TestEntity80",
      };
      new TestEntity81 {
          BooleanField = true,
          Int16Field = 81,
          Int32Field = 81,
          Int64Field = 81,
          FloatField = 81,
          DoubleField = 81,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity81",
          Text = "This is an instance of TestEntity81",
      };
      new TestEntity82 {
          BooleanField = true,
          Int16Field = 82,
          Int32Field = 82,
          Int64Field = 82,
          FloatField = 82,
          DoubleField = 82,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity82",
          Text = "This is an instance of TestEntity82",
      };
      new TestEntity83 {
          BooleanField = true,
          Int16Field = 83,
          Int32Field = 83,
          Int64Field = 83,
          FloatField = 83,
          DoubleField = 83,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity83",
          Text = "This is an instance of TestEntity83",
      };
      new TestEntity84 {
          BooleanField = true,
          Int16Field = 84,
          Int32Field = 84,
          Int64Field = 84,
          FloatField = 84,
          DoubleField = 84,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity84",
          Text = "This is an instance of TestEntity84",
      };
      new TestEntity85 {
          BooleanField = true,
          Int16Field = 85,
          Int32Field = 85,
          Int64Field = 85,
          FloatField = 85,
          DoubleField = 85,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity85",
          Text = "This is an instance of TestEntity85",
      };
      new TestEntity86 {
          BooleanField = true,
          Int16Field = 86,
          Int32Field = 86,
          Int64Field = 86,
          FloatField = 86,
          DoubleField = 86,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity86",
          Text = "This is an instance of TestEntity86",
      };
      new TestEntity87 {
          BooleanField = true,
          Int16Field = 87,
          Int32Field = 87,
          Int64Field = 87,
          FloatField = 87,
          DoubleField = 87,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity87",
          Text = "This is an instance of TestEntity87",
      };
      new TestEntity88 {
          BooleanField = true,
          Int16Field = 88,
          Int32Field = 88,
          Int64Field = 88,
          FloatField = 88,
          DoubleField = 88,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity88",
          Text = "This is an instance of TestEntity88",
      };
      new TestEntity89 {
          BooleanField = true,
          Int16Field = 89,
          Int32Field = 89,
          Int64Field = 89,
          FloatField = 89,
          DoubleField = 89,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity89",
          Text = "This is an instance of TestEntity89",
      };
      new TestEntity90 {
          BooleanField = true,
          Int16Field = 90,
          Int32Field = 90,
          Int64Field = 90,
          FloatField = 90,
          DoubleField = 90,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity90",
          Text = "This is an instance of TestEntity90",
      };
      new TestEntity91 {
          BooleanField = true,
          Int16Field = 91,
          Int32Field = 91,
          Int64Field = 91,
          FloatField = 91,
          DoubleField = 91,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity91",
          Text = "This is an instance of TestEntity91",
      };
      new TestEntity92 {
          BooleanField = true,
          Int16Field = 92,
          Int32Field = 92,
          Int64Field = 92,
          FloatField = 92,
          DoubleField = 92,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity92",
          Text = "This is an instance of TestEntity92",
      };
      new TestEntity93 {
          BooleanField = true,
          Int16Field = 93,
          Int32Field = 93,
          Int64Field = 93,
          FloatField = 93,
          DoubleField = 93,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity93",
          Text = "This is an instance of TestEntity93",
      };
      new TestEntity94 {
          BooleanField = true,
          Int16Field = 94,
          Int32Field = 94,
          Int64Field = 94,
          FloatField = 94,
          DoubleField = 94,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity94",
          Text = "This is an instance of TestEntity94",
      };
      new TestEntity95 {
          BooleanField = true,
          Int16Field = 95,
          Int32Field = 95,
          Int64Field = 95,
          FloatField = 95,
          DoubleField = 95,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity95",
          Text = "This is an instance of TestEntity95",
      };
      new TestEntity96 {
          BooleanField = true,
          Int16Field = 96,
          Int32Field = 96,
          Int64Field = 96,
          FloatField = 96,
          DoubleField = 96,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity96",
          Text = "This is an instance of TestEntity96",
      };
      new TestEntity97 {
          BooleanField = true,
          Int16Field = 97,
          Int32Field = 97,
          Int64Field = 97,
          FloatField = 97,
          DoubleField = 97,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity97",
          Text = "This is an instance of TestEntity97",
      };
      new TestEntity98 {
          BooleanField = true,
          Int16Field = 98,
          Int32Field = 98,
          Int64Field = 98,
          FloatField = 98,
          DoubleField = 98,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity98",
          Text = "This is an instance of TestEntity98",
      };
      new TestEntity99 {
          BooleanField = true,
          Int16Field = 99,
          Int32Field = 99,
          Int64Field = 99,
          FloatField = 99,
          DoubleField = 99,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity99",
          Text = "This is an instance of TestEntity99",
      };
      new TestEntity100 {
          BooleanField = true,
          Int16Field = 100,
          Int32Field = 100,
          Int64Field = 100,
          FloatField = 100,
          DoubleField = 100,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity100",
          Text = "This is an instance of TestEntity100",
      };
      new TestEntity101 {
          BooleanField = true,
          Int16Field = 101,
          Int32Field = 101,
          Int64Field = 101,
          FloatField = 101,
          DoubleField = 101,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity101",
          Text = "This is an instance of TestEntity101",
      };
      new TestEntity102 {
          BooleanField = true,
          Int16Field = 102,
          Int32Field = 102,
          Int64Field = 102,
          FloatField = 102,
          DoubleField = 102,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity102",
          Text = "This is an instance of TestEntity102",
      };
      new TestEntity103 {
          BooleanField = true,
          Int16Field = 103,
          Int32Field = 103,
          Int64Field = 103,
          FloatField = 103,
          DoubleField = 103,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity103",
          Text = "This is an instance of TestEntity103",
      };
      new TestEntity104 {
          BooleanField = true,
          Int16Field = 104,
          Int32Field = 104,
          Int64Field = 104,
          FloatField = 104,
          DoubleField = 104,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity104",
          Text = "This is an instance of TestEntity104",
      };
      new TestEntity105 {
          BooleanField = true,
          Int16Field = 105,
          Int32Field = 105,
          Int64Field = 105,
          FloatField = 105,
          DoubleField = 105,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity105",
          Text = "This is an instance of TestEntity105",
      };
      new TestEntity106 {
          BooleanField = true,
          Int16Field = 106,
          Int32Field = 106,
          Int64Field = 106,
          FloatField = 106,
          DoubleField = 106,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity106",
          Text = "This is an instance of TestEntity106",
      };
      new TestEntity107 {
          BooleanField = true,
          Int16Field = 107,
          Int32Field = 107,
          Int64Field = 107,
          FloatField = 107,
          DoubleField = 107,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity107",
          Text = "This is an instance of TestEntity107",
      };
      new TestEntity108 {
          BooleanField = true,
          Int16Field = 108,
          Int32Field = 108,
          Int64Field = 108,
          FloatField = 108,
          DoubleField = 108,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity108",
          Text = "This is an instance of TestEntity108",
      };
      new TestEntity109 {
          BooleanField = true,
          Int16Field = 109,
          Int32Field = 109,
          Int64Field = 109,
          FloatField = 109,
          DoubleField = 109,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity109",
          Text = "This is an instance of TestEntity109",
      };
      new TestEntity110 {
          BooleanField = true,
          Int16Field = 110,
          Int32Field = 110,
          Int64Field = 110,
          FloatField = 110,
          DoubleField = 110,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity110",
          Text = "This is an instance of TestEntity110",
      };
      new TestEntity111 {
          BooleanField = true,
          Int16Field = 111,
          Int32Field = 111,
          Int64Field = 111,
          FloatField = 111,
          DoubleField = 111,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity111",
          Text = "This is an instance of TestEntity111",
      };
      new TestEntity112 {
          BooleanField = true,
          Int16Field = 112,
          Int32Field = 112,
          Int64Field = 112,
          FloatField = 112,
          DoubleField = 112,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity112",
          Text = "This is an instance of TestEntity112",
      };
      new TestEntity113 {
          BooleanField = true,
          Int16Field = 113,
          Int32Field = 113,
          Int64Field = 113,
          FloatField = 113,
          DoubleField = 113,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity113",
          Text = "This is an instance of TestEntity113",
      };
      new TestEntity114 {
          BooleanField = true,
          Int16Field = 114,
          Int32Field = 114,
          Int64Field = 114,
          FloatField = 114,
          DoubleField = 114,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity114",
          Text = "This is an instance of TestEntity114",
      };
      new TestEntity115 {
          BooleanField = true,
          Int16Field = 115,
          Int32Field = 115,
          Int64Field = 115,
          FloatField = 115,
          DoubleField = 115,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity115",
          Text = "This is an instance of TestEntity115",
      };
      new TestEntity116 {
          BooleanField = true,
          Int16Field = 116,
          Int32Field = 116,
          Int64Field = 116,
          FloatField = 116,
          DoubleField = 116,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity116",
          Text = "This is an instance of TestEntity116",
      };
      new TestEntity117 {
          BooleanField = true,
          Int16Field = 117,
          Int32Field = 117,
          Int64Field = 117,
          FloatField = 117,
          DoubleField = 117,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity117",
          Text = "This is an instance of TestEntity117",
      };
      new TestEntity118 {
          BooleanField = true,
          Int16Field = 118,
          Int32Field = 118,
          Int64Field = 118,
          FloatField = 118,
          DoubleField = 118,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity118",
          Text = "This is an instance of TestEntity118",
      };
      new TestEntity119 {
          BooleanField = true,
          Int16Field = 119,
          Int32Field = 119,
          Int64Field = 119,
          FloatField = 119,
          DoubleField = 119,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity119",
          Text = "This is an instance of TestEntity119",
      };
      new TestEntity120 {
          BooleanField = true,
          Int16Field = 120,
          Int32Field = 120,
          Int64Field = 120,
          FloatField = 120,
          DoubleField = 120,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity120",
          Text = "This is an instance of TestEntity120",
      };
      new TestEntity121 {
          BooleanField = true,
          Int16Field = 121,
          Int32Field = 121,
          Int64Field = 121,
          FloatField = 121,
          DoubleField = 121,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity121",
          Text = "This is an instance of TestEntity121",
      };
      new TestEntity122 {
          BooleanField = true,
          Int16Field = 122,
          Int32Field = 122,
          Int64Field = 122,
          FloatField = 122,
          DoubleField = 122,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity122",
          Text = "This is an instance of TestEntity122",
      };
      new TestEntity123 {
          BooleanField = true,
          Int16Field = 123,
          Int32Field = 123,
          Int64Field = 123,
          FloatField = 123,
          DoubleField = 123,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity123",
          Text = "This is an instance of TestEntity123",
      };
      new TestEntity124 {
          BooleanField = true,
          Int16Field = 124,
          Int32Field = 124,
          Int64Field = 124,
          FloatField = 124,
          DoubleField = 124,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity124",
          Text = "This is an instance of TestEntity124",
      };
      new TestEntity125 {
          BooleanField = true,
          Int16Field = 125,
          Int32Field = 125,
          Int64Field = 125,
          FloatField = 125,
          DoubleField = 125,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity125",
          Text = "This is an instance of TestEntity125",
      };
      new TestEntity126 {
          BooleanField = true,
          Int16Field = 126,
          Int32Field = 126,
          Int64Field = 126,
          FloatField = 126,
          DoubleField = 126,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity126",
          Text = "This is an instance of TestEntity126",
      };
      new TestEntity127 {
          BooleanField = true,
          Int16Field = 127,
          Int32Field = 127,
          Int64Field = 127,
          FloatField = 127,
          DoubleField = 127,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity127",
          Text = "This is an instance of TestEntity127",
      };
      new TestEntity128 {
          BooleanField = true,
          Int16Field = 128,
          Int32Field = 128,
          Int64Field = 128,
          FloatField = 128,
          DoubleField = 128,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity128",
          Text = "This is an instance of TestEntity128",
      };
      new TestEntity129 {
          BooleanField = true,
          Int16Field = 129,
          Int32Field = 129,
          Int64Field = 129,
          FloatField = 129,
          DoubleField = 129,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity129",
          Text = "This is an instance of TestEntity129",
      };
      new TestEntity130 {
          BooleanField = true,
          Int16Field = 130,
          Int32Field = 130,
          Int64Field = 130,
          FloatField = 130,
          DoubleField = 130,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity130",
          Text = "This is an instance of TestEntity130",
      };
      new TestEntity131 {
          BooleanField = true,
          Int16Field = 131,
          Int32Field = 131,
          Int64Field = 131,
          FloatField = 131,
          DoubleField = 131,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity131",
          Text = "This is an instance of TestEntity131",
      };
      new TestEntity132 {
          BooleanField = true,
          Int16Field = 132,
          Int32Field = 132,
          Int64Field = 132,
          FloatField = 132,
          DoubleField = 132,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity132",
          Text = "This is an instance of TestEntity132",
      };
      new TestEntity133 {
          BooleanField = true,
          Int16Field = 133,
          Int32Field = 133,
          Int64Field = 133,
          FloatField = 133,
          DoubleField = 133,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity133",
          Text = "This is an instance of TestEntity133",
      };
      new TestEntity134 {
          BooleanField = true,
          Int16Field = 134,
          Int32Field = 134,
          Int64Field = 134,
          FloatField = 134,
          DoubleField = 134,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity134",
          Text = "This is an instance of TestEntity134",
      };
      new TestEntity135 {
          BooleanField = true,
          Int16Field = 135,
          Int32Field = 135,
          Int64Field = 135,
          FloatField = 135,
          DoubleField = 135,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity135",
          Text = "This is an instance of TestEntity135",
      };
      new TestEntity136 {
          BooleanField = true,
          Int16Field = 136,
          Int32Field = 136,
          Int64Field = 136,
          FloatField = 136,
          DoubleField = 136,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity136",
          Text = "This is an instance of TestEntity136",
      };
      new TestEntity137 {
          BooleanField = true,
          Int16Field = 137,
          Int32Field = 137,
          Int64Field = 137,
          FloatField = 137,
          DoubleField = 137,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity137",
          Text = "This is an instance of TestEntity137",
      };
      new TestEntity138 {
          BooleanField = true,
          Int16Field = 138,
          Int32Field = 138,
          Int64Field = 138,
          FloatField = 138,
          DoubleField = 138,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity138",
          Text = "This is an instance of TestEntity138",
      };
      new TestEntity139 {
          BooleanField = true,
          Int16Field = 139,
          Int32Field = 139,
          Int64Field = 139,
          FloatField = 139,
          DoubleField = 139,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity139",
          Text = "This is an instance of TestEntity139",
      };
      new TestEntity140 {
          BooleanField = true,
          Int16Field = 140,
          Int32Field = 140,
          Int64Field = 140,
          FloatField = 140,
          DoubleField = 140,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity140",
          Text = "This is an instance of TestEntity140",
      };
      new TestEntity141 {
          BooleanField = true,
          Int16Field = 141,
          Int32Field = 141,
          Int64Field = 141,
          FloatField = 141,
          DoubleField = 141,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity141",
          Text = "This is an instance of TestEntity141",
      };
      new TestEntity142 {
          BooleanField = true,
          Int16Field = 142,
          Int32Field = 142,
          Int64Field = 142,
          FloatField = 142,
          DoubleField = 142,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity142",
          Text = "This is an instance of TestEntity142",
      };
      new TestEntity143 {
          BooleanField = true,
          Int16Field = 143,
          Int32Field = 143,
          Int64Field = 143,
          FloatField = 143,
          DoubleField = 143,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity143",
          Text = "This is an instance of TestEntity143",
      };
      new TestEntity144 {
          BooleanField = true,
          Int16Field = 144,
          Int32Field = 144,
          Int64Field = 144,
          FloatField = 144,
          DoubleField = 144,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity144",
          Text = "This is an instance of TestEntity144",
      };
      new TestEntity145 {
          BooleanField = true,
          Int16Field = 145,
          Int32Field = 145,
          Int64Field = 145,
          FloatField = 145,
          DoubleField = 145,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity145",
          Text = "This is an instance of TestEntity145",
      };
      new TestEntity146 {
          BooleanField = true,
          Int16Field = 146,
          Int32Field = 146,
          Int64Field = 146,
          FloatField = 146,
          DoubleField = 146,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity146",
          Text = "This is an instance of TestEntity146",
      };
      new TestEntity147 {
          BooleanField = true,
          Int16Field = 147,
          Int32Field = 147,
          Int64Field = 147,
          FloatField = 147,
          DoubleField = 147,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity147",
          Text = "This is an instance of TestEntity147",
      };
      new TestEntity148 {
          BooleanField = true,
          Int16Field = 148,
          Int32Field = 148,
          Int64Field = 148,
          FloatField = 148,
          DoubleField = 148,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity148",
          Text = "This is an instance of TestEntity148",
      };
      new TestEntity149 {
          BooleanField = true,
          Int16Field = 149,
          Int32Field = 149,
          Int64Field = 149,
          FloatField = 149,
          DoubleField = 149,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity149",
          Text = "This is an instance of TestEntity149",
      };
      new TestEntity150 {
          BooleanField = true,
          Int16Field = 150,
          Int32Field = 150,
          Int64Field = 150,
          FloatField = 150,
          DoubleField = 150,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity150",
          Text = "This is an instance of TestEntity150",
      };
      new TestEntity151 {
          BooleanField = true,
          Int16Field = 151,
          Int32Field = 151,
          Int64Field = 151,
          FloatField = 151,
          DoubleField = 151,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity151",
          Text = "This is an instance of TestEntity151",
      };
      new TestEntity152 {
          BooleanField = true,
          Int16Field = 152,
          Int32Field = 152,
          Int64Field = 152,
          FloatField = 152,
          DoubleField = 152,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity152",
          Text = "This is an instance of TestEntity152",
      };
      new TestEntity153 {
          BooleanField = true,
          Int16Field = 153,
          Int32Field = 153,
          Int64Field = 153,
          FloatField = 153,
          DoubleField = 153,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity153",
          Text = "This is an instance of TestEntity153",
      };
      new TestEntity154 {
          BooleanField = true,
          Int16Field = 154,
          Int32Field = 154,
          Int64Field = 154,
          FloatField = 154,
          DoubleField = 154,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity154",
          Text = "This is an instance of TestEntity154",
      };
      new TestEntity155 {
          BooleanField = true,
          Int16Field = 155,
          Int32Field = 155,
          Int64Field = 155,
          FloatField = 155,
          DoubleField = 155,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity155",
          Text = "This is an instance of TestEntity155",
      };
      new TestEntity156 {
          BooleanField = true,
          Int16Field = 156,
          Int32Field = 156,
          Int64Field = 156,
          FloatField = 156,
          DoubleField = 156,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity156",
          Text = "This is an instance of TestEntity156",
      };
      new TestEntity157 {
          BooleanField = true,
          Int16Field = 157,
          Int32Field = 157,
          Int64Field = 157,
          FloatField = 157,
          DoubleField = 157,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity157",
          Text = "This is an instance of TestEntity157",
      };
      new TestEntity158 {
          BooleanField = true,
          Int16Field = 158,
          Int32Field = 158,
          Int64Field = 158,
          FloatField = 158,
          DoubleField = 158,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity158",
          Text = "This is an instance of TestEntity158",
      };
      new TestEntity159 {
          BooleanField = true,
          Int16Field = 159,
          Int32Field = 159,
          Int64Field = 159,
          FloatField = 159,
          DoubleField = 159,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity159",
          Text = "This is an instance of TestEntity159",
      };
      new TestEntity160 {
          BooleanField = true,
          Int16Field = 160,
          Int32Field = 160,
          Int64Field = 160,
          FloatField = 160,
          DoubleField = 160,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity160",
          Text = "This is an instance of TestEntity160",
      };
      new TestEntity161 {
          BooleanField = true,
          Int16Field = 161,
          Int32Field = 161,
          Int64Field = 161,
          FloatField = 161,
          DoubleField = 161,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity161",
          Text = "This is an instance of TestEntity161",
      };
      new TestEntity162 {
          BooleanField = true,
          Int16Field = 162,
          Int32Field = 162,
          Int64Field = 162,
          FloatField = 162,
          DoubleField = 162,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity162",
          Text = "This is an instance of TestEntity162",
      };
      new TestEntity163 {
          BooleanField = true,
          Int16Field = 163,
          Int32Field = 163,
          Int64Field = 163,
          FloatField = 163,
          DoubleField = 163,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity163",
          Text = "This is an instance of TestEntity163",
      };
      new TestEntity164 {
          BooleanField = true,
          Int16Field = 164,
          Int32Field = 164,
          Int64Field = 164,
          FloatField = 164,
          DoubleField = 164,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity164",
          Text = "This is an instance of TestEntity164",
      };
      new TestEntity165 {
          BooleanField = true,
          Int16Field = 165,
          Int32Field = 165,
          Int64Field = 165,
          FloatField = 165,
          DoubleField = 165,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity165",
          Text = "This is an instance of TestEntity165",
      };
      new TestEntity166 {
          BooleanField = true,
          Int16Field = 166,
          Int32Field = 166,
          Int64Field = 166,
          FloatField = 166,
          DoubleField = 166,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity166",
          Text = "This is an instance of TestEntity166",
      };
      new TestEntity167 {
          BooleanField = true,
          Int16Field = 167,
          Int32Field = 167,
          Int64Field = 167,
          FloatField = 167,
          DoubleField = 167,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity167",
          Text = "This is an instance of TestEntity167",
      };
      new TestEntity168 {
          BooleanField = true,
          Int16Field = 168,
          Int32Field = 168,
          Int64Field = 168,
          FloatField = 168,
          DoubleField = 168,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity168",
          Text = "This is an instance of TestEntity168",
      };
      new TestEntity169 {
          BooleanField = true,
          Int16Field = 169,
          Int32Field = 169,
          Int64Field = 169,
          FloatField = 169,
          DoubleField = 169,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity169",
          Text = "This is an instance of TestEntity169",
      };
      new TestEntity170 {
          BooleanField = true,
          Int16Field = 170,
          Int32Field = 170,
          Int64Field = 170,
          FloatField = 170,
          DoubleField = 170,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity170",
          Text = "This is an instance of TestEntity170",
      };
      new TestEntity171 {
          BooleanField = true,
          Int16Field = 171,
          Int32Field = 171,
          Int64Field = 171,
          FloatField = 171,
          DoubleField = 171,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity171",
          Text = "This is an instance of TestEntity171",
      };
      new TestEntity172 {
          BooleanField = true,
          Int16Field = 172,
          Int32Field = 172,
          Int64Field = 172,
          FloatField = 172,
          DoubleField = 172,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity172",
          Text = "This is an instance of TestEntity172",
      };
      new TestEntity173 {
          BooleanField = true,
          Int16Field = 173,
          Int32Field = 173,
          Int64Field = 173,
          FloatField = 173,
          DoubleField = 173,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity173",
          Text = "This is an instance of TestEntity173",
      };
      new TestEntity174 {
          BooleanField = true,
          Int16Field = 174,
          Int32Field = 174,
          Int64Field = 174,
          FloatField = 174,
          DoubleField = 174,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity174",
          Text = "This is an instance of TestEntity174",
      };
      new TestEntity175 {
          BooleanField = true,
          Int16Field = 175,
          Int32Field = 175,
          Int64Field = 175,
          FloatField = 175,
          DoubleField = 175,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity175",
          Text = "This is an instance of TestEntity175",
      };
      new TestEntity176 {
          BooleanField = true,
          Int16Field = 176,
          Int32Field = 176,
          Int64Field = 176,
          FloatField = 176,
          DoubleField = 176,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity176",
          Text = "This is an instance of TestEntity176",
      };
      new TestEntity177 {
          BooleanField = true,
          Int16Field = 177,
          Int32Field = 177,
          Int64Field = 177,
          FloatField = 177,
          DoubleField = 177,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity177",
          Text = "This is an instance of TestEntity177",
      };
      new TestEntity178 {
          BooleanField = true,
          Int16Field = 178,
          Int32Field = 178,
          Int64Field = 178,
          FloatField = 178,
          DoubleField = 178,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity178",
          Text = "This is an instance of TestEntity178",
      };
      new TestEntity179 {
          BooleanField = true,
          Int16Field = 179,
          Int32Field = 179,
          Int64Field = 179,
          FloatField = 179,
          DoubleField = 179,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity179",
          Text = "This is an instance of TestEntity179",
      };
      new TestEntity180 {
          BooleanField = true,
          Int16Field = 180,
          Int32Field = 180,
          Int64Field = 180,
          FloatField = 180,
          DoubleField = 180,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity180",
          Text = "This is an instance of TestEntity180",
      };
      new TestEntity181 {
          BooleanField = true,
          Int16Field = 181,
          Int32Field = 181,
          Int64Field = 181,
          FloatField = 181,
          DoubleField = 181,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity181",
          Text = "This is an instance of TestEntity181",
      };
      new TestEntity182 {
          BooleanField = true,
          Int16Field = 182,
          Int32Field = 182,
          Int64Field = 182,
          FloatField = 182,
          DoubleField = 182,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity182",
          Text = "This is an instance of TestEntity182",
      };
      new TestEntity183 {
          BooleanField = true,
          Int16Field = 183,
          Int32Field = 183,
          Int64Field = 183,
          FloatField = 183,
          DoubleField = 183,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity183",
          Text = "This is an instance of TestEntity183",
      };
      new TestEntity184 {
          BooleanField = true,
          Int16Field = 184,
          Int32Field = 184,
          Int64Field = 184,
          FloatField = 184,
          DoubleField = 184,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity184",
          Text = "This is an instance of TestEntity184",
      };
      new TestEntity185 {
          BooleanField = true,
          Int16Field = 185,
          Int32Field = 185,
          Int64Field = 185,
          FloatField = 185,
          DoubleField = 185,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity185",
          Text = "This is an instance of TestEntity185",
      };
      new TestEntity186 {
          BooleanField = true,
          Int16Field = 186,
          Int32Field = 186,
          Int64Field = 186,
          FloatField = 186,
          DoubleField = 186,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity186",
          Text = "This is an instance of TestEntity186",
      };
      new TestEntity187 {
          BooleanField = true,
          Int16Field = 187,
          Int32Field = 187,
          Int64Field = 187,
          FloatField = 187,
          DoubleField = 187,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity187",
          Text = "This is an instance of TestEntity187",
      };
      new TestEntity188 {
          BooleanField = true,
          Int16Field = 188,
          Int32Field = 188,
          Int64Field = 188,
          FloatField = 188,
          DoubleField = 188,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity188",
          Text = "This is an instance of TestEntity188",
      };
      new TestEntity189 {
          BooleanField = true,
          Int16Field = 189,
          Int32Field = 189,
          Int64Field = 189,
          FloatField = 189,
          DoubleField = 189,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity189",
          Text = "This is an instance of TestEntity189",
      };
      new TestEntity190 {
          BooleanField = true,
          Int16Field = 190,
          Int32Field = 190,
          Int64Field = 190,
          FloatField = 190,
          DoubleField = 190,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity190",
          Text = "This is an instance of TestEntity190",
      };
      new TestEntity191 {
          BooleanField = true,
          Int16Field = 191,
          Int32Field = 191,
          Int64Field = 191,
          FloatField = 191,
          DoubleField = 191,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity191",
          Text = "This is an instance of TestEntity191",
      };
      new TestEntity192 {
          BooleanField = true,
          Int16Field = 192,
          Int32Field = 192,
          Int64Field = 192,
          FloatField = 192,
          DoubleField = 192,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity192",
          Text = "This is an instance of TestEntity192",
      };
      new TestEntity193 {
          BooleanField = true,
          Int16Field = 193,
          Int32Field = 193,
          Int64Field = 193,
          FloatField = 193,
          DoubleField = 193,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity193",
          Text = "This is an instance of TestEntity193",
      };
      new TestEntity194 {
          BooleanField = true,
          Int16Field = 194,
          Int32Field = 194,
          Int64Field = 194,
          FloatField = 194,
          DoubleField = 194,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity194",
          Text = "This is an instance of TestEntity194",
      };
      new TestEntity195 {
          BooleanField = true,
          Int16Field = 195,
          Int32Field = 195,
          Int64Field = 195,
          FloatField = 195,
          DoubleField = 195,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity195",
          Text = "This is an instance of TestEntity195",
      };
      new TestEntity196 {
          BooleanField = true,
          Int16Field = 196,
          Int32Field = 196,
          Int64Field = 196,
          FloatField = 196,
          DoubleField = 196,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity196",
          Text = "This is an instance of TestEntity196",
      };
      new TestEntity197 {
          BooleanField = true,
          Int16Field = 197,
          Int32Field = 197,
          Int64Field = 197,
          FloatField = 197,
          DoubleField = 197,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity197",
          Text = "This is an instance of TestEntity197",
      };
      new TestEntity198 {
          BooleanField = true,
          Int16Field = 198,
          Int32Field = 198,
          Int64Field = 198,
          FloatField = 198,
          DoubleField = 198,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity198",
          Text = "This is an instance of TestEntity198",
      };
      new TestEntity199 {
          BooleanField = true,
          Int16Field = 199,
          Int32Field = 199,
          Int64Field = 199,
          FloatField = 199,
          DoubleField = 199,
          DateTimeField= DateTime.Now.Date,
          StringField = "TestEntity199",
          Text = "This is an instance of TestEntity199",
      };
    }
  }

  public class ModelChecker
  {
    public void Run(Session session)
    {
      var result0 = session.Query.All<TestEntity0>().ToArray();
      Assert.That(result0.Length, Is.EqualTo(1));
      Assert.That(result0[0].BooleanField, Is.True);
      Assert.That(result0[0].Int16Field, Is.EqualTo(0));
      Assert.That(result0[0].Int32Field, Is.EqualTo(0));
      Assert.That(result0[0].Int64Field, Is.EqualTo(0));
      Assert.That(result0[0].FloatField, Is.EqualTo((float)0));
      Assert.That(result0[0].DoubleField, Is.EqualTo((double)0));
      Assert.That(result0[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result0[0].StringField, Is.EqualTo("TestEntity0"));
      Assert.That(result0[0].Text, Is.EqualTo("This is an instance of TestEntity0"));

      var result1 = session.Query.All<TestEntity1>().ToArray();
      Assert.That(result1.Length, Is.EqualTo(1));
      Assert.That(result1[0].BooleanField, Is.True);
      Assert.That(result1[0].Int16Field, Is.EqualTo(1));
      Assert.That(result1[0].Int32Field, Is.EqualTo(1));
      Assert.That(result1[0].Int64Field, Is.EqualTo(1));
      Assert.That(result1[0].FloatField, Is.EqualTo((float)1));
      Assert.That(result1[0].DoubleField, Is.EqualTo((double)1));
      Assert.That(result1[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result1[0].StringField, Is.EqualTo("TestEntity1"));
      Assert.That(result1[0].Text, Is.EqualTo("This is an instance of TestEntity1"));

      var result2 = session.Query.All<TestEntity2>().ToArray();
      Assert.That(result2.Length, Is.EqualTo(1));
      Assert.That(result2[0].BooleanField, Is.True);
      Assert.That(result2[0].Int16Field, Is.EqualTo(2));
      Assert.That(result2[0].Int32Field, Is.EqualTo(2));
      Assert.That(result2[0].Int64Field, Is.EqualTo(2));
      Assert.That(result2[0].FloatField, Is.EqualTo((float)2));
      Assert.That(result2[0].DoubleField, Is.EqualTo((double)2));
      Assert.That(result2[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result2[0].StringField, Is.EqualTo("TestEntity2"));
      Assert.That(result2[0].Text, Is.EqualTo("This is an instance of TestEntity2"));

      var result3 = session.Query.All<TestEntity3>().ToArray();
      Assert.That(result3.Length, Is.EqualTo(1));
      Assert.That(result3[0].BooleanField, Is.True);
      Assert.That(result3[0].Int16Field, Is.EqualTo(3));
      Assert.That(result3[0].Int32Field, Is.EqualTo(3));
      Assert.That(result3[0].Int64Field, Is.EqualTo(3));
      Assert.That(result3[0].FloatField, Is.EqualTo((float)3));
      Assert.That(result3[0].DoubleField, Is.EqualTo((double)3));
      Assert.That(result3[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result3[0].StringField, Is.EqualTo("TestEntity3"));
      Assert.That(result3[0].Text, Is.EqualTo("This is an instance of TestEntity3"));

      var result4 = session.Query.All<TestEntity4>().ToArray();
      Assert.That(result4.Length, Is.EqualTo(1));
      Assert.That(result4[0].BooleanField, Is.True);
      Assert.That(result4[0].Int16Field, Is.EqualTo(4));
      Assert.That(result4[0].Int32Field, Is.EqualTo(4));
      Assert.That(result4[0].Int64Field, Is.EqualTo(4));
      Assert.That(result4[0].FloatField, Is.EqualTo((float)4));
      Assert.That(result4[0].DoubleField, Is.EqualTo((double)4));
      Assert.That(result4[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result4[0].StringField, Is.EqualTo("TestEntity4"));
      Assert.That(result4[0].Text, Is.EqualTo("This is an instance of TestEntity4"));

      var result5 = session.Query.All<TestEntity5>().ToArray();
      Assert.That(result5.Length, Is.EqualTo(1));
      Assert.That(result5[0].BooleanField, Is.True);
      Assert.That(result5[0].Int16Field, Is.EqualTo(5));
      Assert.That(result5[0].Int32Field, Is.EqualTo(5));
      Assert.That(result5[0].Int64Field, Is.EqualTo(5));
      Assert.That(result5[0].FloatField, Is.EqualTo((float)5));
      Assert.That(result5[0].DoubleField, Is.EqualTo((double)5));
      Assert.That(result5[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result5[0].StringField, Is.EqualTo("TestEntity5"));
      Assert.That(result5[0].Text, Is.EqualTo("This is an instance of TestEntity5"));

      var result6 = session.Query.All<TestEntity6>().ToArray();
      Assert.That(result6.Length, Is.EqualTo(1));
      Assert.That(result6[0].BooleanField, Is.True);
      Assert.That(result6[0].Int16Field, Is.EqualTo(6));
      Assert.That(result6[0].Int32Field, Is.EqualTo(6));
      Assert.That(result6[0].Int64Field, Is.EqualTo(6));
      Assert.That(result6[0].FloatField, Is.EqualTo((float)6));
      Assert.That(result6[0].DoubleField, Is.EqualTo((double)6));
      Assert.That(result6[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result6[0].StringField, Is.EqualTo("TestEntity6"));
      Assert.That(result6[0].Text, Is.EqualTo("This is an instance of TestEntity6"));

      var result7 = session.Query.All<TestEntity7>().ToArray();
      Assert.That(result7.Length, Is.EqualTo(1));
      Assert.That(result7[0].BooleanField, Is.True);
      Assert.That(result7[0].Int16Field, Is.EqualTo(7));
      Assert.That(result7[0].Int32Field, Is.EqualTo(7));
      Assert.That(result7[0].Int64Field, Is.EqualTo(7));
      Assert.That(result7[0].FloatField, Is.EqualTo((float)7));
      Assert.That(result7[0].DoubleField, Is.EqualTo((double)7));
      Assert.That(result7[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result7[0].StringField, Is.EqualTo("TestEntity7"));
      Assert.That(result7[0].Text, Is.EqualTo("This is an instance of TestEntity7"));

      var result8 = session.Query.All<TestEntity8>().ToArray();
      Assert.That(result8.Length, Is.EqualTo(1));
      Assert.That(result8[0].BooleanField, Is.True);
      Assert.That(result8[0].Int16Field, Is.EqualTo(8));
      Assert.That(result8[0].Int32Field, Is.EqualTo(8));
      Assert.That(result8[0].Int64Field, Is.EqualTo(8));
      Assert.That(result8[0].FloatField, Is.EqualTo((float)8));
      Assert.That(result8[0].DoubleField, Is.EqualTo((double)8));
      Assert.That(result8[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result8[0].StringField, Is.EqualTo("TestEntity8"));
      Assert.That(result8[0].Text, Is.EqualTo("This is an instance of TestEntity8"));

      var result9 = session.Query.All<TestEntity9>().ToArray();
      Assert.That(result9.Length, Is.EqualTo(1));
      Assert.That(result9[0].BooleanField, Is.True);
      Assert.That(result9[0].Int16Field, Is.EqualTo(9));
      Assert.That(result9[0].Int32Field, Is.EqualTo(9));
      Assert.That(result9[0].Int64Field, Is.EqualTo(9));
      Assert.That(result9[0].FloatField, Is.EqualTo((float)9));
      Assert.That(result9[0].DoubleField, Is.EqualTo((double)9));
      Assert.That(result9[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result9[0].StringField, Is.EqualTo("TestEntity9"));
      Assert.That(result9[0].Text, Is.EqualTo("This is an instance of TestEntity9"));

      var result10 = session.Query.All<TestEntity10>().ToArray();
      Assert.That(result10.Length, Is.EqualTo(1));
      Assert.That(result10[0].BooleanField, Is.True);
      Assert.That(result10[0].Int16Field, Is.EqualTo(10));
      Assert.That(result10[0].Int32Field, Is.EqualTo(10));
      Assert.That(result10[0].Int64Field, Is.EqualTo(10));
      Assert.That(result10[0].FloatField, Is.EqualTo((float)10));
      Assert.That(result10[0].DoubleField, Is.EqualTo((double)10));
      Assert.That(result10[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result10[0].StringField, Is.EqualTo("TestEntity10"));
      Assert.That(result10[0].Text, Is.EqualTo("This is an instance of TestEntity10"));

      var result11 = session.Query.All<TestEntity11>().ToArray();
      Assert.That(result11.Length, Is.EqualTo(1));
      Assert.That(result11[0].BooleanField, Is.True);
      Assert.That(result11[0].Int16Field, Is.EqualTo(11));
      Assert.That(result11[0].Int32Field, Is.EqualTo(11));
      Assert.That(result11[0].Int64Field, Is.EqualTo(11));
      Assert.That(result11[0].FloatField, Is.EqualTo((float)11));
      Assert.That(result11[0].DoubleField, Is.EqualTo((double)11));
      Assert.That(result11[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result11[0].StringField, Is.EqualTo("TestEntity11"));
      Assert.That(result11[0].Text, Is.EqualTo("This is an instance of TestEntity11"));

      var result12 = session.Query.All<TestEntity12>().ToArray();
      Assert.That(result12.Length, Is.EqualTo(1));
      Assert.That(result12[0].BooleanField, Is.True);
      Assert.That(result12[0].Int16Field, Is.EqualTo(12));
      Assert.That(result12[0].Int32Field, Is.EqualTo(12));
      Assert.That(result12[0].Int64Field, Is.EqualTo(12));
      Assert.That(result12[0].FloatField, Is.EqualTo((float)12));
      Assert.That(result12[0].DoubleField, Is.EqualTo((double)12));
      Assert.That(result12[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result12[0].StringField, Is.EqualTo("TestEntity12"));
      Assert.That(result12[0].Text, Is.EqualTo("This is an instance of TestEntity12"));

      var result13 = session.Query.All<TestEntity13>().ToArray();
      Assert.That(result13.Length, Is.EqualTo(1));
      Assert.That(result13[0].BooleanField, Is.True);
      Assert.That(result13[0].Int16Field, Is.EqualTo(13));
      Assert.That(result13[0].Int32Field, Is.EqualTo(13));
      Assert.That(result13[0].Int64Field, Is.EqualTo(13));
      Assert.That(result13[0].FloatField, Is.EqualTo((float)13));
      Assert.That(result13[0].DoubleField, Is.EqualTo((double)13));
      Assert.That(result13[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result13[0].StringField, Is.EqualTo("TestEntity13"));
      Assert.That(result13[0].Text, Is.EqualTo("This is an instance of TestEntity13"));

      var result14 = session.Query.All<TestEntity14>().ToArray();
      Assert.That(result14.Length, Is.EqualTo(1));
      Assert.That(result14[0].BooleanField, Is.True);
      Assert.That(result14[0].Int16Field, Is.EqualTo(14));
      Assert.That(result14[0].Int32Field, Is.EqualTo(14));
      Assert.That(result14[0].Int64Field, Is.EqualTo(14));
      Assert.That(result14[0].FloatField, Is.EqualTo((float)14));
      Assert.That(result14[0].DoubleField, Is.EqualTo((double)14));
      Assert.That(result14[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result14[0].StringField, Is.EqualTo("TestEntity14"));
      Assert.That(result14[0].Text, Is.EqualTo("This is an instance of TestEntity14"));

      var result15 = session.Query.All<TestEntity15>().ToArray();
      Assert.That(result15.Length, Is.EqualTo(1));
      Assert.That(result15[0].BooleanField, Is.True);
      Assert.That(result15[0].Int16Field, Is.EqualTo(15));
      Assert.That(result15[0].Int32Field, Is.EqualTo(15));
      Assert.That(result15[0].Int64Field, Is.EqualTo(15));
      Assert.That(result15[0].FloatField, Is.EqualTo((float)15));
      Assert.That(result15[0].DoubleField, Is.EqualTo((double)15));
      Assert.That(result15[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result15[0].StringField, Is.EqualTo("TestEntity15"));
      Assert.That(result15[0].Text, Is.EqualTo("This is an instance of TestEntity15"));

      var result16 = session.Query.All<TestEntity16>().ToArray();
      Assert.That(result16.Length, Is.EqualTo(1));
      Assert.That(result16[0].BooleanField, Is.True);
      Assert.That(result16[0].Int16Field, Is.EqualTo(16));
      Assert.That(result16[0].Int32Field, Is.EqualTo(16));
      Assert.That(result16[0].Int64Field, Is.EqualTo(16));
      Assert.That(result16[0].FloatField, Is.EqualTo((float)16));
      Assert.That(result16[0].DoubleField, Is.EqualTo((double)16));
      Assert.That(result16[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result16[0].StringField, Is.EqualTo("TestEntity16"));
      Assert.That(result16[0].Text, Is.EqualTo("This is an instance of TestEntity16"));

      var result17 = session.Query.All<TestEntity17>().ToArray();
      Assert.That(result17.Length, Is.EqualTo(1));
      Assert.That(result17[0].BooleanField, Is.True);
      Assert.That(result17[0].Int16Field, Is.EqualTo(17));
      Assert.That(result17[0].Int32Field, Is.EqualTo(17));
      Assert.That(result17[0].Int64Field, Is.EqualTo(17));
      Assert.That(result17[0].FloatField, Is.EqualTo((float)17));
      Assert.That(result17[0].DoubleField, Is.EqualTo((double)17));
      Assert.That(result17[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result17[0].StringField, Is.EqualTo("TestEntity17"));
      Assert.That(result17[0].Text, Is.EqualTo("This is an instance of TestEntity17"));

      var result18 = session.Query.All<TestEntity18>().ToArray();
      Assert.That(result18.Length, Is.EqualTo(1));
      Assert.That(result18[0].BooleanField, Is.True);
      Assert.That(result18[0].Int16Field, Is.EqualTo(18));
      Assert.That(result18[0].Int32Field, Is.EqualTo(18));
      Assert.That(result18[0].Int64Field, Is.EqualTo(18));
      Assert.That(result18[0].FloatField, Is.EqualTo((float)18));
      Assert.That(result18[0].DoubleField, Is.EqualTo((double)18));
      Assert.That(result18[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result18[0].StringField, Is.EqualTo("TestEntity18"));
      Assert.That(result18[0].Text, Is.EqualTo("This is an instance of TestEntity18"));

      var result19 = session.Query.All<TestEntity19>().ToArray();
      Assert.That(result19.Length, Is.EqualTo(1));
      Assert.That(result19[0].BooleanField, Is.True);
      Assert.That(result19[0].Int16Field, Is.EqualTo(19));
      Assert.That(result19[0].Int32Field, Is.EqualTo(19));
      Assert.That(result19[0].Int64Field, Is.EqualTo(19));
      Assert.That(result19[0].FloatField, Is.EqualTo((float)19));
      Assert.That(result19[0].DoubleField, Is.EqualTo((double)19));
      Assert.That(result19[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result19[0].StringField, Is.EqualTo("TestEntity19"));
      Assert.That(result19[0].Text, Is.EqualTo("This is an instance of TestEntity19"));

      var result20 = session.Query.All<TestEntity20>().ToArray();
      Assert.That(result20.Length, Is.EqualTo(1));
      Assert.That(result20[0].BooleanField, Is.True);
      Assert.That(result20[0].Int16Field, Is.EqualTo(20));
      Assert.That(result20[0].Int32Field, Is.EqualTo(20));
      Assert.That(result20[0].Int64Field, Is.EqualTo(20));
      Assert.That(result20[0].FloatField, Is.EqualTo((float)20));
      Assert.That(result20[0].DoubleField, Is.EqualTo((double)20));
      Assert.That(result20[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result20[0].StringField, Is.EqualTo("TestEntity20"));
      Assert.That(result20[0].Text, Is.EqualTo("This is an instance of TestEntity20"));

      var result21 = session.Query.All<TestEntity21>().ToArray();
      Assert.That(result21.Length, Is.EqualTo(1));
      Assert.That(result21[0].BooleanField, Is.True);
      Assert.That(result21[0].Int16Field, Is.EqualTo(21));
      Assert.That(result21[0].Int32Field, Is.EqualTo(21));
      Assert.That(result21[0].Int64Field, Is.EqualTo(21));
      Assert.That(result21[0].FloatField, Is.EqualTo((float)21));
      Assert.That(result21[0].DoubleField, Is.EqualTo((double)21));
      Assert.That(result21[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result21[0].StringField, Is.EqualTo("TestEntity21"));
      Assert.That(result21[0].Text, Is.EqualTo("This is an instance of TestEntity21"));

      var result22 = session.Query.All<TestEntity22>().ToArray();
      Assert.That(result22.Length, Is.EqualTo(1));
      Assert.That(result22[0].BooleanField, Is.True);
      Assert.That(result22[0].Int16Field, Is.EqualTo(22));
      Assert.That(result22[0].Int32Field, Is.EqualTo(22));
      Assert.That(result22[0].Int64Field, Is.EqualTo(22));
      Assert.That(result22[0].FloatField, Is.EqualTo((float)22));
      Assert.That(result22[0].DoubleField, Is.EqualTo((double)22));
      Assert.That(result22[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result22[0].StringField, Is.EqualTo("TestEntity22"));
      Assert.That(result22[0].Text, Is.EqualTo("This is an instance of TestEntity22"));

      var result23 = session.Query.All<TestEntity23>().ToArray();
      Assert.That(result23.Length, Is.EqualTo(1));
      Assert.That(result23[0].BooleanField, Is.True);
      Assert.That(result23[0].Int16Field, Is.EqualTo(23));
      Assert.That(result23[0].Int32Field, Is.EqualTo(23));
      Assert.That(result23[0].Int64Field, Is.EqualTo(23));
      Assert.That(result23[0].FloatField, Is.EqualTo((float)23));
      Assert.That(result23[0].DoubleField, Is.EqualTo((double)23));
      Assert.That(result23[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result23[0].StringField, Is.EqualTo("TestEntity23"));
      Assert.That(result23[0].Text, Is.EqualTo("This is an instance of TestEntity23"));

      var result24 = session.Query.All<TestEntity24>().ToArray();
      Assert.That(result24.Length, Is.EqualTo(1));
      Assert.That(result24[0].BooleanField, Is.True);
      Assert.That(result24[0].Int16Field, Is.EqualTo(24));
      Assert.That(result24[0].Int32Field, Is.EqualTo(24));
      Assert.That(result24[0].Int64Field, Is.EqualTo(24));
      Assert.That(result24[0].FloatField, Is.EqualTo((float)24));
      Assert.That(result24[0].DoubleField, Is.EqualTo((double)24));
      Assert.That(result24[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result24[0].StringField, Is.EqualTo("TestEntity24"));
      Assert.That(result24[0].Text, Is.EqualTo("This is an instance of TestEntity24"));

      var result25 = session.Query.All<TestEntity25>().ToArray();
      Assert.That(result25.Length, Is.EqualTo(1));
      Assert.That(result25[0].BooleanField, Is.True);
      Assert.That(result25[0].Int16Field, Is.EqualTo(25));
      Assert.That(result25[0].Int32Field, Is.EqualTo(25));
      Assert.That(result25[0].Int64Field, Is.EqualTo(25));
      Assert.That(result25[0].FloatField, Is.EqualTo((float)25));
      Assert.That(result25[0].DoubleField, Is.EqualTo((double)25));
      Assert.That(result25[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result25[0].StringField, Is.EqualTo("TestEntity25"));
      Assert.That(result25[0].Text, Is.EqualTo("This is an instance of TestEntity25"));

      var result26 = session.Query.All<TestEntity26>().ToArray();
      Assert.That(result26.Length, Is.EqualTo(1));
      Assert.That(result26[0].BooleanField, Is.True);
      Assert.That(result26[0].Int16Field, Is.EqualTo(26));
      Assert.That(result26[0].Int32Field, Is.EqualTo(26));
      Assert.That(result26[0].Int64Field, Is.EqualTo(26));
      Assert.That(result26[0].FloatField, Is.EqualTo((float)26));
      Assert.That(result26[0].DoubleField, Is.EqualTo((double)26));
      Assert.That(result26[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result26[0].StringField, Is.EqualTo("TestEntity26"));
      Assert.That(result26[0].Text, Is.EqualTo("This is an instance of TestEntity26"));

      var result27 = session.Query.All<TestEntity27>().ToArray();
      Assert.That(result27.Length, Is.EqualTo(1));
      Assert.That(result27[0].BooleanField, Is.True);
      Assert.That(result27[0].Int16Field, Is.EqualTo(27));
      Assert.That(result27[0].Int32Field, Is.EqualTo(27));
      Assert.That(result27[0].Int64Field, Is.EqualTo(27));
      Assert.That(result27[0].FloatField, Is.EqualTo((float)27));
      Assert.That(result27[0].DoubleField, Is.EqualTo((double)27));
      Assert.That(result27[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result27[0].StringField, Is.EqualTo("TestEntity27"));
      Assert.That(result27[0].Text, Is.EqualTo("This is an instance of TestEntity27"));

      var result28 = session.Query.All<TestEntity28>().ToArray();
      Assert.That(result28.Length, Is.EqualTo(1));
      Assert.That(result28[0].BooleanField, Is.True);
      Assert.That(result28[0].Int16Field, Is.EqualTo(28));
      Assert.That(result28[0].Int32Field, Is.EqualTo(28));
      Assert.That(result28[0].Int64Field, Is.EqualTo(28));
      Assert.That(result28[0].FloatField, Is.EqualTo((float)28));
      Assert.That(result28[0].DoubleField, Is.EqualTo((double)28));
      Assert.That(result28[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result28[0].StringField, Is.EqualTo("TestEntity28"));
      Assert.That(result28[0].Text, Is.EqualTo("This is an instance of TestEntity28"));

      var result29 = session.Query.All<TestEntity29>().ToArray();
      Assert.That(result29.Length, Is.EqualTo(1));
      Assert.That(result29[0].BooleanField, Is.True);
      Assert.That(result29[0].Int16Field, Is.EqualTo(29));
      Assert.That(result29[0].Int32Field, Is.EqualTo(29));
      Assert.That(result29[0].Int64Field, Is.EqualTo(29));
      Assert.That(result29[0].FloatField, Is.EqualTo((float)29));
      Assert.That(result29[0].DoubleField, Is.EqualTo((double)29));
      Assert.That(result29[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result29[0].StringField, Is.EqualTo("TestEntity29"));
      Assert.That(result29[0].Text, Is.EqualTo("This is an instance of TestEntity29"));

      var result30 = session.Query.All<TestEntity30>().ToArray();
      Assert.That(result30.Length, Is.EqualTo(1));
      Assert.That(result30[0].BooleanField, Is.True);
      Assert.That(result30[0].Int16Field, Is.EqualTo(30));
      Assert.That(result30[0].Int32Field, Is.EqualTo(30));
      Assert.That(result30[0].Int64Field, Is.EqualTo(30));
      Assert.That(result30[0].FloatField, Is.EqualTo((float)30));
      Assert.That(result30[0].DoubleField, Is.EqualTo((double)30));
      Assert.That(result30[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result30[0].StringField, Is.EqualTo("TestEntity30"));
      Assert.That(result30[0].Text, Is.EqualTo("This is an instance of TestEntity30"));

      var result31 = session.Query.All<TestEntity31>().ToArray();
      Assert.That(result31.Length, Is.EqualTo(1));
      Assert.That(result31[0].BooleanField, Is.True);
      Assert.That(result31[0].Int16Field, Is.EqualTo(31));
      Assert.That(result31[0].Int32Field, Is.EqualTo(31));
      Assert.That(result31[0].Int64Field, Is.EqualTo(31));
      Assert.That(result31[0].FloatField, Is.EqualTo((float)31));
      Assert.That(result31[0].DoubleField, Is.EqualTo((double)31));
      Assert.That(result31[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result31[0].StringField, Is.EqualTo("TestEntity31"));
      Assert.That(result31[0].Text, Is.EqualTo("This is an instance of TestEntity31"));

      var result32 = session.Query.All<TestEntity32>().ToArray();
      Assert.That(result32.Length, Is.EqualTo(1));
      Assert.That(result32[0].BooleanField, Is.True);
      Assert.That(result32[0].Int16Field, Is.EqualTo(32));
      Assert.That(result32[0].Int32Field, Is.EqualTo(32));
      Assert.That(result32[0].Int64Field, Is.EqualTo(32));
      Assert.That(result32[0].FloatField, Is.EqualTo((float)32));
      Assert.That(result32[0].DoubleField, Is.EqualTo((double)32));
      Assert.That(result32[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result32[0].StringField, Is.EqualTo("TestEntity32"));
      Assert.That(result32[0].Text, Is.EqualTo("This is an instance of TestEntity32"));

      var result33 = session.Query.All<TestEntity33>().ToArray();
      Assert.That(result33.Length, Is.EqualTo(1));
      Assert.That(result33[0].BooleanField, Is.True);
      Assert.That(result33[0].Int16Field, Is.EqualTo(33));
      Assert.That(result33[0].Int32Field, Is.EqualTo(33));
      Assert.That(result33[0].Int64Field, Is.EqualTo(33));
      Assert.That(result33[0].FloatField, Is.EqualTo((float)33));
      Assert.That(result33[0].DoubleField, Is.EqualTo((double)33));
      Assert.That(result33[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result33[0].StringField, Is.EqualTo("TestEntity33"));
      Assert.That(result33[0].Text, Is.EqualTo("This is an instance of TestEntity33"));

      var result34 = session.Query.All<TestEntity34>().ToArray();
      Assert.That(result34.Length, Is.EqualTo(1));
      Assert.That(result34[0].BooleanField, Is.True);
      Assert.That(result34[0].Int16Field, Is.EqualTo(34));
      Assert.That(result34[0].Int32Field, Is.EqualTo(34));
      Assert.That(result34[0].Int64Field, Is.EqualTo(34));
      Assert.That(result34[0].FloatField, Is.EqualTo((float)34));
      Assert.That(result34[0].DoubleField, Is.EqualTo((double)34));
      Assert.That(result34[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result34[0].StringField, Is.EqualTo("TestEntity34"));
      Assert.That(result34[0].Text, Is.EqualTo("This is an instance of TestEntity34"));

      var result35 = session.Query.All<TestEntity35>().ToArray();
      Assert.That(result35.Length, Is.EqualTo(1));
      Assert.That(result35[0].BooleanField, Is.True);
      Assert.That(result35[0].Int16Field, Is.EqualTo(35));
      Assert.That(result35[0].Int32Field, Is.EqualTo(35));
      Assert.That(result35[0].Int64Field, Is.EqualTo(35));
      Assert.That(result35[0].FloatField, Is.EqualTo((float)35));
      Assert.That(result35[0].DoubleField, Is.EqualTo((double)35));
      Assert.That(result35[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result35[0].StringField, Is.EqualTo("TestEntity35"));
      Assert.That(result35[0].Text, Is.EqualTo("This is an instance of TestEntity35"));

      var result36 = session.Query.All<TestEntity36>().ToArray();
      Assert.That(result36.Length, Is.EqualTo(1));
      Assert.That(result36[0].BooleanField, Is.True);
      Assert.That(result36[0].Int16Field, Is.EqualTo(36));
      Assert.That(result36[0].Int32Field, Is.EqualTo(36));
      Assert.That(result36[0].Int64Field, Is.EqualTo(36));
      Assert.That(result36[0].FloatField, Is.EqualTo((float)36));
      Assert.That(result36[0].DoubleField, Is.EqualTo((double)36));
      Assert.That(result36[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result36[0].StringField, Is.EqualTo("TestEntity36"));
      Assert.That(result36[0].Text, Is.EqualTo("This is an instance of TestEntity36"));

      var result37 = session.Query.All<TestEntity37>().ToArray();
      Assert.That(result37.Length, Is.EqualTo(1));
      Assert.That(result37[0].BooleanField, Is.True);
      Assert.That(result37[0].Int16Field, Is.EqualTo(37));
      Assert.That(result37[0].Int32Field, Is.EqualTo(37));
      Assert.That(result37[0].Int64Field, Is.EqualTo(37));
      Assert.That(result37[0].FloatField, Is.EqualTo((float)37));
      Assert.That(result37[0].DoubleField, Is.EqualTo((double)37));
      Assert.That(result37[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result37[0].StringField, Is.EqualTo("TestEntity37"));
      Assert.That(result37[0].Text, Is.EqualTo("This is an instance of TestEntity37"));

      var result38 = session.Query.All<TestEntity38>().ToArray();
      Assert.That(result38.Length, Is.EqualTo(1));
      Assert.That(result38[0].BooleanField, Is.True);
      Assert.That(result38[0].Int16Field, Is.EqualTo(38));
      Assert.That(result38[0].Int32Field, Is.EqualTo(38));
      Assert.That(result38[0].Int64Field, Is.EqualTo(38));
      Assert.That(result38[0].FloatField, Is.EqualTo((float)38));
      Assert.That(result38[0].DoubleField, Is.EqualTo((double)38));
      Assert.That(result38[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result38[0].StringField, Is.EqualTo("TestEntity38"));
      Assert.That(result38[0].Text, Is.EqualTo("This is an instance of TestEntity38"));

      var result39 = session.Query.All<TestEntity39>().ToArray();
      Assert.That(result39.Length, Is.EqualTo(1));
      Assert.That(result39[0].BooleanField, Is.True);
      Assert.That(result39[0].Int16Field, Is.EqualTo(39));
      Assert.That(result39[0].Int32Field, Is.EqualTo(39));
      Assert.That(result39[0].Int64Field, Is.EqualTo(39));
      Assert.That(result39[0].FloatField, Is.EqualTo((float)39));
      Assert.That(result39[0].DoubleField, Is.EqualTo((double)39));
      Assert.That(result39[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result39[0].StringField, Is.EqualTo("TestEntity39"));
      Assert.That(result39[0].Text, Is.EqualTo("This is an instance of TestEntity39"));

      var result40 = session.Query.All<TestEntity40>().ToArray();
      Assert.That(result40.Length, Is.EqualTo(1));
      Assert.That(result40[0].BooleanField, Is.True);
      Assert.That(result40[0].Int16Field, Is.EqualTo(40));
      Assert.That(result40[0].Int32Field, Is.EqualTo(40));
      Assert.That(result40[0].Int64Field, Is.EqualTo(40));
      Assert.That(result40[0].FloatField, Is.EqualTo((float)40));
      Assert.That(result40[0].DoubleField, Is.EqualTo((double)40));
      Assert.That(result40[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result40[0].StringField, Is.EqualTo("TestEntity40"));
      Assert.That(result40[0].Text, Is.EqualTo("This is an instance of TestEntity40"));

      var result41 = session.Query.All<TestEntity41>().ToArray();
      Assert.That(result41.Length, Is.EqualTo(1));
      Assert.That(result41[0].BooleanField, Is.True);
      Assert.That(result41[0].Int16Field, Is.EqualTo(41));
      Assert.That(result41[0].Int32Field, Is.EqualTo(41));
      Assert.That(result41[0].Int64Field, Is.EqualTo(41));
      Assert.That(result41[0].FloatField, Is.EqualTo((float)41));
      Assert.That(result41[0].DoubleField, Is.EqualTo((double)41));
      Assert.That(result41[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result41[0].StringField, Is.EqualTo("TestEntity41"));
      Assert.That(result41[0].Text, Is.EqualTo("This is an instance of TestEntity41"));

      var result42 = session.Query.All<TestEntity42>().ToArray();
      Assert.That(result42.Length, Is.EqualTo(1));
      Assert.That(result42[0].BooleanField, Is.True);
      Assert.That(result42[0].Int16Field, Is.EqualTo(42));
      Assert.That(result42[0].Int32Field, Is.EqualTo(42));
      Assert.That(result42[0].Int64Field, Is.EqualTo(42));
      Assert.That(result42[0].FloatField, Is.EqualTo((float)42));
      Assert.That(result42[0].DoubleField, Is.EqualTo((double)42));
      Assert.That(result42[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result42[0].StringField, Is.EqualTo("TestEntity42"));
      Assert.That(result42[0].Text, Is.EqualTo("This is an instance of TestEntity42"));

      var result43 = session.Query.All<TestEntity43>().ToArray();
      Assert.That(result43.Length, Is.EqualTo(1));
      Assert.That(result43[0].BooleanField, Is.True);
      Assert.That(result43[0].Int16Field, Is.EqualTo(43));
      Assert.That(result43[0].Int32Field, Is.EqualTo(43));
      Assert.That(result43[0].Int64Field, Is.EqualTo(43));
      Assert.That(result43[0].FloatField, Is.EqualTo((float)43));
      Assert.That(result43[0].DoubleField, Is.EqualTo((double)43));
      Assert.That(result43[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result43[0].StringField, Is.EqualTo("TestEntity43"));
      Assert.That(result43[0].Text, Is.EqualTo("This is an instance of TestEntity43"));

      var result44 = session.Query.All<TestEntity44>().ToArray();
      Assert.That(result44.Length, Is.EqualTo(1));
      Assert.That(result44[0].BooleanField, Is.True);
      Assert.That(result44[0].Int16Field, Is.EqualTo(44));
      Assert.That(result44[0].Int32Field, Is.EqualTo(44));
      Assert.That(result44[0].Int64Field, Is.EqualTo(44));
      Assert.That(result44[0].FloatField, Is.EqualTo((float)44));
      Assert.That(result44[0].DoubleField, Is.EqualTo((double)44));
      Assert.That(result44[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result44[0].StringField, Is.EqualTo("TestEntity44"));
      Assert.That(result44[0].Text, Is.EqualTo("This is an instance of TestEntity44"));

      var result45 = session.Query.All<TestEntity45>().ToArray();
      Assert.That(result45.Length, Is.EqualTo(1));
      Assert.That(result45[0].BooleanField, Is.True);
      Assert.That(result45[0].Int16Field, Is.EqualTo(45));
      Assert.That(result45[0].Int32Field, Is.EqualTo(45));
      Assert.That(result45[0].Int64Field, Is.EqualTo(45));
      Assert.That(result45[0].FloatField, Is.EqualTo((float)45));
      Assert.That(result45[0].DoubleField, Is.EqualTo((double)45));
      Assert.That(result45[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result45[0].StringField, Is.EqualTo("TestEntity45"));
      Assert.That(result45[0].Text, Is.EqualTo("This is an instance of TestEntity45"));

      var result46 = session.Query.All<TestEntity46>().ToArray();
      Assert.That(result46.Length, Is.EqualTo(1));
      Assert.That(result46[0].BooleanField, Is.True);
      Assert.That(result46[0].Int16Field, Is.EqualTo(46));
      Assert.That(result46[0].Int32Field, Is.EqualTo(46));
      Assert.That(result46[0].Int64Field, Is.EqualTo(46));
      Assert.That(result46[0].FloatField, Is.EqualTo((float)46));
      Assert.That(result46[0].DoubleField, Is.EqualTo((double)46));
      Assert.That(result46[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result46[0].StringField, Is.EqualTo("TestEntity46"));
      Assert.That(result46[0].Text, Is.EqualTo("This is an instance of TestEntity46"));

      var result47 = session.Query.All<TestEntity47>().ToArray();
      Assert.That(result47.Length, Is.EqualTo(1));
      Assert.That(result47[0].BooleanField, Is.True);
      Assert.That(result47[0].Int16Field, Is.EqualTo(47));
      Assert.That(result47[0].Int32Field, Is.EqualTo(47));
      Assert.That(result47[0].Int64Field, Is.EqualTo(47));
      Assert.That(result47[0].FloatField, Is.EqualTo((float)47));
      Assert.That(result47[0].DoubleField, Is.EqualTo((double)47));
      Assert.That(result47[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result47[0].StringField, Is.EqualTo("TestEntity47"));
      Assert.That(result47[0].Text, Is.EqualTo("This is an instance of TestEntity47"));

      var result48 = session.Query.All<TestEntity48>().ToArray();
      Assert.That(result48.Length, Is.EqualTo(1));
      Assert.That(result48[0].BooleanField, Is.True);
      Assert.That(result48[0].Int16Field, Is.EqualTo(48));
      Assert.That(result48[0].Int32Field, Is.EqualTo(48));
      Assert.That(result48[0].Int64Field, Is.EqualTo(48));
      Assert.That(result48[0].FloatField, Is.EqualTo((float)48));
      Assert.That(result48[0].DoubleField, Is.EqualTo((double)48));
      Assert.That(result48[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result48[0].StringField, Is.EqualTo("TestEntity48"));
      Assert.That(result48[0].Text, Is.EqualTo("This is an instance of TestEntity48"));

      var result49 = session.Query.All<TestEntity49>().ToArray();
      Assert.That(result49.Length, Is.EqualTo(1));
      Assert.That(result49[0].BooleanField, Is.True);
      Assert.That(result49[0].Int16Field, Is.EqualTo(49));
      Assert.That(result49[0].Int32Field, Is.EqualTo(49));
      Assert.That(result49[0].Int64Field, Is.EqualTo(49));
      Assert.That(result49[0].FloatField, Is.EqualTo((float)49));
      Assert.That(result49[0].DoubleField, Is.EqualTo((double)49));
      Assert.That(result49[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result49[0].StringField, Is.EqualTo("TestEntity49"));
      Assert.That(result49[0].Text, Is.EqualTo("This is an instance of TestEntity49"));

      var result50 = session.Query.All<TestEntity50>().ToArray();
      Assert.That(result50.Length, Is.EqualTo(1));
      Assert.That(result50[0].BooleanField, Is.True);
      Assert.That(result50[0].Int16Field, Is.EqualTo(50));
      Assert.That(result50[0].Int32Field, Is.EqualTo(50));
      Assert.That(result50[0].Int64Field, Is.EqualTo(50));
      Assert.That(result50[0].FloatField, Is.EqualTo((float)50));
      Assert.That(result50[0].DoubleField, Is.EqualTo((double)50));
      Assert.That(result50[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result50[0].StringField, Is.EqualTo("TestEntity50"));
      Assert.That(result50[0].Text, Is.EqualTo("This is an instance of TestEntity50"));

      var result51 = session.Query.All<TestEntity51>().ToArray();
      Assert.That(result51.Length, Is.EqualTo(1));
      Assert.That(result51[0].BooleanField, Is.True);
      Assert.That(result51[0].Int16Field, Is.EqualTo(51));
      Assert.That(result51[0].Int32Field, Is.EqualTo(51));
      Assert.That(result51[0].Int64Field, Is.EqualTo(51));
      Assert.That(result51[0].FloatField, Is.EqualTo((float)51));
      Assert.That(result51[0].DoubleField, Is.EqualTo((double)51));
      Assert.That(result51[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result51[0].StringField, Is.EqualTo("TestEntity51"));
      Assert.That(result51[0].Text, Is.EqualTo("This is an instance of TestEntity51"));

      var result52 = session.Query.All<TestEntity52>().ToArray();
      Assert.That(result52.Length, Is.EqualTo(1));
      Assert.That(result52[0].BooleanField, Is.True);
      Assert.That(result52[0].Int16Field, Is.EqualTo(52));
      Assert.That(result52[0].Int32Field, Is.EqualTo(52));
      Assert.That(result52[0].Int64Field, Is.EqualTo(52));
      Assert.That(result52[0].FloatField, Is.EqualTo((float)52));
      Assert.That(result52[0].DoubleField, Is.EqualTo((double)52));
      Assert.That(result52[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result52[0].StringField, Is.EqualTo("TestEntity52"));
      Assert.That(result52[0].Text, Is.EqualTo("This is an instance of TestEntity52"));

      var result53 = session.Query.All<TestEntity53>().ToArray();
      Assert.That(result53.Length, Is.EqualTo(1));
      Assert.That(result53[0].BooleanField, Is.True);
      Assert.That(result53[0].Int16Field, Is.EqualTo(53));
      Assert.That(result53[0].Int32Field, Is.EqualTo(53));
      Assert.That(result53[0].Int64Field, Is.EqualTo(53));
      Assert.That(result53[0].FloatField, Is.EqualTo((float)53));
      Assert.That(result53[0].DoubleField, Is.EqualTo((double)53));
      Assert.That(result53[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result53[0].StringField, Is.EqualTo("TestEntity53"));
      Assert.That(result53[0].Text, Is.EqualTo("This is an instance of TestEntity53"));

      var result54 = session.Query.All<TestEntity54>().ToArray();
      Assert.That(result54.Length, Is.EqualTo(1));
      Assert.That(result54[0].BooleanField, Is.True);
      Assert.That(result54[0].Int16Field, Is.EqualTo(54));
      Assert.That(result54[0].Int32Field, Is.EqualTo(54));
      Assert.That(result54[0].Int64Field, Is.EqualTo(54));
      Assert.That(result54[0].FloatField, Is.EqualTo((float)54));
      Assert.That(result54[0].DoubleField, Is.EqualTo((double)54));
      Assert.That(result54[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result54[0].StringField, Is.EqualTo("TestEntity54"));
      Assert.That(result54[0].Text, Is.EqualTo("This is an instance of TestEntity54"));

      var result55 = session.Query.All<TestEntity55>().ToArray();
      Assert.That(result55.Length, Is.EqualTo(1));
      Assert.That(result55[0].BooleanField, Is.True);
      Assert.That(result55[0].Int16Field, Is.EqualTo(55));
      Assert.That(result55[0].Int32Field, Is.EqualTo(55));
      Assert.That(result55[0].Int64Field, Is.EqualTo(55));
      Assert.That(result55[0].FloatField, Is.EqualTo((float)55));
      Assert.That(result55[0].DoubleField, Is.EqualTo((double)55));
      Assert.That(result55[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result55[0].StringField, Is.EqualTo("TestEntity55"));
      Assert.That(result55[0].Text, Is.EqualTo("This is an instance of TestEntity55"));

      var result56 = session.Query.All<TestEntity56>().ToArray();
      Assert.That(result56.Length, Is.EqualTo(1));
      Assert.That(result56[0].BooleanField, Is.True);
      Assert.That(result56[0].Int16Field, Is.EqualTo(56));
      Assert.That(result56[0].Int32Field, Is.EqualTo(56));
      Assert.That(result56[0].Int64Field, Is.EqualTo(56));
      Assert.That(result56[0].FloatField, Is.EqualTo((float)56));
      Assert.That(result56[0].DoubleField, Is.EqualTo((double)56));
      Assert.That(result56[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result56[0].StringField, Is.EqualTo("TestEntity56"));
      Assert.That(result56[0].Text, Is.EqualTo("This is an instance of TestEntity56"));

      var result57 = session.Query.All<TestEntity57>().ToArray();
      Assert.That(result57.Length, Is.EqualTo(1));
      Assert.That(result57[0].BooleanField, Is.True);
      Assert.That(result57[0].Int16Field, Is.EqualTo(57));
      Assert.That(result57[0].Int32Field, Is.EqualTo(57));
      Assert.That(result57[0].Int64Field, Is.EqualTo(57));
      Assert.That(result57[0].FloatField, Is.EqualTo((float)57));
      Assert.That(result57[0].DoubleField, Is.EqualTo((double)57));
      Assert.That(result57[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result57[0].StringField, Is.EqualTo("TestEntity57"));
      Assert.That(result57[0].Text, Is.EqualTo("This is an instance of TestEntity57"));

      var result58 = session.Query.All<TestEntity58>().ToArray();
      Assert.That(result58.Length, Is.EqualTo(1));
      Assert.That(result58[0].BooleanField, Is.True);
      Assert.That(result58[0].Int16Field, Is.EqualTo(58));
      Assert.That(result58[0].Int32Field, Is.EqualTo(58));
      Assert.That(result58[0].Int64Field, Is.EqualTo(58));
      Assert.That(result58[0].FloatField, Is.EqualTo((float)58));
      Assert.That(result58[0].DoubleField, Is.EqualTo((double)58));
      Assert.That(result58[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result58[0].StringField, Is.EqualTo("TestEntity58"));
      Assert.That(result58[0].Text, Is.EqualTo("This is an instance of TestEntity58"));

      var result59 = session.Query.All<TestEntity59>().ToArray();
      Assert.That(result59.Length, Is.EqualTo(1));
      Assert.That(result59[0].BooleanField, Is.True);
      Assert.That(result59[0].Int16Field, Is.EqualTo(59));
      Assert.That(result59[0].Int32Field, Is.EqualTo(59));
      Assert.That(result59[0].Int64Field, Is.EqualTo(59));
      Assert.That(result59[0].FloatField, Is.EqualTo((float)59));
      Assert.That(result59[0].DoubleField, Is.EqualTo((double)59));
      Assert.That(result59[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result59[0].StringField, Is.EqualTo("TestEntity59"));
      Assert.That(result59[0].Text, Is.EqualTo("This is an instance of TestEntity59"));

      var result60 = session.Query.All<TestEntity60>().ToArray();
      Assert.That(result60.Length, Is.EqualTo(1));
      Assert.That(result60[0].BooleanField, Is.True);
      Assert.That(result60[0].Int16Field, Is.EqualTo(60));
      Assert.That(result60[0].Int32Field, Is.EqualTo(60));
      Assert.That(result60[0].Int64Field, Is.EqualTo(60));
      Assert.That(result60[0].FloatField, Is.EqualTo((float)60));
      Assert.That(result60[0].DoubleField, Is.EqualTo((double)60));
      Assert.That(result60[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result60[0].StringField, Is.EqualTo("TestEntity60"));
      Assert.That(result60[0].Text, Is.EqualTo("This is an instance of TestEntity60"));

      var result61 = session.Query.All<TestEntity61>().ToArray();
      Assert.That(result61.Length, Is.EqualTo(1));
      Assert.That(result61[0].BooleanField, Is.True);
      Assert.That(result61[0].Int16Field, Is.EqualTo(61));
      Assert.That(result61[0].Int32Field, Is.EqualTo(61));
      Assert.That(result61[0].Int64Field, Is.EqualTo(61));
      Assert.That(result61[0].FloatField, Is.EqualTo((float)61));
      Assert.That(result61[0].DoubleField, Is.EqualTo((double)61));
      Assert.That(result61[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result61[0].StringField, Is.EqualTo("TestEntity61"));
      Assert.That(result61[0].Text, Is.EqualTo("This is an instance of TestEntity61"));

      var result62 = session.Query.All<TestEntity62>().ToArray();
      Assert.That(result62.Length, Is.EqualTo(1));
      Assert.That(result62[0].BooleanField, Is.True);
      Assert.That(result62[0].Int16Field, Is.EqualTo(62));
      Assert.That(result62[0].Int32Field, Is.EqualTo(62));
      Assert.That(result62[0].Int64Field, Is.EqualTo(62));
      Assert.That(result62[0].FloatField, Is.EqualTo((float)62));
      Assert.That(result62[0].DoubleField, Is.EqualTo((double)62));
      Assert.That(result62[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result62[0].StringField, Is.EqualTo("TestEntity62"));
      Assert.That(result62[0].Text, Is.EqualTo("This is an instance of TestEntity62"));

      var result63 = session.Query.All<TestEntity63>().ToArray();
      Assert.That(result63.Length, Is.EqualTo(1));
      Assert.That(result63[0].BooleanField, Is.True);
      Assert.That(result63[0].Int16Field, Is.EqualTo(63));
      Assert.That(result63[0].Int32Field, Is.EqualTo(63));
      Assert.That(result63[0].Int64Field, Is.EqualTo(63));
      Assert.That(result63[0].FloatField, Is.EqualTo((float)63));
      Assert.That(result63[0].DoubleField, Is.EqualTo((double)63));
      Assert.That(result63[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result63[0].StringField, Is.EqualTo("TestEntity63"));
      Assert.That(result63[0].Text, Is.EqualTo("This is an instance of TestEntity63"));

      var result64 = session.Query.All<TestEntity64>().ToArray();
      Assert.That(result64.Length, Is.EqualTo(1));
      Assert.That(result64[0].BooleanField, Is.True);
      Assert.That(result64[0].Int16Field, Is.EqualTo(64));
      Assert.That(result64[0].Int32Field, Is.EqualTo(64));
      Assert.That(result64[0].Int64Field, Is.EqualTo(64));
      Assert.That(result64[0].FloatField, Is.EqualTo((float)64));
      Assert.That(result64[0].DoubleField, Is.EqualTo((double)64));
      Assert.That(result64[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result64[0].StringField, Is.EqualTo("TestEntity64"));
      Assert.That(result64[0].Text, Is.EqualTo("This is an instance of TestEntity64"));

      var result65 = session.Query.All<TestEntity65>().ToArray();
      Assert.That(result65.Length, Is.EqualTo(1));
      Assert.That(result65[0].BooleanField, Is.True);
      Assert.That(result65[0].Int16Field, Is.EqualTo(65));
      Assert.That(result65[0].Int32Field, Is.EqualTo(65));
      Assert.That(result65[0].Int64Field, Is.EqualTo(65));
      Assert.That(result65[0].FloatField, Is.EqualTo((float)65));
      Assert.That(result65[0].DoubleField, Is.EqualTo((double)65));
      Assert.That(result65[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result65[0].StringField, Is.EqualTo("TestEntity65"));
      Assert.That(result65[0].Text, Is.EqualTo("This is an instance of TestEntity65"));

      var result66 = session.Query.All<TestEntity66>().ToArray();
      Assert.That(result66.Length, Is.EqualTo(1));
      Assert.That(result66[0].BooleanField, Is.True);
      Assert.That(result66[0].Int16Field, Is.EqualTo(66));
      Assert.That(result66[0].Int32Field, Is.EqualTo(66));
      Assert.That(result66[0].Int64Field, Is.EqualTo(66));
      Assert.That(result66[0].FloatField, Is.EqualTo((float)66));
      Assert.That(result66[0].DoubleField, Is.EqualTo((double)66));
      Assert.That(result66[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result66[0].StringField, Is.EqualTo("TestEntity66"));
      Assert.That(result66[0].Text, Is.EqualTo("This is an instance of TestEntity66"));

      var result67 = session.Query.All<TestEntity67>().ToArray();
      Assert.That(result67.Length, Is.EqualTo(1));
      Assert.That(result67[0].BooleanField, Is.True);
      Assert.That(result67[0].Int16Field, Is.EqualTo(67));
      Assert.That(result67[0].Int32Field, Is.EqualTo(67));
      Assert.That(result67[0].Int64Field, Is.EqualTo(67));
      Assert.That(result67[0].FloatField, Is.EqualTo((float)67));
      Assert.That(result67[0].DoubleField, Is.EqualTo((double)67));
      Assert.That(result67[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result67[0].StringField, Is.EqualTo("TestEntity67"));
      Assert.That(result67[0].Text, Is.EqualTo("This is an instance of TestEntity67"));

      var result68 = session.Query.All<TestEntity68>().ToArray();
      Assert.That(result68.Length, Is.EqualTo(1));
      Assert.That(result68[0].BooleanField, Is.True);
      Assert.That(result68[0].Int16Field, Is.EqualTo(68));
      Assert.That(result68[0].Int32Field, Is.EqualTo(68));
      Assert.That(result68[0].Int64Field, Is.EqualTo(68));
      Assert.That(result68[0].FloatField, Is.EqualTo((float)68));
      Assert.That(result68[0].DoubleField, Is.EqualTo((double)68));
      Assert.That(result68[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result68[0].StringField, Is.EqualTo("TestEntity68"));
      Assert.That(result68[0].Text, Is.EqualTo("This is an instance of TestEntity68"));

      var result69 = session.Query.All<TestEntity69>().ToArray();
      Assert.That(result69.Length, Is.EqualTo(1));
      Assert.That(result69[0].BooleanField, Is.True);
      Assert.That(result69[0].Int16Field, Is.EqualTo(69));
      Assert.That(result69[0].Int32Field, Is.EqualTo(69));
      Assert.That(result69[0].Int64Field, Is.EqualTo(69));
      Assert.That(result69[0].FloatField, Is.EqualTo((float)69));
      Assert.That(result69[0].DoubleField, Is.EqualTo((double)69));
      Assert.That(result69[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result69[0].StringField, Is.EqualTo("TestEntity69"));
      Assert.That(result69[0].Text, Is.EqualTo("This is an instance of TestEntity69"));

      var result70 = session.Query.All<TestEntity70>().ToArray();
      Assert.That(result70.Length, Is.EqualTo(1));
      Assert.That(result70[0].BooleanField, Is.True);
      Assert.That(result70[0].Int16Field, Is.EqualTo(70));
      Assert.That(result70[0].Int32Field, Is.EqualTo(70));
      Assert.That(result70[0].Int64Field, Is.EqualTo(70));
      Assert.That(result70[0].FloatField, Is.EqualTo((float)70));
      Assert.That(result70[0].DoubleField, Is.EqualTo((double)70));
      Assert.That(result70[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result70[0].StringField, Is.EqualTo("TestEntity70"));
      Assert.That(result70[0].Text, Is.EqualTo("This is an instance of TestEntity70"));

      var result71 = session.Query.All<TestEntity71>().ToArray();
      Assert.That(result71.Length, Is.EqualTo(1));
      Assert.That(result71[0].BooleanField, Is.True);
      Assert.That(result71[0].Int16Field, Is.EqualTo(71));
      Assert.That(result71[0].Int32Field, Is.EqualTo(71));
      Assert.That(result71[0].Int64Field, Is.EqualTo(71));
      Assert.That(result71[0].FloatField, Is.EqualTo((float)71));
      Assert.That(result71[0].DoubleField, Is.EqualTo((double)71));
      Assert.That(result71[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result71[0].StringField, Is.EqualTo("TestEntity71"));
      Assert.That(result71[0].Text, Is.EqualTo("This is an instance of TestEntity71"));

      var result72 = session.Query.All<TestEntity72>().ToArray();
      Assert.That(result72.Length, Is.EqualTo(1));
      Assert.That(result72[0].BooleanField, Is.True);
      Assert.That(result72[0].Int16Field, Is.EqualTo(72));
      Assert.That(result72[0].Int32Field, Is.EqualTo(72));
      Assert.That(result72[0].Int64Field, Is.EqualTo(72));
      Assert.That(result72[0].FloatField, Is.EqualTo((float)72));
      Assert.That(result72[0].DoubleField, Is.EqualTo((double)72));
      Assert.That(result72[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result72[0].StringField, Is.EqualTo("TestEntity72"));
      Assert.That(result72[0].Text, Is.EqualTo("This is an instance of TestEntity72"));

      var result73 = session.Query.All<TestEntity73>().ToArray();
      Assert.That(result73.Length, Is.EqualTo(1));
      Assert.That(result73[0].BooleanField, Is.True);
      Assert.That(result73[0].Int16Field, Is.EqualTo(73));
      Assert.That(result73[0].Int32Field, Is.EqualTo(73));
      Assert.That(result73[0].Int64Field, Is.EqualTo(73));
      Assert.That(result73[0].FloatField, Is.EqualTo((float)73));
      Assert.That(result73[0].DoubleField, Is.EqualTo((double)73));
      Assert.That(result73[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result73[0].StringField, Is.EqualTo("TestEntity73"));
      Assert.That(result73[0].Text, Is.EqualTo("This is an instance of TestEntity73"));

      var result74 = session.Query.All<TestEntity74>().ToArray();
      Assert.That(result74.Length, Is.EqualTo(1));
      Assert.That(result74[0].BooleanField, Is.True);
      Assert.That(result74[0].Int16Field, Is.EqualTo(74));
      Assert.That(result74[0].Int32Field, Is.EqualTo(74));
      Assert.That(result74[0].Int64Field, Is.EqualTo(74));
      Assert.That(result74[0].FloatField, Is.EqualTo((float)74));
      Assert.That(result74[0].DoubleField, Is.EqualTo((double)74));
      Assert.That(result74[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result74[0].StringField, Is.EqualTo("TestEntity74"));
      Assert.That(result74[0].Text, Is.EqualTo("This is an instance of TestEntity74"));

      var result75 = session.Query.All<TestEntity75>().ToArray();
      Assert.That(result75.Length, Is.EqualTo(1));
      Assert.That(result75[0].BooleanField, Is.True);
      Assert.That(result75[0].Int16Field, Is.EqualTo(75));
      Assert.That(result75[0].Int32Field, Is.EqualTo(75));
      Assert.That(result75[0].Int64Field, Is.EqualTo(75));
      Assert.That(result75[0].FloatField, Is.EqualTo((float)75));
      Assert.That(result75[0].DoubleField, Is.EqualTo((double)75));
      Assert.That(result75[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result75[0].StringField, Is.EqualTo("TestEntity75"));
      Assert.That(result75[0].Text, Is.EqualTo("This is an instance of TestEntity75"));

      var result76 = session.Query.All<TestEntity76>().ToArray();
      Assert.That(result76.Length, Is.EqualTo(1));
      Assert.That(result76[0].BooleanField, Is.True);
      Assert.That(result76[0].Int16Field, Is.EqualTo(76));
      Assert.That(result76[0].Int32Field, Is.EqualTo(76));
      Assert.That(result76[0].Int64Field, Is.EqualTo(76));
      Assert.That(result76[0].FloatField, Is.EqualTo((float)76));
      Assert.That(result76[0].DoubleField, Is.EqualTo((double)76));
      Assert.That(result76[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result76[0].StringField, Is.EqualTo("TestEntity76"));
      Assert.That(result76[0].Text, Is.EqualTo("This is an instance of TestEntity76"));

      var result77 = session.Query.All<TestEntity77>().ToArray();
      Assert.That(result77.Length, Is.EqualTo(1));
      Assert.That(result77[0].BooleanField, Is.True);
      Assert.That(result77[0].Int16Field, Is.EqualTo(77));
      Assert.That(result77[0].Int32Field, Is.EqualTo(77));
      Assert.That(result77[0].Int64Field, Is.EqualTo(77));
      Assert.That(result77[0].FloatField, Is.EqualTo((float)77));
      Assert.That(result77[0].DoubleField, Is.EqualTo((double)77));
      Assert.That(result77[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result77[0].StringField, Is.EqualTo("TestEntity77"));
      Assert.That(result77[0].Text, Is.EqualTo("This is an instance of TestEntity77"));

      var result78 = session.Query.All<TestEntity78>().ToArray();
      Assert.That(result78.Length, Is.EqualTo(1));
      Assert.That(result78[0].BooleanField, Is.True);
      Assert.That(result78[0].Int16Field, Is.EqualTo(78));
      Assert.That(result78[0].Int32Field, Is.EqualTo(78));
      Assert.That(result78[0].Int64Field, Is.EqualTo(78));
      Assert.That(result78[0].FloatField, Is.EqualTo((float)78));
      Assert.That(result78[0].DoubleField, Is.EqualTo((double)78));
      Assert.That(result78[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result78[0].StringField, Is.EqualTo("TestEntity78"));
      Assert.That(result78[0].Text, Is.EqualTo("This is an instance of TestEntity78"));

      var result79 = session.Query.All<TestEntity79>().ToArray();
      Assert.That(result79.Length, Is.EqualTo(1));
      Assert.That(result79[0].BooleanField, Is.True);
      Assert.That(result79[0].Int16Field, Is.EqualTo(79));
      Assert.That(result79[0].Int32Field, Is.EqualTo(79));
      Assert.That(result79[0].Int64Field, Is.EqualTo(79));
      Assert.That(result79[0].FloatField, Is.EqualTo((float)79));
      Assert.That(result79[0].DoubleField, Is.EqualTo((double)79));
      Assert.That(result79[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result79[0].StringField, Is.EqualTo("TestEntity79"));
      Assert.That(result79[0].Text, Is.EqualTo("This is an instance of TestEntity79"));

      var result80 = session.Query.All<TestEntity80>().ToArray();
      Assert.That(result80.Length, Is.EqualTo(1));
      Assert.That(result80[0].BooleanField, Is.True);
      Assert.That(result80[0].Int16Field, Is.EqualTo(80));
      Assert.That(result80[0].Int32Field, Is.EqualTo(80));
      Assert.That(result80[0].Int64Field, Is.EqualTo(80));
      Assert.That(result80[0].FloatField, Is.EqualTo((float)80));
      Assert.That(result80[0].DoubleField, Is.EqualTo((double)80));
      Assert.That(result80[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result80[0].StringField, Is.EqualTo("TestEntity80"));
      Assert.That(result80[0].Text, Is.EqualTo("This is an instance of TestEntity80"));

      var result81 = session.Query.All<TestEntity81>().ToArray();
      Assert.That(result81.Length, Is.EqualTo(1));
      Assert.That(result81[0].BooleanField, Is.True);
      Assert.That(result81[0].Int16Field, Is.EqualTo(81));
      Assert.That(result81[0].Int32Field, Is.EqualTo(81));
      Assert.That(result81[0].Int64Field, Is.EqualTo(81));
      Assert.That(result81[0].FloatField, Is.EqualTo((float)81));
      Assert.That(result81[0].DoubleField, Is.EqualTo((double)81));
      Assert.That(result81[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result81[0].StringField, Is.EqualTo("TestEntity81"));
      Assert.That(result81[0].Text, Is.EqualTo("This is an instance of TestEntity81"));

      var result82 = session.Query.All<TestEntity82>().ToArray();
      Assert.That(result82.Length, Is.EqualTo(1));
      Assert.That(result82[0].BooleanField, Is.True);
      Assert.That(result82[0].Int16Field, Is.EqualTo(82));
      Assert.That(result82[0].Int32Field, Is.EqualTo(82));
      Assert.That(result82[0].Int64Field, Is.EqualTo(82));
      Assert.That(result82[0].FloatField, Is.EqualTo((float)82));
      Assert.That(result82[0].DoubleField, Is.EqualTo((double)82));
      Assert.That(result82[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result82[0].StringField, Is.EqualTo("TestEntity82"));
      Assert.That(result82[0].Text, Is.EqualTo("This is an instance of TestEntity82"));

      var result83 = session.Query.All<TestEntity83>().ToArray();
      Assert.That(result83.Length, Is.EqualTo(1));
      Assert.That(result83[0].BooleanField, Is.True);
      Assert.That(result83[0].Int16Field, Is.EqualTo(83));
      Assert.That(result83[0].Int32Field, Is.EqualTo(83));
      Assert.That(result83[0].Int64Field, Is.EqualTo(83));
      Assert.That(result83[0].FloatField, Is.EqualTo((float)83));
      Assert.That(result83[0].DoubleField, Is.EqualTo((double)83));
      Assert.That(result83[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result83[0].StringField, Is.EqualTo("TestEntity83"));
      Assert.That(result83[0].Text, Is.EqualTo("This is an instance of TestEntity83"));

      var result84 = session.Query.All<TestEntity84>().ToArray();
      Assert.That(result84.Length, Is.EqualTo(1));
      Assert.That(result84[0].BooleanField, Is.True);
      Assert.That(result84[0].Int16Field, Is.EqualTo(84));
      Assert.That(result84[0].Int32Field, Is.EqualTo(84));
      Assert.That(result84[0].Int64Field, Is.EqualTo(84));
      Assert.That(result84[0].FloatField, Is.EqualTo((float)84));
      Assert.That(result84[0].DoubleField, Is.EqualTo((double)84));
      Assert.That(result84[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result84[0].StringField, Is.EqualTo("TestEntity84"));
      Assert.That(result84[0].Text, Is.EqualTo("This is an instance of TestEntity84"));

      var result85 = session.Query.All<TestEntity85>().ToArray();
      Assert.That(result85.Length, Is.EqualTo(1));
      Assert.That(result85[0].BooleanField, Is.True);
      Assert.That(result85[0].Int16Field, Is.EqualTo(85));
      Assert.That(result85[0].Int32Field, Is.EqualTo(85));
      Assert.That(result85[0].Int64Field, Is.EqualTo(85));
      Assert.That(result85[0].FloatField, Is.EqualTo((float)85));
      Assert.That(result85[0].DoubleField, Is.EqualTo((double)85));
      Assert.That(result85[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result85[0].StringField, Is.EqualTo("TestEntity85"));
      Assert.That(result85[0].Text, Is.EqualTo("This is an instance of TestEntity85"));

      var result86 = session.Query.All<TestEntity86>().ToArray();
      Assert.That(result86.Length, Is.EqualTo(1));
      Assert.That(result86[0].BooleanField, Is.True);
      Assert.That(result86[0].Int16Field, Is.EqualTo(86));
      Assert.That(result86[0].Int32Field, Is.EqualTo(86));
      Assert.That(result86[0].Int64Field, Is.EqualTo(86));
      Assert.That(result86[0].FloatField, Is.EqualTo((float)86));
      Assert.That(result86[0].DoubleField, Is.EqualTo((double)86));
      Assert.That(result86[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result86[0].StringField, Is.EqualTo("TestEntity86"));
      Assert.That(result86[0].Text, Is.EqualTo("This is an instance of TestEntity86"));

      var result87 = session.Query.All<TestEntity87>().ToArray();
      Assert.That(result87.Length, Is.EqualTo(1));
      Assert.That(result87[0].BooleanField, Is.True);
      Assert.That(result87[0].Int16Field, Is.EqualTo(87));
      Assert.That(result87[0].Int32Field, Is.EqualTo(87));
      Assert.That(result87[0].Int64Field, Is.EqualTo(87));
      Assert.That(result87[0].FloatField, Is.EqualTo((float)87));
      Assert.That(result87[0].DoubleField, Is.EqualTo((double)87));
      Assert.That(result87[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result87[0].StringField, Is.EqualTo("TestEntity87"));
      Assert.That(result87[0].Text, Is.EqualTo("This is an instance of TestEntity87"));

      var result88 = session.Query.All<TestEntity88>().ToArray();
      Assert.That(result88.Length, Is.EqualTo(1));
      Assert.That(result88[0].BooleanField, Is.True);
      Assert.That(result88[0].Int16Field, Is.EqualTo(88));
      Assert.That(result88[0].Int32Field, Is.EqualTo(88));
      Assert.That(result88[0].Int64Field, Is.EqualTo(88));
      Assert.That(result88[0].FloatField, Is.EqualTo((float)88));
      Assert.That(result88[0].DoubleField, Is.EqualTo((double)88));
      Assert.That(result88[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result88[0].StringField, Is.EqualTo("TestEntity88"));
      Assert.That(result88[0].Text, Is.EqualTo("This is an instance of TestEntity88"));

      var result89 = session.Query.All<TestEntity89>().ToArray();
      Assert.That(result89.Length, Is.EqualTo(1));
      Assert.That(result89[0].BooleanField, Is.True);
      Assert.That(result89[0].Int16Field, Is.EqualTo(89));
      Assert.That(result89[0].Int32Field, Is.EqualTo(89));
      Assert.That(result89[0].Int64Field, Is.EqualTo(89));
      Assert.That(result89[0].FloatField, Is.EqualTo((float)89));
      Assert.That(result89[0].DoubleField, Is.EqualTo((double)89));
      Assert.That(result89[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result89[0].StringField, Is.EqualTo("TestEntity89"));
      Assert.That(result89[0].Text, Is.EqualTo("This is an instance of TestEntity89"));

      var result90 = session.Query.All<TestEntity90>().ToArray();
      Assert.That(result90.Length, Is.EqualTo(1));
      Assert.That(result90[0].BooleanField, Is.True);
      Assert.That(result90[0].Int16Field, Is.EqualTo(90));
      Assert.That(result90[0].Int32Field, Is.EqualTo(90));
      Assert.That(result90[0].Int64Field, Is.EqualTo(90));
      Assert.That(result90[0].FloatField, Is.EqualTo((float)90));
      Assert.That(result90[0].DoubleField, Is.EqualTo((double)90));
      Assert.That(result90[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result90[0].StringField, Is.EqualTo("TestEntity90"));
      Assert.That(result90[0].Text, Is.EqualTo("This is an instance of TestEntity90"));

      var result91 = session.Query.All<TestEntity91>().ToArray();
      Assert.That(result91.Length, Is.EqualTo(1));
      Assert.That(result91[0].BooleanField, Is.True);
      Assert.That(result91[0].Int16Field, Is.EqualTo(91));
      Assert.That(result91[0].Int32Field, Is.EqualTo(91));
      Assert.That(result91[0].Int64Field, Is.EqualTo(91));
      Assert.That(result91[0].FloatField, Is.EqualTo((float)91));
      Assert.That(result91[0].DoubleField, Is.EqualTo((double)91));
      Assert.That(result91[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result91[0].StringField, Is.EqualTo("TestEntity91"));
      Assert.That(result91[0].Text, Is.EqualTo("This is an instance of TestEntity91"));

      var result92 = session.Query.All<TestEntity92>().ToArray();
      Assert.That(result92.Length, Is.EqualTo(1));
      Assert.That(result92[0].BooleanField, Is.True);
      Assert.That(result92[0].Int16Field, Is.EqualTo(92));
      Assert.That(result92[0].Int32Field, Is.EqualTo(92));
      Assert.That(result92[0].Int64Field, Is.EqualTo(92));
      Assert.That(result92[0].FloatField, Is.EqualTo((float)92));
      Assert.That(result92[0].DoubleField, Is.EqualTo((double)92));
      Assert.That(result92[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result92[0].StringField, Is.EqualTo("TestEntity92"));
      Assert.That(result92[0].Text, Is.EqualTo("This is an instance of TestEntity92"));

      var result93 = session.Query.All<TestEntity93>().ToArray();
      Assert.That(result93.Length, Is.EqualTo(1));
      Assert.That(result93[0].BooleanField, Is.True);
      Assert.That(result93[0].Int16Field, Is.EqualTo(93));
      Assert.That(result93[0].Int32Field, Is.EqualTo(93));
      Assert.That(result93[0].Int64Field, Is.EqualTo(93));
      Assert.That(result93[0].FloatField, Is.EqualTo((float)93));
      Assert.That(result93[0].DoubleField, Is.EqualTo((double)93));
      Assert.That(result93[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result93[0].StringField, Is.EqualTo("TestEntity93"));
      Assert.That(result93[0].Text, Is.EqualTo("This is an instance of TestEntity93"));

      var result94 = session.Query.All<TestEntity94>().ToArray();
      Assert.That(result94.Length, Is.EqualTo(1));
      Assert.That(result94[0].BooleanField, Is.True);
      Assert.That(result94[0].Int16Field, Is.EqualTo(94));
      Assert.That(result94[0].Int32Field, Is.EqualTo(94));
      Assert.That(result94[0].Int64Field, Is.EqualTo(94));
      Assert.That(result94[0].FloatField, Is.EqualTo((float)94));
      Assert.That(result94[0].DoubleField, Is.EqualTo((double)94));
      Assert.That(result94[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result94[0].StringField, Is.EqualTo("TestEntity94"));
      Assert.That(result94[0].Text, Is.EqualTo("This is an instance of TestEntity94"));

      var result95 = session.Query.All<TestEntity95>().ToArray();
      Assert.That(result95.Length, Is.EqualTo(1));
      Assert.That(result95[0].BooleanField, Is.True);
      Assert.That(result95[0].Int16Field, Is.EqualTo(95));
      Assert.That(result95[0].Int32Field, Is.EqualTo(95));
      Assert.That(result95[0].Int64Field, Is.EqualTo(95));
      Assert.That(result95[0].FloatField, Is.EqualTo((float)95));
      Assert.That(result95[0].DoubleField, Is.EqualTo((double)95));
      Assert.That(result95[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result95[0].StringField, Is.EqualTo("TestEntity95"));
      Assert.That(result95[0].Text, Is.EqualTo("This is an instance of TestEntity95"));

      var result96 = session.Query.All<TestEntity96>().ToArray();
      Assert.That(result96.Length, Is.EqualTo(1));
      Assert.That(result96[0].BooleanField, Is.True);
      Assert.That(result96[0].Int16Field, Is.EqualTo(96));
      Assert.That(result96[0].Int32Field, Is.EqualTo(96));
      Assert.That(result96[0].Int64Field, Is.EqualTo(96));
      Assert.That(result96[0].FloatField, Is.EqualTo((float)96));
      Assert.That(result96[0].DoubleField, Is.EqualTo((double)96));
      Assert.That(result96[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result96[0].StringField, Is.EqualTo("TestEntity96"));
      Assert.That(result96[0].Text, Is.EqualTo("This is an instance of TestEntity96"));

      var result97 = session.Query.All<TestEntity97>().ToArray();
      Assert.That(result97.Length, Is.EqualTo(1));
      Assert.That(result97[0].BooleanField, Is.True);
      Assert.That(result97[0].Int16Field, Is.EqualTo(97));
      Assert.That(result97[0].Int32Field, Is.EqualTo(97));
      Assert.That(result97[0].Int64Field, Is.EqualTo(97));
      Assert.That(result97[0].FloatField, Is.EqualTo((float)97));
      Assert.That(result97[0].DoubleField, Is.EqualTo((double)97));
      Assert.That(result97[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result97[0].StringField, Is.EqualTo("TestEntity97"));
      Assert.That(result97[0].Text, Is.EqualTo("This is an instance of TestEntity97"));

      var result98 = session.Query.All<TestEntity98>().ToArray();
      Assert.That(result98.Length, Is.EqualTo(1));
      Assert.That(result98[0].BooleanField, Is.True);
      Assert.That(result98[0].Int16Field, Is.EqualTo(98));
      Assert.That(result98[0].Int32Field, Is.EqualTo(98));
      Assert.That(result98[0].Int64Field, Is.EqualTo(98));
      Assert.That(result98[0].FloatField, Is.EqualTo((float)98));
      Assert.That(result98[0].DoubleField, Is.EqualTo((double)98));
      Assert.That(result98[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result98[0].StringField, Is.EqualTo("TestEntity98"));
      Assert.That(result98[0].Text, Is.EqualTo("This is an instance of TestEntity98"));

      var result99 = session.Query.All<TestEntity99>().ToArray();
      Assert.That(result99.Length, Is.EqualTo(1));
      Assert.That(result99[0].BooleanField, Is.True);
      Assert.That(result99[0].Int16Field, Is.EqualTo(99));
      Assert.That(result99[0].Int32Field, Is.EqualTo(99));
      Assert.That(result99[0].Int64Field, Is.EqualTo(99));
      Assert.That(result99[0].FloatField, Is.EqualTo((float)99));
      Assert.That(result99[0].DoubleField, Is.EqualTo((double)99));
      Assert.That(result99[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result99[0].StringField, Is.EqualTo("TestEntity99"));
      Assert.That(result99[0].Text, Is.EqualTo("This is an instance of TestEntity99"));

      var result100 = session.Query.All<TestEntity100>().ToArray();
      Assert.That(result100.Length, Is.EqualTo(1));
      Assert.That(result100[0].BooleanField, Is.True);
      Assert.That(result100[0].Int16Field, Is.EqualTo(100));
      Assert.That(result100[0].Int32Field, Is.EqualTo(100));
      Assert.That(result100[0].Int64Field, Is.EqualTo(100));
      Assert.That(result100[0].FloatField, Is.EqualTo((float)100));
      Assert.That(result100[0].DoubleField, Is.EqualTo((double)100));
      Assert.That(result100[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result100[0].StringField, Is.EqualTo("TestEntity100"));
      Assert.That(result100[0].Text, Is.EqualTo("This is an instance of TestEntity100"));

      var result101 = session.Query.All<TestEntity101>().ToArray();
      Assert.That(result101.Length, Is.EqualTo(1));
      Assert.That(result101[0].BooleanField, Is.True);
      Assert.That(result101[0].Int16Field, Is.EqualTo(101));
      Assert.That(result101[0].Int32Field, Is.EqualTo(101));
      Assert.That(result101[0].Int64Field, Is.EqualTo(101));
      Assert.That(result101[0].FloatField, Is.EqualTo((float)101));
      Assert.That(result101[0].DoubleField, Is.EqualTo((double)101));
      Assert.That(result101[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result101[0].StringField, Is.EqualTo("TestEntity101"));
      Assert.That(result101[0].Text, Is.EqualTo("This is an instance of TestEntity101"));

      var result102 = session.Query.All<TestEntity102>().ToArray();
      Assert.That(result102.Length, Is.EqualTo(1));
      Assert.That(result102[0].BooleanField, Is.True);
      Assert.That(result102[0].Int16Field, Is.EqualTo(102));
      Assert.That(result102[0].Int32Field, Is.EqualTo(102));
      Assert.That(result102[0].Int64Field, Is.EqualTo(102));
      Assert.That(result102[0].FloatField, Is.EqualTo((float)102));
      Assert.That(result102[0].DoubleField, Is.EqualTo((double)102));
      Assert.That(result102[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result102[0].StringField, Is.EqualTo("TestEntity102"));
      Assert.That(result102[0].Text, Is.EqualTo("This is an instance of TestEntity102"));

      var result103 = session.Query.All<TestEntity103>().ToArray();
      Assert.That(result103.Length, Is.EqualTo(1));
      Assert.That(result103[0].BooleanField, Is.True);
      Assert.That(result103[0].Int16Field, Is.EqualTo(103));
      Assert.That(result103[0].Int32Field, Is.EqualTo(103));
      Assert.That(result103[0].Int64Field, Is.EqualTo(103));
      Assert.That(result103[0].FloatField, Is.EqualTo((float)103));
      Assert.That(result103[0].DoubleField, Is.EqualTo((double)103));
      Assert.That(result103[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result103[0].StringField, Is.EqualTo("TestEntity103"));
      Assert.That(result103[0].Text, Is.EqualTo("This is an instance of TestEntity103"));

      var result104 = session.Query.All<TestEntity104>().ToArray();
      Assert.That(result104.Length, Is.EqualTo(1));
      Assert.That(result104[0].BooleanField, Is.True);
      Assert.That(result104[0].Int16Field, Is.EqualTo(104));
      Assert.That(result104[0].Int32Field, Is.EqualTo(104));
      Assert.That(result104[0].Int64Field, Is.EqualTo(104));
      Assert.That(result104[0].FloatField, Is.EqualTo((float)104));
      Assert.That(result104[0].DoubleField, Is.EqualTo((double)104));
      Assert.That(result104[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result104[0].StringField, Is.EqualTo("TestEntity104"));
      Assert.That(result104[0].Text, Is.EqualTo("This is an instance of TestEntity104"));

      var result105 = session.Query.All<TestEntity105>().ToArray();
      Assert.That(result105.Length, Is.EqualTo(1));
      Assert.That(result105[0].BooleanField, Is.True);
      Assert.That(result105[0].Int16Field, Is.EqualTo(105));
      Assert.That(result105[0].Int32Field, Is.EqualTo(105));
      Assert.That(result105[0].Int64Field, Is.EqualTo(105));
      Assert.That(result105[0].FloatField, Is.EqualTo((float)105));
      Assert.That(result105[0].DoubleField, Is.EqualTo((double)105));
      Assert.That(result105[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result105[0].StringField, Is.EqualTo("TestEntity105"));
      Assert.That(result105[0].Text, Is.EqualTo("This is an instance of TestEntity105"));

      var result106 = session.Query.All<TestEntity106>().ToArray();
      Assert.That(result106.Length, Is.EqualTo(1));
      Assert.That(result106[0].BooleanField, Is.True);
      Assert.That(result106[0].Int16Field, Is.EqualTo(106));
      Assert.That(result106[0].Int32Field, Is.EqualTo(106));
      Assert.That(result106[0].Int64Field, Is.EqualTo(106));
      Assert.That(result106[0].FloatField, Is.EqualTo((float)106));
      Assert.That(result106[0].DoubleField, Is.EqualTo((double)106));
      Assert.That(result106[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result106[0].StringField, Is.EqualTo("TestEntity106"));
      Assert.That(result106[0].Text, Is.EqualTo("This is an instance of TestEntity106"));

      var result107 = session.Query.All<TestEntity107>().ToArray();
      Assert.That(result107.Length, Is.EqualTo(1));
      Assert.That(result107[0].BooleanField, Is.True);
      Assert.That(result107[0].Int16Field, Is.EqualTo(107));
      Assert.That(result107[0].Int32Field, Is.EqualTo(107));
      Assert.That(result107[0].Int64Field, Is.EqualTo(107));
      Assert.That(result107[0].FloatField, Is.EqualTo((float)107));
      Assert.That(result107[0].DoubleField, Is.EqualTo((double)107));
      Assert.That(result107[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result107[0].StringField, Is.EqualTo("TestEntity107"));
      Assert.That(result107[0].Text, Is.EqualTo("This is an instance of TestEntity107"));

      var result108 = session.Query.All<TestEntity108>().ToArray();
      Assert.That(result108.Length, Is.EqualTo(1));
      Assert.That(result108[0].BooleanField, Is.True);
      Assert.That(result108[0].Int16Field, Is.EqualTo(108));
      Assert.That(result108[0].Int32Field, Is.EqualTo(108));
      Assert.That(result108[0].Int64Field, Is.EqualTo(108));
      Assert.That(result108[0].FloatField, Is.EqualTo((float)108));
      Assert.That(result108[0].DoubleField, Is.EqualTo((double)108));
      Assert.That(result108[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result108[0].StringField, Is.EqualTo("TestEntity108"));
      Assert.That(result108[0].Text, Is.EqualTo("This is an instance of TestEntity108"));

      var result109 = session.Query.All<TestEntity109>().ToArray();
      Assert.That(result109.Length, Is.EqualTo(1));
      Assert.That(result109[0].BooleanField, Is.True);
      Assert.That(result109[0].Int16Field, Is.EqualTo(109));
      Assert.That(result109[0].Int32Field, Is.EqualTo(109));
      Assert.That(result109[0].Int64Field, Is.EqualTo(109));
      Assert.That(result109[0].FloatField, Is.EqualTo((float)109));
      Assert.That(result109[0].DoubleField, Is.EqualTo((double)109));
      Assert.That(result109[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result109[0].StringField, Is.EqualTo("TestEntity109"));
      Assert.That(result109[0].Text, Is.EqualTo("This is an instance of TestEntity109"));

      var result110 = session.Query.All<TestEntity110>().ToArray();
      Assert.That(result110.Length, Is.EqualTo(1));
      Assert.That(result110[0].BooleanField, Is.True);
      Assert.That(result110[0].Int16Field, Is.EqualTo(110));
      Assert.That(result110[0].Int32Field, Is.EqualTo(110));
      Assert.That(result110[0].Int64Field, Is.EqualTo(110));
      Assert.That(result110[0].FloatField, Is.EqualTo((float)110));
      Assert.That(result110[0].DoubleField, Is.EqualTo((double)110));
      Assert.That(result110[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result110[0].StringField, Is.EqualTo("TestEntity110"));
      Assert.That(result110[0].Text, Is.EqualTo("This is an instance of TestEntity110"));

      var result111 = session.Query.All<TestEntity111>().ToArray();
      Assert.That(result111.Length, Is.EqualTo(1));
      Assert.That(result111[0].BooleanField, Is.True);
      Assert.That(result111[0].Int16Field, Is.EqualTo(111));
      Assert.That(result111[0].Int32Field, Is.EqualTo(111));
      Assert.That(result111[0].Int64Field, Is.EqualTo(111));
      Assert.That(result111[0].FloatField, Is.EqualTo((float)111));
      Assert.That(result111[0].DoubleField, Is.EqualTo((double)111));
      Assert.That(result111[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result111[0].StringField, Is.EqualTo("TestEntity111"));
      Assert.That(result111[0].Text, Is.EqualTo("This is an instance of TestEntity111"));

      var result112 = session.Query.All<TestEntity112>().ToArray();
      Assert.That(result112.Length, Is.EqualTo(1));
      Assert.That(result112[0].BooleanField, Is.True);
      Assert.That(result112[0].Int16Field, Is.EqualTo(112));
      Assert.That(result112[0].Int32Field, Is.EqualTo(112));
      Assert.That(result112[0].Int64Field, Is.EqualTo(112));
      Assert.That(result112[0].FloatField, Is.EqualTo((float)112));
      Assert.That(result112[0].DoubleField, Is.EqualTo((double)112));
      Assert.That(result112[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result112[0].StringField, Is.EqualTo("TestEntity112"));
      Assert.That(result112[0].Text, Is.EqualTo("This is an instance of TestEntity112"));

      var result113 = session.Query.All<TestEntity113>().ToArray();
      Assert.That(result113.Length, Is.EqualTo(1));
      Assert.That(result113[0].BooleanField, Is.True);
      Assert.That(result113[0].Int16Field, Is.EqualTo(113));
      Assert.That(result113[0].Int32Field, Is.EqualTo(113));
      Assert.That(result113[0].Int64Field, Is.EqualTo(113));
      Assert.That(result113[0].FloatField, Is.EqualTo((float)113));
      Assert.That(result113[0].DoubleField, Is.EqualTo((double)113));
      Assert.That(result113[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result113[0].StringField, Is.EqualTo("TestEntity113"));
      Assert.That(result113[0].Text, Is.EqualTo("This is an instance of TestEntity113"));

      var result114 = session.Query.All<TestEntity114>().ToArray();
      Assert.That(result114.Length, Is.EqualTo(1));
      Assert.That(result114[0].BooleanField, Is.True);
      Assert.That(result114[0].Int16Field, Is.EqualTo(114));
      Assert.That(result114[0].Int32Field, Is.EqualTo(114));
      Assert.That(result114[0].Int64Field, Is.EqualTo(114));
      Assert.That(result114[0].FloatField, Is.EqualTo((float)114));
      Assert.That(result114[0].DoubleField, Is.EqualTo((double)114));
      Assert.That(result114[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result114[0].StringField, Is.EqualTo("TestEntity114"));
      Assert.That(result114[0].Text, Is.EqualTo("This is an instance of TestEntity114"));

      var result115 = session.Query.All<TestEntity115>().ToArray();
      Assert.That(result115.Length, Is.EqualTo(1));
      Assert.That(result115[0].BooleanField, Is.True);
      Assert.That(result115[0].Int16Field, Is.EqualTo(115));
      Assert.That(result115[0].Int32Field, Is.EqualTo(115));
      Assert.That(result115[0].Int64Field, Is.EqualTo(115));
      Assert.That(result115[0].FloatField, Is.EqualTo((float)115));
      Assert.That(result115[0].DoubleField, Is.EqualTo((double)115));
      Assert.That(result115[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result115[0].StringField, Is.EqualTo("TestEntity115"));
      Assert.That(result115[0].Text, Is.EqualTo("This is an instance of TestEntity115"));

      var result116 = session.Query.All<TestEntity116>().ToArray();
      Assert.That(result116.Length, Is.EqualTo(1));
      Assert.That(result116[0].BooleanField, Is.True);
      Assert.That(result116[0].Int16Field, Is.EqualTo(116));
      Assert.That(result116[0].Int32Field, Is.EqualTo(116));
      Assert.That(result116[0].Int64Field, Is.EqualTo(116));
      Assert.That(result116[0].FloatField, Is.EqualTo((float)116));
      Assert.That(result116[0].DoubleField, Is.EqualTo((double)116));
      Assert.That(result116[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result116[0].StringField, Is.EqualTo("TestEntity116"));
      Assert.That(result116[0].Text, Is.EqualTo("This is an instance of TestEntity116"));

      var result117 = session.Query.All<TestEntity117>().ToArray();
      Assert.That(result117.Length, Is.EqualTo(1));
      Assert.That(result117[0].BooleanField, Is.True);
      Assert.That(result117[0].Int16Field, Is.EqualTo(117));
      Assert.That(result117[0].Int32Field, Is.EqualTo(117));
      Assert.That(result117[0].Int64Field, Is.EqualTo(117));
      Assert.That(result117[0].FloatField, Is.EqualTo((float)117));
      Assert.That(result117[0].DoubleField, Is.EqualTo((double)117));
      Assert.That(result117[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result117[0].StringField, Is.EqualTo("TestEntity117"));
      Assert.That(result117[0].Text, Is.EqualTo("This is an instance of TestEntity117"));

      var result118 = session.Query.All<TestEntity118>().ToArray();
      Assert.That(result118.Length, Is.EqualTo(1));
      Assert.That(result118[0].BooleanField, Is.True);
      Assert.That(result118[0].Int16Field, Is.EqualTo(118));
      Assert.That(result118[0].Int32Field, Is.EqualTo(118));
      Assert.That(result118[0].Int64Field, Is.EqualTo(118));
      Assert.That(result118[0].FloatField, Is.EqualTo((float)118));
      Assert.That(result118[0].DoubleField, Is.EqualTo((double)118));
      Assert.That(result118[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result118[0].StringField, Is.EqualTo("TestEntity118"));
      Assert.That(result118[0].Text, Is.EqualTo("This is an instance of TestEntity118"));

      var result119 = session.Query.All<TestEntity119>().ToArray();
      Assert.That(result119.Length, Is.EqualTo(1));
      Assert.That(result119[0].BooleanField, Is.True);
      Assert.That(result119[0].Int16Field, Is.EqualTo(119));
      Assert.That(result119[0].Int32Field, Is.EqualTo(119));
      Assert.That(result119[0].Int64Field, Is.EqualTo(119));
      Assert.That(result119[0].FloatField, Is.EqualTo((float)119));
      Assert.That(result119[0].DoubleField, Is.EqualTo((double)119));
      Assert.That(result119[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result119[0].StringField, Is.EqualTo("TestEntity119"));
      Assert.That(result119[0].Text, Is.EqualTo("This is an instance of TestEntity119"));

      var result120 = session.Query.All<TestEntity120>().ToArray();
      Assert.That(result120.Length, Is.EqualTo(1));
      Assert.That(result120[0].BooleanField, Is.True);
      Assert.That(result120[0].Int16Field, Is.EqualTo(120));
      Assert.That(result120[0].Int32Field, Is.EqualTo(120));
      Assert.That(result120[0].Int64Field, Is.EqualTo(120));
      Assert.That(result120[0].FloatField, Is.EqualTo((float)120));
      Assert.That(result120[0].DoubleField, Is.EqualTo((double)120));
      Assert.That(result120[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result120[0].StringField, Is.EqualTo("TestEntity120"));
      Assert.That(result120[0].Text, Is.EqualTo("This is an instance of TestEntity120"));

      var result121 = session.Query.All<TestEntity121>().ToArray();
      Assert.That(result121.Length, Is.EqualTo(1));
      Assert.That(result121[0].BooleanField, Is.True);
      Assert.That(result121[0].Int16Field, Is.EqualTo(121));
      Assert.That(result121[0].Int32Field, Is.EqualTo(121));
      Assert.That(result121[0].Int64Field, Is.EqualTo(121));
      Assert.That(result121[0].FloatField, Is.EqualTo((float)121));
      Assert.That(result121[0].DoubleField, Is.EqualTo((double)121));
      Assert.That(result121[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result121[0].StringField, Is.EqualTo("TestEntity121"));
      Assert.That(result121[0].Text, Is.EqualTo("This is an instance of TestEntity121"));

      var result122 = session.Query.All<TestEntity122>().ToArray();
      Assert.That(result122.Length, Is.EqualTo(1));
      Assert.That(result122[0].BooleanField, Is.True);
      Assert.That(result122[0].Int16Field, Is.EqualTo(122));
      Assert.That(result122[0].Int32Field, Is.EqualTo(122));
      Assert.That(result122[0].Int64Field, Is.EqualTo(122));
      Assert.That(result122[0].FloatField, Is.EqualTo((float)122));
      Assert.That(result122[0].DoubleField, Is.EqualTo((double)122));
      Assert.That(result122[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result122[0].StringField, Is.EqualTo("TestEntity122"));
      Assert.That(result122[0].Text, Is.EqualTo("This is an instance of TestEntity122"));

      var result123 = session.Query.All<TestEntity123>().ToArray();
      Assert.That(result123.Length, Is.EqualTo(1));
      Assert.That(result123[0].BooleanField, Is.True);
      Assert.That(result123[0].Int16Field, Is.EqualTo(123));
      Assert.That(result123[0].Int32Field, Is.EqualTo(123));
      Assert.That(result123[0].Int64Field, Is.EqualTo(123));
      Assert.That(result123[0].FloatField, Is.EqualTo((float)123));
      Assert.That(result123[0].DoubleField, Is.EqualTo((double)123));
      Assert.That(result123[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result123[0].StringField, Is.EqualTo("TestEntity123"));
      Assert.That(result123[0].Text, Is.EqualTo("This is an instance of TestEntity123"));

      var result124 = session.Query.All<TestEntity124>().ToArray();
      Assert.That(result124.Length, Is.EqualTo(1));
      Assert.That(result124[0].BooleanField, Is.True);
      Assert.That(result124[0].Int16Field, Is.EqualTo(124));
      Assert.That(result124[0].Int32Field, Is.EqualTo(124));
      Assert.That(result124[0].Int64Field, Is.EqualTo(124));
      Assert.That(result124[0].FloatField, Is.EqualTo((float)124));
      Assert.That(result124[0].DoubleField, Is.EqualTo((double)124));
      Assert.That(result124[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result124[0].StringField, Is.EqualTo("TestEntity124"));
      Assert.That(result124[0].Text, Is.EqualTo("This is an instance of TestEntity124"));

      var result125 = session.Query.All<TestEntity125>().ToArray();
      Assert.That(result125.Length, Is.EqualTo(1));
      Assert.That(result125[0].BooleanField, Is.True);
      Assert.That(result125[0].Int16Field, Is.EqualTo(125));
      Assert.That(result125[0].Int32Field, Is.EqualTo(125));
      Assert.That(result125[0].Int64Field, Is.EqualTo(125));
      Assert.That(result125[0].FloatField, Is.EqualTo((float)125));
      Assert.That(result125[0].DoubleField, Is.EqualTo((double)125));
      Assert.That(result125[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result125[0].StringField, Is.EqualTo("TestEntity125"));
      Assert.That(result125[0].Text, Is.EqualTo("This is an instance of TestEntity125"));

      var result126 = session.Query.All<TestEntity126>().ToArray();
      Assert.That(result126.Length, Is.EqualTo(1));
      Assert.That(result126[0].BooleanField, Is.True);
      Assert.That(result126[0].Int16Field, Is.EqualTo(126));
      Assert.That(result126[0].Int32Field, Is.EqualTo(126));
      Assert.That(result126[0].Int64Field, Is.EqualTo(126));
      Assert.That(result126[0].FloatField, Is.EqualTo((float)126));
      Assert.That(result126[0].DoubleField, Is.EqualTo((double)126));
      Assert.That(result126[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result126[0].StringField, Is.EqualTo("TestEntity126"));
      Assert.That(result126[0].Text, Is.EqualTo("This is an instance of TestEntity126"));

      var result127 = session.Query.All<TestEntity127>().ToArray();
      Assert.That(result127.Length, Is.EqualTo(1));
      Assert.That(result127[0].BooleanField, Is.True);
      Assert.That(result127[0].Int16Field, Is.EqualTo(127));
      Assert.That(result127[0].Int32Field, Is.EqualTo(127));
      Assert.That(result127[0].Int64Field, Is.EqualTo(127));
      Assert.That(result127[0].FloatField, Is.EqualTo((float)127));
      Assert.That(result127[0].DoubleField, Is.EqualTo((double)127));
      Assert.That(result127[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result127[0].StringField, Is.EqualTo("TestEntity127"));
      Assert.That(result127[0].Text, Is.EqualTo("This is an instance of TestEntity127"));

      var result128 = session.Query.All<TestEntity128>().ToArray();
      Assert.That(result128.Length, Is.EqualTo(1));
      Assert.That(result128[0].BooleanField, Is.True);
      Assert.That(result128[0].Int16Field, Is.EqualTo(128));
      Assert.That(result128[0].Int32Field, Is.EqualTo(128));
      Assert.That(result128[0].Int64Field, Is.EqualTo(128));
      Assert.That(result128[0].FloatField, Is.EqualTo((float)128));
      Assert.That(result128[0].DoubleField, Is.EqualTo((double)128));
      Assert.That(result128[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result128[0].StringField, Is.EqualTo("TestEntity128"));
      Assert.That(result128[0].Text, Is.EqualTo("This is an instance of TestEntity128"));

      var result129 = session.Query.All<TestEntity129>().ToArray();
      Assert.That(result129.Length, Is.EqualTo(1));
      Assert.That(result129[0].BooleanField, Is.True);
      Assert.That(result129[0].Int16Field, Is.EqualTo(129));
      Assert.That(result129[0].Int32Field, Is.EqualTo(129));
      Assert.That(result129[0].Int64Field, Is.EqualTo(129));
      Assert.That(result129[0].FloatField, Is.EqualTo((float)129));
      Assert.That(result129[0].DoubleField, Is.EqualTo((double)129));
      Assert.That(result129[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result129[0].StringField, Is.EqualTo("TestEntity129"));
      Assert.That(result129[0].Text, Is.EqualTo("This is an instance of TestEntity129"));

      var result130 = session.Query.All<TestEntity130>().ToArray();
      Assert.That(result130.Length, Is.EqualTo(1));
      Assert.That(result130[0].BooleanField, Is.True);
      Assert.That(result130[0].Int16Field, Is.EqualTo(130));
      Assert.That(result130[0].Int32Field, Is.EqualTo(130));
      Assert.That(result130[0].Int64Field, Is.EqualTo(130));
      Assert.That(result130[0].FloatField, Is.EqualTo((float)130));
      Assert.That(result130[0].DoubleField, Is.EqualTo((double)130));
      Assert.That(result130[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result130[0].StringField, Is.EqualTo("TestEntity130"));
      Assert.That(result130[0].Text, Is.EqualTo("This is an instance of TestEntity130"));

      var result131 = session.Query.All<TestEntity131>().ToArray();
      Assert.That(result131.Length, Is.EqualTo(1));
      Assert.That(result131[0].BooleanField, Is.True);
      Assert.That(result131[0].Int16Field, Is.EqualTo(131));
      Assert.That(result131[0].Int32Field, Is.EqualTo(131));
      Assert.That(result131[0].Int64Field, Is.EqualTo(131));
      Assert.That(result131[0].FloatField, Is.EqualTo((float)131));
      Assert.That(result131[0].DoubleField, Is.EqualTo((double)131));
      Assert.That(result131[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result131[0].StringField, Is.EqualTo("TestEntity131"));
      Assert.That(result131[0].Text, Is.EqualTo("This is an instance of TestEntity131"));

      var result132 = session.Query.All<TestEntity132>().ToArray();
      Assert.That(result132.Length, Is.EqualTo(1));
      Assert.That(result132[0].BooleanField, Is.True);
      Assert.That(result132[0].Int16Field, Is.EqualTo(132));
      Assert.That(result132[0].Int32Field, Is.EqualTo(132));
      Assert.That(result132[0].Int64Field, Is.EqualTo(132));
      Assert.That(result132[0].FloatField, Is.EqualTo((float)132));
      Assert.That(result132[0].DoubleField, Is.EqualTo((double)132));
      Assert.That(result132[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result132[0].StringField, Is.EqualTo("TestEntity132"));
      Assert.That(result132[0].Text, Is.EqualTo("This is an instance of TestEntity132"));

      var result133 = session.Query.All<TestEntity133>().ToArray();
      Assert.That(result133.Length, Is.EqualTo(1));
      Assert.That(result133[0].BooleanField, Is.True);
      Assert.That(result133[0].Int16Field, Is.EqualTo(133));
      Assert.That(result133[0].Int32Field, Is.EqualTo(133));
      Assert.That(result133[0].Int64Field, Is.EqualTo(133));
      Assert.That(result133[0].FloatField, Is.EqualTo((float)133));
      Assert.That(result133[0].DoubleField, Is.EqualTo((double)133));
      Assert.That(result133[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result133[0].StringField, Is.EqualTo("TestEntity133"));
      Assert.That(result133[0].Text, Is.EqualTo("This is an instance of TestEntity133"));

      var result134 = session.Query.All<TestEntity134>().ToArray();
      Assert.That(result134.Length, Is.EqualTo(1));
      Assert.That(result134[0].BooleanField, Is.True);
      Assert.That(result134[0].Int16Field, Is.EqualTo(134));
      Assert.That(result134[0].Int32Field, Is.EqualTo(134));
      Assert.That(result134[0].Int64Field, Is.EqualTo(134));
      Assert.That(result134[0].FloatField, Is.EqualTo((float)134));
      Assert.That(result134[0].DoubleField, Is.EqualTo((double)134));
      Assert.That(result134[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result134[0].StringField, Is.EqualTo("TestEntity134"));
      Assert.That(result134[0].Text, Is.EqualTo("This is an instance of TestEntity134"));

      var result135 = session.Query.All<TestEntity135>().ToArray();
      Assert.That(result135.Length, Is.EqualTo(1));
      Assert.That(result135[0].BooleanField, Is.True);
      Assert.That(result135[0].Int16Field, Is.EqualTo(135));
      Assert.That(result135[0].Int32Field, Is.EqualTo(135));
      Assert.That(result135[0].Int64Field, Is.EqualTo(135));
      Assert.That(result135[0].FloatField, Is.EqualTo((float)135));
      Assert.That(result135[0].DoubleField, Is.EqualTo((double)135));
      Assert.That(result135[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result135[0].StringField, Is.EqualTo("TestEntity135"));
      Assert.That(result135[0].Text, Is.EqualTo("This is an instance of TestEntity135"));

      var result136 = session.Query.All<TestEntity136>().ToArray();
      Assert.That(result136.Length, Is.EqualTo(1));
      Assert.That(result136[0].BooleanField, Is.True);
      Assert.That(result136[0].Int16Field, Is.EqualTo(136));
      Assert.That(result136[0].Int32Field, Is.EqualTo(136));
      Assert.That(result136[0].Int64Field, Is.EqualTo(136));
      Assert.That(result136[0].FloatField, Is.EqualTo((float)136));
      Assert.That(result136[0].DoubleField, Is.EqualTo((double)136));
      Assert.That(result136[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result136[0].StringField, Is.EqualTo("TestEntity136"));
      Assert.That(result136[0].Text, Is.EqualTo("This is an instance of TestEntity136"));

      var result137 = session.Query.All<TestEntity137>().ToArray();
      Assert.That(result137.Length, Is.EqualTo(1));
      Assert.That(result137[0].BooleanField, Is.True);
      Assert.That(result137[0].Int16Field, Is.EqualTo(137));
      Assert.That(result137[0].Int32Field, Is.EqualTo(137));
      Assert.That(result137[0].Int64Field, Is.EqualTo(137));
      Assert.That(result137[0].FloatField, Is.EqualTo((float)137));
      Assert.That(result137[0].DoubleField, Is.EqualTo((double)137));
      Assert.That(result137[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result137[0].StringField, Is.EqualTo("TestEntity137"));
      Assert.That(result137[0].Text, Is.EqualTo("This is an instance of TestEntity137"));

      var result138 = session.Query.All<TestEntity138>().ToArray();
      Assert.That(result138.Length, Is.EqualTo(1));
      Assert.That(result138[0].BooleanField, Is.True);
      Assert.That(result138[0].Int16Field, Is.EqualTo(138));
      Assert.That(result138[0].Int32Field, Is.EqualTo(138));
      Assert.That(result138[0].Int64Field, Is.EqualTo(138));
      Assert.That(result138[0].FloatField, Is.EqualTo((float)138));
      Assert.That(result138[0].DoubleField, Is.EqualTo((double)138));
      Assert.That(result138[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result138[0].StringField, Is.EqualTo("TestEntity138"));
      Assert.That(result138[0].Text, Is.EqualTo("This is an instance of TestEntity138"));

      var result139 = session.Query.All<TestEntity139>().ToArray();
      Assert.That(result139.Length, Is.EqualTo(1));
      Assert.That(result139[0].BooleanField, Is.True);
      Assert.That(result139[0].Int16Field, Is.EqualTo(139));
      Assert.That(result139[0].Int32Field, Is.EqualTo(139));
      Assert.That(result139[0].Int64Field, Is.EqualTo(139));
      Assert.That(result139[0].FloatField, Is.EqualTo((float)139));
      Assert.That(result139[0].DoubleField, Is.EqualTo((double)139));
      Assert.That(result139[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result139[0].StringField, Is.EqualTo("TestEntity139"));
      Assert.That(result139[0].Text, Is.EqualTo("This is an instance of TestEntity139"));

      var result140 = session.Query.All<TestEntity140>().ToArray();
      Assert.That(result140.Length, Is.EqualTo(1));
      Assert.That(result140[0].BooleanField, Is.True);
      Assert.That(result140[0].Int16Field, Is.EqualTo(140));
      Assert.That(result140[0].Int32Field, Is.EqualTo(140));
      Assert.That(result140[0].Int64Field, Is.EqualTo(140));
      Assert.That(result140[0].FloatField, Is.EqualTo((float)140));
      Assert.That(result140[0].DoubleField, Is.EqualTo((double)140));
      Assert.That(result140[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result140[0].StringField, Is.EqualTo("TestEntity140"));
      Assert.That(result140[0].Text, Is.EqualTo("This is an instance of TestEntity140"));

      var result141 = session.Query.All<TestEntity141>().ToArray();
      Assert.That(result141.Length, Is.EqualTo(1));
      Assert.That(result141[0].BooleanField, Is.True);
      Assert.That(result141[0].Int16Field, Is.EqualTo(141));
      Assert.That(result141[0].Int32Field, Is.EqualTo(141));
      Assert.That(result141[0].Int64Field, Is.EqualTo(141));
      Assert.That(result141[0].FloatField, Is.EqualTo((float)141));
      Assert.That(result141[0].DoubleField, Is.EqualTo((double)141));
      Assert.That(result141[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result141[0].StringField, Is.EqualTo("TestEntity141"));
      Assert.That(result141[0].Text, Is.EqualTo("This is an instance of TestEntity141"));

      var result142 = session.Query.All<TestEntity142>().ToArray();
      Assert.That(result142.Length, Is.EqualTo(1));
      Assert.That(result142[0].BooleanField, Is.True);
      Assert.That(result142[0].Int16Field, Is.EqualTo(142));
      Assert.That(result142[0].Int32Field, Is.EqualTo(142));
      Assert.That(result142[0].Int64Field, Is.EqualTo(142));
      Assert.That(result142[0].FloatField, Is.EqualTo((float)142));
      Assert.That(result142[0].DoubleField, Is.EqualTo((double)142));
      Assert.That(result142[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result142[0].StringField, Is.EqualTo("TestEntity142"));
      Assert.That(result142[0].Text, Is.EqualTo("This is an instance of TestEntity142"));

      var result143 = session.Query.All<TestEntity143>().ToArray();
      Assert.That(result143.Length, Is.EqualTo(1));
      Assert.That(result143[0].BooleanField, Is.True);
      Assert.That(result143[0].Int16Field, Is.EqualTo(143));
      Assert.That(result143[0].Int32Field, Is.EqualTo(143));
      Assert.That(result143[0].Int64Field, Is.EqualTo(143));
      Assert.That(result143[0].FloatField, Is.EqualTo((float)143));
      Assert.That(result143[0].DoubleField, Is.EqualTo((double)143));
      Assert.That(result143[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result143[0].StringField, Is.EqualTo("TestEntity143"));
      Assert.That(result143[0].Text, Is.EqualTo("This is an instance of TestEntity143"));

      var result144 = session.Query.All<TestEntity144>().ToArray();
      Assert.That(result144.Length, Is.EqualTo(1));
      Assert.That(result144[0].BooleanField, Is.True);
      Assert.That(result144[0].Int16Field, Is.EqualTo(144));
      Assert.That(result144[0].Int32Field, Is.EqualTo(144));
      Assert.That(result144[0].Int64Field, Is.EqualTo(144));
      Assert.That(result144[0].FloatField, Is.EqualTo((float)144));
      Assert.That(result144[0].DoubleField, Is.EqualTo((double)144));
      Assert.That(result144[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result144[0].StringField, Is.EqualTo("TestEntity144"));
      Assert.That(result144[0].Text, Is.EqualTo("This is an instance of TestEntity144"));

      var result145 = session.Query.All<TestEntity145>().ToArray();
      Assert.That(result145.Length, Is.EqualTo(1));
      Assert.That(result145[0].BooleanField, Is.True);
      Assert.That(result145[0].Int16Field, Is.EqualTo(145));
      Assert.That(result145[0].Int32Field, Is.EqualTo(145));
      Assert.That(result145[0].Int64Field, Is.EqualTo(145));
      Assert.That(result145[0].FloatField, Is.EqualTo((float)145));
      Assert.That(result145[0].DoubleField, Is.EqualTo((double)145));
      Assert.That(result145[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result145[0].StringField, Is.EqualTo("TestEntity145"));
      Assert.That(result145[0].Text, Is.EqualTo("This is an instance of TestEntity145"));

      var result146 = session.Query.All<TestEntity146>().ToArray();
      Assert.That(result146.Length, Is.EqualTo(1));
      Assert.That(result146[0].BooleanField, Is.True);
      Assert.That(result146[0].Int16Field, Is.EqualTo(146));
      Assert.That(result146[0].Int32Field, Is.EqualTo(146));
      Assert.That(result146[0].Int64Field, Is.EqualTo(146));
      Assert.That(result146[0].FloatField, Is.EqualTo((float)146));
      Assert.That(result146[0].DoubleField, Is.EqualTo((double)146));
      Assert.That(result146[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result146[0].StringField, Is.EqualTo("TestEntity146"));
      Assert.That(result146[0].Text, Is.EqualTo("This is an instance of TestEntity146"));

      var result147 = session.Query.All<TestEntity147>().ToArray();
      Assert.That(result147.Length, Is.EqualTo(1));
      Assert.That(result147[0].BooleanField, Is.True);
      Assert.That(result147[0].Int16Field, Is.EqualTo(147));
      Assert.That(result147[0].Int32Field, Is.EqualTo(147));
      Assert.That(result147[0].Int64Field, Is.EqualTo(147));
      Assert.That(result147[0].FloatField, Is.EqualTo((float)147));
      Assert.That(result147[0].DoubleField, Is.EqualTo((double)147));
      Assert.That(result147[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result147[0].StringField, Is.EqualTo("TestEntity147"));
      Assert.That(result147[0].Text, Is.EqualTo("This is an instance of TestEntity147"));

      var result148 = session.Query.All<TestEntity148>().ToArray();
      Assert.That(result148.Length, Is.EqualTo(1));
      Assert.That(result148[0].BooleanField, Is.True);
      Assert.That(result148[0].Int16Field, Is.EqualTo(148));
      Assert.That(result148[0].Int32Field, Is.EqualTo(148));
      Assert.That(result148[0].Int64Field, Is.EqualTo(148));
      Assert.That(result148[0].FloatField, Is.EqualTo((float)148));
      Assert.That(result148[0].DoubleField, Is.EqualTo((double)148));
      Assert.That(result148[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result148[0].StringField, Is.EqualTo("TestEntity148"));
      Assert.That(result148[0].Text, Is.EqualTo("This is an instance of TestEntity148"));

      var result149 = session.Query.All<TestEntity149>().ToArray();
      Assert.That(result149.Length, Is.EqualTo(1));
      Assert.That(result149[0].BooleanField, Is.True);
      Assert.That(result149[0].Int16Field, Is.EqualTo(149));
      Assert.That(result149[0].Int32Field, Is.EqualTo(149));
      Assert.That(result149[0].Int64Field, Is.EqualTo(149));
      Assert.That(result149[0].FloatField, Is.EqualTo((float)149));
      Assert.That(result149[0].DoubleField, Is.EqualTo((double)149));
      Assert.That(result149[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result149[0].StringField, Is.EqualTo("TestEntity149"));
      Assert.That(result149[0].Text, Is.EqualTo("This is an instance of TestEntity149"));

      var result150 = session.Query.All<TestEntity150>().ToArray();
      Assert.That(result150.Length, Is.EqualTo(1));
      Assert.That(result150[0].BooleanField, Is.True);
      Assert.That(result150[0].Int16Field, Is.EqualTo(150));
      Assert.That(result150[0].Int32Field, Is.EqualTo(150));
      Assert.That(result150[0].Int64Field, Is.EqualTo(150));
      Assert.That(result150[0].FloatField, Is.EqualTo((float)150));
      Assert.That(result150[0].DoubleField, Is.EqualTo((double)150));
      Assert.That(result150[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result150[0].StringField, Is.EqualTo("TestEntity150"));
      Assert.That(result150[0].Text, Is.EqualTo("This is an instance of TestEntity150"));

      var result151 = session.Query.All<TestEntity151>().ToArray();
      Assert.That(result151.Length, Is.EqualTo(1));
      Assert.That(result151[0].BooleanField, Is.True);
      Assert.That(result151[0].Int16Field, Is.EqualTo(151));
      Assert.That(result151[0].Int32Field, Is.EqualTo(151));
      Assert.That(result151[0].Int64Field, Is.EqualTo(151));
      Assert.That(result151[0].FloatField, Is.EqualTo((float)151));
      Assert.That(result151[0].DoubleField, Is.EqualTo((double)151));
      Assert.That(result151[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result151[0].StringField, Is.EqualTo("TestEntity151"));
      Assert.That(result151[0].Text, Is.EqualTo("This is an instance of TestEntity151"));

      var result152 = session.Query.All<TestEntity152>().ToArray();
      Assert.That(result152.Length, Is.EqualTo(1));
      Assert.That(result152[0].BooleanField, Is.True);
      Assert.That(result152[0].Int16Field, Is.EqualTo(152));
      Assert.That(result152[0].Int32Field, Is.EqualTo(152));
      Assert.That(result152[0].Int64Field, Is.EqualTo(152));
      Assert.That(result152[0].FloatField, Is.EqualTo((float)152));
      Assert.That(result152[0].DoubleField, Is.EqualTo((double)152));
      Assert.That(result152[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result152[0].StringField, Is.EqualTo("TestEntity152"));
      Assert.That(result152[0].Text, Is.EqualTo("This is an instance of TestEntity152"));

      var result153 = session.Query.All<TestEntity153>().ToArray();
      Assert.That(result153.Length, Is.EqualTo(1));
      Assert.That(result153[0].BooleanField, Is.True);
      Assert.That(result153[0].Int16Field, Is.EqualTo(153));
      Assert.That(result153[0].Int32Field, Is.EqualTo(153));
      Assert.That(result153[0].Int64Field, Is.EqualTo(153));
      Assert.That(result153[0].FloatField, Is.EqualTo((float)153));
      Assert.That(result153[0].DoubleField, Is.EqualTo((double)153));
      Assert.That(result153[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result153[0].StringField, Is.EqualTo("TestEntity153"));
      Assert.That(result153[0].Text, Is.EqualTo("This is an instance of TestEntity153"));

      var result154 = session.Query.All<TestEntity154>().ToArray();
      Assert.That(result154.Length, Is.EqualTo(1));
      Assert.That(result154[0].BooleanField, Is.True);
      Assert.That(result154[0].Int16Field, Is.EqualTo(154));
      Assert.That(result154[0].Int32Field, Is.EqualTo(154));
      Assert.That(result154[0].Int64Field, Is.EqualTo(154));
      Assert.That(result154[0].FloatField, Is.EqualTo((float)154));
      Assert.That(result154[0].DoubleField, Is.EqualTo((double)154));
      Assert.That(result154[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result154[0].StringField, Is.EqualTo("TestEntity154"));
      Assert.That(result154[0].Text, Is.EqualTo("This is an instance of TestEntity154"));

      var result155 = session.Query.All<TestEntity155>().ToArray();
      Assert.That(result155.Length, Is.EqualTo(1));
      Assert.That(result155[0].BooleanField, Is.True);
      Assert.That(result155[0].Int16Field, Is.EqualTo(155));
      Assert.That(result155[0].Int32Field, Is.EqualTo(155));
      Assert.That(result155[0].Int64Field, Is.EqualTo(155));
      Assert.That(result155[0].FloatField, Is.EqualTo((float)155));
      Assert.That(result155[0].DoubleField, Is.EqualTo((double)155));
      Assert.That(result155[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result155[0].StringField, Is.EqualTo("TestEntity155"));
      Assert.That(result155[0].Text, Is.EqualTo("This is an instance of TestEntity155"));

      var result156 = session.Query.All<TestEntity156>().ToArray();
      Assert.That(result156.Length, Is.EqualTo(1));
      Assert.That(result156[0].BooleanField, Is.True);
      Assert.That(result156[0].Int16Field, Is.EqualTo(156));
      Assert.That(result156[0].Int32Field, Is.EqualTo(156));
      Assert.That(result156[0].Int64Field, Is.EqualTo(156));
      Assert.That(result156[0].FloatField, Is.EqualTo((float)156));
      Assert.That(result156[0].DoubleField, Is.EqualTo((double)156));
      Assert.That(result156[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result156[0].StringField, Is.EqualTo("TestEntity156"));
      Assert.That(result156[0].Text, Is.EqualTo("This is an instance of TestEntity156"));

      var result157 = session.Query.All<TestEntity157>().ToArray();
      Assert.That(result157.Length, Is.EqualTo(1));
      Assert.That(result157[0].BooleanField, Is.True);
      Assert.That(result157[0].Int16Field, Is.EqualTo(157));
      Assert.That(result157[0].Int32Field, Is.EqualTo(157));
      Assert.That(result157[0].Int64Field, Is.EqualTo(157));
      Assert.That(result157[0].FloatField, Is.EqualTo((float)157));
      Assert.That(result157[0].DoubleField, Is.EqualTo((double)157));
      Assert.That(result157[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result157[0].StringField, Is.EqualTo("TestEntity157"));
      Assert.That(result157[0].Text, Is.EqualTo("This is an instance of TestEntity157"));

      var result158 = session.Query.All<TestEntity158>().ToArray();
      Assert.That(result158.Length, Is.EqualTo(1));
      Assert.That(result158[0].BooleanField, Is.True);
      Assert.That(result158[0].Int16Field, Is.EqualTo(158));
      Assert.That(result158[0].Int32Field, Is.EqualTo(158));
      Assert.That(result158[0].Int64Field, Is.EqualTo(158));
      Assert.That(result158[0].FloatField, Is.EqualTo((float)158));
      Assert.That(result158[0].DoubleField, Is.EqualTo((double)158));
      Assert.That(result158[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result158[0].StringField, Is.EqualTo("TestEntity158"));
      Assert.That(result158[0].Text, Is.EqualTo("This is an instance of TestEntity158"));

      var result159 = session.Query.All<TestEntity159>().ToArray();
      Assert.That(result159.Length, Is.EqualTo(1));
      Assert.That(result159[0].BooleanField, Is.True);
      Assert.That(result159[0].Int16Field, Is.EqualTo(159));
      Assert.That(result159[0].Int32Field, Is.EqualTo(159));
      Assert.That(result159[0].Int64Field, Is.EqualTo(159));
      Assert.That(result159[0].FloatField, Is.EqualTo((float)159));
      Assert.That(result159[0].DoubleField, Is.EqualTo((double)159));
      Assert.That(result159[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result159[0].StringField, Is.EqualTo("TestEntity159"));
      Assert.That(result159[0].Text, Is.EqualTo("This is an instance of TestEntity159"));

      var result160 = session.Query.All<TestEntity160>().ToArray();
      Assert.That(result160.Length, Is.EqualTo(1));
      Assert.That(result160[0].BooleanField, Is.True);
      Assert.That(result160[0].Int16Field, Is.EqualTo(160));
      Assert.That(result160[0].Int32Field, Is.EqualTo(160));
      Assert.That(result160[0].Int64Field, Is.EqualTo(160));
      Assert.That(result160[0].FloatField, Is.EqualTo((float)160));
      Assert.That(result160[0].DoubleField, Is.EqualTo((double)160));
      Assert.That(result160[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result160[0].StringField, Is.EqualTo("TestEntity160"));
      Assert.That(result160[0].Text, Is.EqualTo("This is an instance of TestEntity160"));

      var result161 = session.Query.All<TestEntity161>().ToArray();
      Assert.That(result161.Length, Is.EqualTo(1));
      Assert.That(result161[0].BooleanField, Is.True);
      Assert.That(result161[0].Int16Field, Is.EqualTo(161));
      Assert.That(result161[0].Int32Field, Is.EqualTo(161));
      Assert.That(result161[0].Int64Field, Is.EqualTo(161));
      Assert.That(result161[0].FloatField, Is.EqualTo((float)161));
      Assert.That(result161[0].DoubleField, Is.EqualTo((double)161));
      Assert.That(result161[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result161[0].StringField, Is.EqualTo("TestEntity161"));
      Assert.That(result161[0].Text, Is.EqualTo("This is an instance of TestEntity161"));

      var result162 = session.Query.All<TestEntity162>().ToArray();
      Assert.That(result162.Length, Is.EqualTo(1));
      Assert.That(result162[0].BooleanField, Is.True);
      Assert.That(result162[0].Int16Field, Is.EqualTo(162));
      Assert.That(result162[0].Int32Field, Is.EqualTo(162));
      Assert.That(result162[0].Int64Field, Is.EqualTo(162));
      Assert.That(result162[0].FloatField, Is.EqualTo((float)162));
      Assert.That(result162[0].DoubleField, Is.EqualTo((double)162));
      Assert.That(result162[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result162[0].StringField, Is.EqualTo("TestEntity162"));
      Assert.That(result162[0].Text, Is.EqualTo("This is an instance of TestEntity162"));

      var result163 = session.Query.All<TestEntity163>().ToArray();
      Assert.That(result163.Length, Is.EqualTo(1));
      Assert.That(result163[0].BooleanField, Is.True);
      Assert.That(result163[0].Int16Field, Is.EqualTo(163));
      Assert.That(result163[0].Int32Field, Is.EqualTo(163));
      Assert.That(result163[0].Int64Field, Is.EqualTo(163));
      Assert.That(result163[0].FloatField, Is.EqualTo((float)163));
      Assert.That(result163[0].DoubleField, Is.EqualTo((double)163));
      Assert.That(result163[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result163[0].StringField, Is.EqualTo("TestEntity163"));
      Assert.That(result163[0].Text, Is.EqualTo("This is an instance of TestEntity163"));

      var result164 = session.Query.All<TestEntity164>().ToArray();
      Assert.That(result164.Length, Is.EqualTo(1));
      Assert.That(result164[0].BooleanField, Is.True);
      Assert.That(result164[0].Int16Field, Is.EqualTo(164));
      Assert.That(result164[0].Int32Field, Is.EqualTo(164));
      Assert.That(result164[0].Int64Field, Is.EqualTo(164));
      Assert.That(result164[0].FloatField, Is.EqualTo((float)164));
      Assert.That(result164[0].DoubleField, Is.EqualTo((double)164));
      Assert.That(result164[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result164[0].StringField, Is.EqualTo("TestEntity164"));
      Assert.That(result164[0].Text, Is.EqualTo("This is an instance of TestEntity164"));

      var result165 = session.Query.All<TestEntity165>().ToArray();
      Assert.That(result165.Length, Is.EqualTo(1));
      Assert.That(result165[0].BooleanField, Is.True);
      Assert.That(result165[0].Int16Field, Is.EqualTo(165));
      Assert.That(result165[0].Int32Field, Is.EqualTo(165));
      Assert.That(result165[0].Int64Field, Is.EqualTo(165));
      Assert.That(result165[0].FloatField, Is.EqualTo((float)165));
      Assert.That(result165[0].DoubleField, Is.EqualTo((double)165));
      Assert.That(result165[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result165[0].StringField, Is.EqualTo("TestEntity165"));
      Assert.That(result165[0].Text, Is.EqualTo("This is an instance of TestEntity165"));

      var result166 = session.Query.All<TestEntity166>().ToArray();
      Assert.That(result166.Length, Is.EqualTo(1));
      Assert.That(result166[0].BooleanField, Is.True);
      Assert.That(result166[0].Int16Field, Is.EqualTo(166));
      Assert.That(result166[0].Int32Field, Is.EqualTo(166));
      Assert.That(result166[0].Int64Field, Is.EqualTo(166));
      Assert.That(result166[0].FloatField, Is.EqualTo((float)166));
      Assert.That(result166[0].DoubleField, Is.EqualTo((double)166));
      Assert.That(result166[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result166[0].StringField, Is.EqualTo("TestEntity166"));
      Assert.That(result166[0].Text, Is.EqualTo("This is an instance of TestEntity166"));

      var result167 = session.Query.All<TestEntity167>().ToArray();
      Assert.That(result167.Length, Is.EqualTo(1));
      Assert.That(result167[0].BooleanField, Is.True);
      Assert.That(result167[0].Int16Field, Is.EqualTo(167));
      Assert.That(result167[0].Int32Field, Is.EqualTo(167));
      Assert.That(result167[0].Int64Field, Is.EqualTo(167));
      Assert.That(result167[0].FloatField, Is.EqualTo((float)167));
      Assert.That(result167[0].DoubleField, Is.EqualTo((double)167));
      Assert.That(result167[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result167[0].StringField, Is.EqualTo("TestEntity167"));
      Assert.That(result167[0].Text, Is.EqualTo("This is an instance of TestEntity167"));

      var result168 = session.Query.All<TestEntity168>().ToArray();
      Assert.That(result168.Length, Is.EqualTo(1));
      Assert.That(result168[0].BooleanField, Is.True);
      Assert.That(result168[0].Int16Field, Is.EqualTo(168));
      Assert.That(result168[0].Int32Field, Is.EqualTo(168));
      Assert.That(result168[0].Int64Field, Is.EqualTo(168));
      Assert.That(result168[0].FloatField, Is.EqualTo((float)168));
      Assert.That(result168[0].DoubleField, Is.EqualTo((double)168));
      Assert.That(result168[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result168[0].StringField, Is.EqualTo("TestEntity168"));
      Assert.That(result168[0].Text, Is.EqualTo("This is an instance of TestEntity168"));

      var result169 = session.Query.All<TestEntity169>().ToArray();
      Assert.That(result169.Length, Is.EqualTo(1));
      Assert.That(result169[0].BooleanField, Is.True);
      Assert.That(result169[0].Int16Field, Is.EqualTo(169));
      Assert.That(result169[0].Int32Field, Is.EqualTo(169));
      Assert.That(result169[0].Int64Field, Is.EqualTo(169));
      Assert.That(result169[0].FloatField, Is.EqualTo((float)169));
      Assert.That(result169[0].DoubleField, Is.EqualTo((double)169));
      Assert.That(result169[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result169[0].StringField, Is.EqualTo("TestEntity169"));
      Assert.That(result169[0].Text, Is.EqualTo("This is an instance of TestEntity169"));

      var result170 = session.Query.All<TestEntity170>().ToArray();
      Assert.That(result170.Length, Is.EqualTo(1));
      Assert.That(result170[0].BooleanField, Is.True);
      Assert.That(result170[0].Int16Field, Is.EqualTo(170));
      Assert.That(result170[0].Int32Field, Is.EqualTo(170));
      Assert.That(result170[0].Int64Field, Is.EqualTo(170));
      Assert.That(result170[0].FloatField, Is.EqualTo((float)170));
      Assert.That(result170[0].DoubleField, Is.EqualTo((double)170));
      Assert.That(result170[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result170[0].StringField, Is.EqualTo("TestEntity170"));
      Assert.That(result170[0].Text, Is.EqualTo("This is an instance of TestEntity170"));

      var result171 = session.Query.All<TestEntity171>().ToArray();
      Assert.That(result171.Length, Is.EqualTo(1));
      Assert.That(result171[0].BooleanField, Is.True);
      Assert.That(result171[0].Int16Field, Is.EqualTo(171));
      Assert.That(result171[0].Int32Field, Is.EqualTo(171));
      Assert.That(result171[0].Int64Field, Is.EqualTo(171));
      Assert.That(result171[0].FloatField, Is.EqualTo((float)171));
      Assert.That(result171[0].DoubleField, Is.EqualTo((double)171));
      Assert.That(result171[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result171[0].StringField, Is.EqualTo("TestEntity171"));
      Assert.That(result171[0].Text, Is.EqualTo("This is an instance of TestEntity171"));

      var result172 = session.Query.All<TestEntity172>().ToArray();
      Assert.That(result172.Length, Is.EqualTo(1));
      Assert.That(result172[0].BooleanField, Is.True);
      Assert.That(result172[0].Int16Field, Is.EqualTo(172));
      Assert.That(result172[0].Int32Field, Is.EqualTo(172));
      Assert.That(result172[0].Int64Field, Is.EqualTo(172));
      Assert.That(result172[0].FloatField, Is.EqualTo((float)172));
      Assert.That(result172[0].DoubleField, Is.EqualTo((double)172));
      Assert.That(result172[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result172[0].StringField, Is.EqualTo("TestEntity172"));
      Assert.That(result172[0].Text, Is.EqualTo("This is an instance of TestEntity172"));

      var result173 = session.Query.All<TestEntity173>().ToArray();
      Assert.That(result173.Length, Is.EqualTo(1));
      Assert.That(result173[0].BooleanField, Is.True);
      Assert.That(result173[0].Int16Field, Is.EqualTo(173));
      Assert.That(result173[0].Int32Field, Is.EqualTo(173));
      Assert.That(result173[0].Int64Field, Is.EqualTo(173));
      Assert.That(result173[0].FloatField, Is.EqualTo((float)173));
      Assert.That(result173[0].DoubleField, Is.EqualTo((double)173));
      Assert.That(result173[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result173[0].StringField, Is.EqualTo("TestEntity173"));
      Assert.That(result173[0].Text, Is.EqualTo("This is an instance of TestEntity173"));

      var result174 = session.Query.All<TestEntity174>().ToArray();
      Assert.That(result174.Length, Is.EqualTo(1));
      Assert.That(result174[0].BooleanField, Is.True);
      Assert.That(result174[0].Int16Field, Is.EqualTo(174));
      Assert.That(result174[0].Int32Field, Is.EqualTo(174));
      Assert.That(result174[0].Int64Field, Is.EqualTo(174));
      Assert.That(result174[0].FloatField, Is.EqualTo((float)174));
      Assert.That(result174[0].DoubleField, Is.EqualTo((double)174));
      Assert.That(result174[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result174[0].StringField, Is.EqualTo("TestEntity174"));
      Assert.That(result174[0].Text, Is.EqualTo("This is an instance of TestEntity174"));

      var result175 = session.Query.All<TestEntity175>().ToArray();
      Assert.That(result175.Length, Is.EqualTo(1));
      Assert.That(result175[0].BooleanField, Is.True);
      Assert.That(result175[0].Int16Field, Is.EqualTo(175));
      Assert.That(result175[0].Int32Field, Is.EqualTo(175));
      Assert.That(result175[0].Int64Field, Is.EqualTo(175));
      Assert.That(result175[0].FloatField, Is.EqualTo((float)175));
      Assert.That(result175[0].DoubleField, Is.EqualTo((double)175));
      Assert.That(result175[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result175[0].StringField, Is.EqualTo("TestEntity175"));
      Assert.That(result175[0].Text, Is.EqualTo("This is an instance of TestEntity175"));

      var result176 = session.Query.All<TestEntity176>().ToArray();
      Assert.That(result176.Length, Is.EqualTo(1));
      Assert.That(result176[0].BooleanField, Is.True);
      Assert.That(result176[0].Int16Field, Is.EqualTo(176));
      Assert.That(result176[0].Int32Field, Is.EqualTo(176));
      Assert.That(result176[0].Int64Field, Is.EqualTo(176));
      Assert.That(result176[0].FloatField, Is.EqualTo((float)176));
      Assert.That(result176[0].DoubleField, Is.EqualTo((double)176));
      Assert.That(result176[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result176[0].StringField, Is.EqualTo("TestEntity176"));
      Assert.That(result176[0].Text, Is.EqualTo("This is an instance of TestEntity176"));

      var result177 = session.Query.All<TestEntity177>().ToArray();
      Assert.That(result177.Length, Is.EqualTo(1));
      Assert.That(result177[0].BooleanField, Is.True);
      Assert.That(result177[0].Int16Field, Is.EqualTo(177));
      Assert.That(result177[0].Int32Field, Is.EqualTo(177));
      Assert.That(result177[0].Int64Field, Is.EqualTo(177));
      Assert.That(result177[0].FloatField, Is.EqualTo((float)177));
      Assert.That(result177[0].DoubleField, Is.EqualTo((double)177));
      Assert.That(result177[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result177[0].StringField, Is.EqualTo("TestEntity177"));
      Assert.That(result177[0].Text, Is.EqualTo("This is an instance of TestEntity177"));

      var result178 = session.Query.All<TestEntity178>().ToArray();
      Assert.That(result178.Length, Is.EqualTo(1));
      Assert.That(result178[0].BooleanField, Is.True);
      Assert.That(result178[0].Int16Field, Is.EqualTo(178));
      Assert.That(result178[0].Int32Field, Is.EqualTo(178));
      Assert.That(result178[0].Int64Field, Is.EqualTo(178));
      Assert.That(result178[0].FloatField, Is.EqualTo((float)178));
      Assert.That(result178[0].DoubleField, Is.EqualTo((double)178));
      Assert.That(result178[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result178[0].StringField, Is.EqualTo("TestEntity178"));
      Assert.That(result178[0].Text, Is.EqualTo("This is an instance of TestEntity178"));

      var result179 = session.Query.All<TestEntity179>().ToArray();
      Assert.That(result179.Length, Is.EqualTo(1));
      Assert.That(result179[0].BooleanField, Is.True);
      Assert.That(result179[0].Int16Field, Is.EqualTo(179));
      Assert.That(result179[0].Int32Field, Is.EqualTo(179));
      Assert.That(result179[0].Int64Field, Is.EqualTo(179));
      Assert.That(result179[0].FloatField, Is.EqualTo((float)179));
      Assert.That(result179[0].DoubleField, Is.EqualTo((double)179));
      Assert.That(result179[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result179[0].StringField, Is.EqualTo("TestEntity179"));
      Assert.That(result179[0].Text, Is.EqualTo("This is an instance of TestEntity179"));

      var result180 = session.Query.All<TestEntity180>().ToArray();
      Assert.That(result180.Length, Is.EqualTo(1));
      Assert.That(result180[0].BooleanField, Is.True);
      Assert.That(result180[0].Int16Field, Is.EqualTo(180));
      Assert.That(result180[0].Int32Field, Is.EqualTo(180));
      Assert.That(result180[0].Int64Field, Is.EqualTo(180));
      Assert.That(result180[0].FloatField, Is.EqualTo((float)180));
      Assert.That(result180[0].DoubleField, Is.EqualTo((double)180));
      Assert.That(result180[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result180[0].StringField, Is.EqualTo("TestEntity180"));
      Assert.That(result180[0].Text, Is.EqualTo("This is an instance of TestEntity180"));

      var result181 = session.Query.All<TestEntity181>().ToArray();
      Assert.That(result181.Length, Is.EqualTo(1));
      Assert.That(result181[0].BooleanField, Is.True);
      Assert.That(result181[0].Int16Field, Is.EqualTo(181));
      Assert.That(result181[0].Int32Field, Is.EqualTo(181));
      Assert.That(result181[0].Int64Field, Is.EqualTo(181));
      Assert.That(result181[0].FloatField, Is.EqualTo((float)181));
      Assert.That(result181[0].DoubleField, Is.EqualTo((double)181));
      Assert.That(result181[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result181[0].StringField, Is.EqualTo("TestEntity181"));
      Assert.That(result181[0].Text, Is.EqualTo("This is an instance of TestEntity181"));

      var result182 = session.Query.All<TestEntity182>().ToArray();
      Assert.That(result182.Length, Is.EqualTo(1));
      Assert.That(result182[0].BooleanField, Is.True);
      Assert.That(result182[0].Int16Field, Is.EqualTo(182));
      Assert.That(result182[0].Int32Field, Is.EqualTo(182));
      Assert.That(result182[0].Int64Field, Is.EqualTo(182));
      Assert.That(result182[0].FloatField, Is.EqualTo((float)182));
      Assert.That(result182[0].DoubleField, Is.EqualTo((double)182));
      Assert.That(result182[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result182[0].StringField, Is.EqualTo("TestEntity182"));
      Assert.That(result182[0].Text, Is.EqualTo("This is an instance of TestEntity182"));

      var result183 = session.Query.All<TestEntity183>().ToArray();
      Assert.That(result183.Length, Is.EqualTo(1));
      Assert.That(result183[0].BooleanField, Is.True);
      Assert.That(result183[0].Int16Field, Is.EqualTo(183));
      Assert.That(result183[0].Int32Field, Is.EqualTo(183));
      Assert.That(result183[0].Int64Field, Is.EqualTo(183));
      Assert.That(result183[0].FloatField, Is.EqualTo((float)183));
      Assert.That(result183[0].DoubleField, Is.EqualTo((double)183));
      Assert.That(result183[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result183[0].StringField, Is.EqualTo("TestEntity183"));
      Assert.That(result183[0].Text, Is.EqualTo("This is an instance of TestEntity183"));

      var result184 = session.Query.All<TestEntity184>().ToArray();
      Assert.That(result184.Length, Is.EqualTo(1));
      Assert.That(result184[0].BooleanField, Is.True);
      Assert.That(result184[0].Int16Field, Is.EqualTo(184));
      Assert.That(result184[0].Int32Field, Is.EqualTo(184));
      Assert.That(result184[0].Int64Field, Is.EqualTo(184));
      Assert.That(result184[0].FloatField, Is.EqualTo((float)184));
      Assert.That(result184[0].DoubleField, Is.EqualTo((double)184));
      Assert.That(result184[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result184[0].StringField, Is.EqualTo("TestEntity184"));
      Assert.That(result184[0].Text, Is.EqualTo("This is an instance of TestEntity184"));

      var result185 = session.Query.All<TestEntity185>().ToArray();
      Assert.That(result185.Length, Is.EqualTo(1));
      Assert.That(result185[0].BooleanField, Is.True);
      Assert.That(result185[0].Int16Field, Is.EqualTo(185));
      Assert.That(result185[0].Int32Field, Is.EqualTo(185));
      Assert.That(result185[0].Int64Field, Is.EqualTo(185));
      Assert.That(result185[0].FloatField, Is.EqualTo((float)185));
      Assert.That(result185[0].DoubleField, Is.EqualTo((double)185));
      Assert.That(result185[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result185[0].StringField, Is.EqualTo("TestEntity185"));
      Assert.That(result185[0].Text, Is.EqualTo("This is an instance of TestEntity185"));

      var result186 = session.Query.All<TestEntity186>().ToArray();
      Assert.That(result186.Length, Is.EqualTo(1));
      Assert.That(result186[0].BooleanField, Is.True);
      Assert.That(result186[0].Int16Field, Is.EqualTo(186));
      Assert.That(result186[0].Int32Field, Is.EqualTo(186));
      Assert.That(result186[0].Int64Field, Is.EqualTo(186));
      Assert.That(result186[0].FloatField, Is.EqualTo((float)186));
      Assert.That(result186[0].DoubleField, Is.EqualTo((double)186));
      Assert.That(result186[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result186[0].StringField, Is.EqualTo("TestEntity186"));
      Assert.That(result186[0].Text, Is.EqualTo("This is an instance of TestEntity186"));

      var result187 = session.Query.All<TestEntity187>().ToArray();
      Assert.That(result187.Length, Is.EqualTo(1));
      Assert.That(result187[0].BooleanField, Is.True);
      Assert.That(result187[0].Int16Field, Is.EqualTo(187));
      Assert.That(result187[0].Int32Field, Is.EqualTo(187));
      Assert.That(result187[0].Int64Field, Is.EqualTo(187));
      Assert.That(result187[0].FloatField, Is.EqualTo((float)187));
      Assert.That(result187[0].DoubleField, Is.EqualTo((double)187));
      Assert.That(result187[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result187[0].StringField, Is.EqualTo("TestEntity187"));
      Assert.That(result187[0].Text, Is.EqualTo("This is an instance of TestEntity187"));

      var result188 = session.Query.All<TestEntity188>().ToArray();
      Assert.That(result188.Length, Is.EqualTo(1));
      Assert.That(result188[0].BooleanField, Is.True);
      Assert.That(result188[0].Int16Field, Is.EqualTo(188));
      Assert.That(result188[0].Int32Field, Is.EqualTo(188));
      Assert.That(result188[0].Int64Field, Is.EqualTo(188));
      Assert.That(result188[0].FloatField, Is.EqualTo((float)188));
      Assert.That(result188[0].DoubleField, Is.EqualTo((double)188));
      Assert.That(result188[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result188[0].StringField, Is.EqualTo("TestEntity188"));
      Assert.That(result188[0].Text, Is.EqualTo("This is an instance of TestEntity188"));

      var result189 = session.Query.All<TestEntity189>().ToArray();
      Assert.That(result189.Length, Is.EqualTo(1));
      Assert.That(result189[0].BooleanField, Is.True);
      Assert.That(result189[0].Int16Field, Is.EqualTo(189));
      Assert.That(result189[0].Int32Field, Is.EqualTo(189));
      Assert.That(result189[0].Int64Field, Is.EqualTo(189));
      Assert.That(result189[0].FloatField, Is.EqualTo((float)189));
      Assert.That(result189[0].DoubleField, Is.EqualTo((double)189));
      Assert.That(result189[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result189[0].StringField, Is.EqualTo("TestEntity189"));
      Assert.That(result189[0].Text, Is.EqualTo("This is an instance of TestEntity189"));

      var result190 = session.Query.All<TestEntity190>().ToArray();
      Assert.That(result190.Length, Is.EqualTo(1));
      Assert.That(result190[0].BooleanField, Is.True);
      Assert.That(result190[0].Int16Field, Is.EqualTo(190));
      Assert.That(result190[0].Int32Field, Is.EqualTo(190));
      Assert.That(result190[0].Int64Field, Is.EqualTo(190));
      Assert.That(result190[0].FloatField, Is.EqualTo((float)190));
      Assert.That(result190[0].DoubleField, Is.EqualTo((double)190));
      Assert.That(result190[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result190[0].StringField, Is.EqualTo("TestEntity190"));
      Assert.That(result190[0].Text, Is.EqualTo("This is an instance of TestEntity190"));

      var result191 = session.Query.All<TestEntity191>().ToArray();
      Assert.That(result191.Length, Is.EqualTo(1));
      Assert.That(result191[0].BooleanField, Is.True);
      Assert.That(result191[0].Int16Field, Is.EqualTo(191));
      Assert.That(result191[0].Int32Field, Is.EqualTo(191));
      Assert.That(result191[0].Int64Field, Is.EqualTo(191));
      Assert.That(result191[0].FloatField, Is.EqualTo((float)191));
      Assert.That(result191[0].DoubleField, Is.EqualTo((double)191));
      Assert.That(result191[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result191[0].StringField, Is.EqualTo("TestEntity191"));
      Assert.That(result191[0].Text, Is.EqualTo("This is an instance of TestEntity191"));

      var result192 = session.Query.All<TestEntity192>().ToArray();
      Assert.That(result192.Length, Is.EqualTo(1));
      Assert.That(result192[0].BooleanField, Is.True);
      Assert.That(result192[0].Int16Field, Is.EqualTo(192));
      Assert.That(result192[0].Int32Field, Is.EqualTo(192));
      Assert.That(result192[0].Int64Field, Is.EqualTo(192));
      Assert.That(result192[0].FloatField, Is.EqualTo((float)192));
      Assert.That(result192[0].DoubleField, Is.EqualTo((double)192));
      Assert.That(result192[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result192[0].StringField, Is.EqualTo("TestEntity192"));
      Assert.That(result192[0].Text, Is.EqualTo("This is an instance of TestEntity192"));

      var result193 = session.Query.All<TestEntity193>().ToArray();
      Assert.That(result193.Length, Is.EqualTo(1));
      Assert.That(result193[0].BooleanField, Is.True);
      Assert.That(result193[0].Int16Field, Is.EqualTo(193));
      Assert.That(result193[0].Int32Field, Is.EqualTo(193));
      Assert.That(result193[0].Int64Field, Is.EqualTo(193));
      Assert.That(result193[0].FloatField, Is.EqualTo((float)193));
      Assert.That(result193[0].DoubleField, Is.EqualTo((double)193));
      Assert.That(result193[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result193[0].StringField, Is.EqualTo("TestEntity193"));
      Assert.That(result193[0].Text, Is.EqualTo("This is an instance of TestEntity193"));

      var result194 = session.Query.All<TestEntity194>().ToArray();
      Assert.That(result194.Length, Is.EqualTo(1));
      Assert.That(result194[0].BooleanField, Is.True);
      Assert.That(result194[0].Int16Field, Is.EqualTo(194));
      Assert.That(result194[0].Int32Field, Is.EqualTo(194));
      Assert.That(result194[0].Int64Field, Is.EqualTo(194));
      Assert.That(result194[0].FloatField, Is.EqualTo((float)194));
      Assert.That(result194[0].DoubleField, Is.EqualTo((double)194));
      Assert.That(result194[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result194[0].StringField, Is.EqualTo("TestEntity194"));
      Assert.That(result194[0].Text, Is.EqualTo("This is an instance of TestEntity194"));

      var result195 = session.Query.All<TestEntity195>().ToArray();
      Assert.That(result195.Length, Is.EqualTo(1));
      Assert.That(result195[0].BooleanField, Is.True);
      Assert.That(result195[0].Int16Field, Is.EqualTo(195));
      Assert.That(result195[0].Int32Field, Is.EqualTo(195));
      Assert.That(result195[0].Int64Field, Is.EqualTo(195));
      Assert.That(result195[0].FloatField, Is.EqualTo((float)195));
      Assert.That(result195[0].DoubleField, Is.EqualTo((double)195));
      Assert.That(result195[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result195[0].StringField, Is.EqualTo("TestEntity195"));
      Assert.That(result195[0].Text, Is.EqualTo("This is an instance of TestEntity195"));

      var result196 = session.Query.All<TestEntity196>().ToArray();
      Assert.That(result196.Length, Is.EqualTo(1));
      Assert.That(result196[0].BooleanField, Is.True);
      Assert.That(result196[0].Int16Field, Is.EqualTo(196));
      Assert.That(result196[0].Int32Field, Is.EqualTo(196));
      Assert.That(result196[0].Int64Field, Is.EqualTo(196));
      Assert.That(result196[0].FloatField, Is.EqualTo((float)196));
      Assert.That(result196[0].DoubleField, Is.EqualTo((double)196));
      Assert.That(result196[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result196[0].StringField, Is.EqualTo("TestEntity196"));
      Assert.That(result196[0].Text, Is.EqualTo("This is an instance of TestEntity196"));

      var result197 = session.Query.All<TestEntity197>().ToArray();
      Assert.That(result197.Length, Is.EqualTo(1));
      Assert.That(result197[0].BooleanField, Is.True);
      Assert.That(result197[0].Int16Field, Is.EqualTo(197));
      Assert.That(result197[0].Int32Field, Is.EqualTo(197));
      Assert.That(result197[0].Int64Field, Is.EqualTo(197));
      Assert.That(result197[0].FloatField, Is.EqualTo((float)197));
      Assert.That(result197[0].DoubleField, Is.EqualTo((double)197));
      Assert.That(result197[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result197[0].StringField, Is.EqualTo("TestEntity197"));
      Assert.That(result197[0].Text, Is.EqualTo("This is an instance of TestEntity197"));

      var result198 = session.Query.All<TestEntity198>().ToArray();
      Assert.That(result198.Length, Is.EqualTo(1));
      Assert.That(result198[0].BooleanField, Is.True);
      Assert.That(result198[0].Int16Field, Is.EqualTo(198));
      Assert.That(result198[0].Int32Field, Is.EqualTo(198));
      Assert.That(result198[0].Int64Field, Is.EqualTo(198));
      Assert.That(result198[0].FloatField, Is.EqualTo((float)198));
      Assert.That(result198[0].DoubleField, Is.EqualTo((double)198));
      Assert.That(result198[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result198[0].StringField, Is.EqualTo("TestEntity198"));
      Assert.That(result198[0].Text, Is.EqualTo("This is an instance of TestEntity198"));

      var result199 = session.Query.All<TestEntity199>().ToArray();
      Assert.That(result199.Length, Is.EqualTo(1));
      Assert.That(result199[0].BooleanField, Is.True);
      Assert.That(result199[0].Int16Field, Is.EqualTo(199));
      Assert.That(result199[0].Int32Field, Is.EqualTo(199));
      Assert.That(result199[0].Int64Field, Is.EqualTo(199));
      Assert.That(result199[0].FloatField, Is.EqualTo((float)199));
      Assert.That(result199[0].DoubleField, Is.EqualTo((double)199));
      Assert.That(result199[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result199[0].StringField, Is.EqualTo("TestEntity199"));
      Assert.That(result199[0].Text, Is.EqualTo("This is an instance of TestEntity199"));

    }
  }

}
