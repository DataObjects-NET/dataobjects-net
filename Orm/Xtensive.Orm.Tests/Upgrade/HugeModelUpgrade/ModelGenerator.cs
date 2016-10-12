using System;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.Model
{
  [HierarchyRoot]
  [Index("Int16Field")]
  [Index("Int32Field")]
  [Index("Int64Field")]
  [Index("FloatField")]
  [Index("DoubleField")]
  public class TestEntity0 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity1 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity2 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity3 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity4 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity5 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity6 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity7 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity8 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity9 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity10 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity11 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity12 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity13 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity14 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity15 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity16 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity17 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity18 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity19 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity20 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity21 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity22 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity23 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity24 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity25 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity26 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity27 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity28 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity29 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity30 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity31 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity32 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity33 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity34 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity35 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity36 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity37 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity38 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity39 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity40 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity41 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity42 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity43 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity44 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity45 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity46 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity47 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity48 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity49 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity50 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity51 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity52 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity53 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity54 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity55 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity56 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity57 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity58 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity59 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity60 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity61 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity62 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity63 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity64 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity65 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity66 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity67 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity68 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity69 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity70 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity71 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity72 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity73 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity74 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity75 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity76 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity77 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity78 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity79 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity80 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity81 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity82 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity83 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity84 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity85 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity86 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity87 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity88 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity89 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity90 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity91 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity92 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity93 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity94 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity95 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity96 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity97 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity98 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity99 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity100 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity101 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity102 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity103 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity104 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity105 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity106 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity107 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity108 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity109 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity110 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity111 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity112 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity113 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity114 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity115 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity116 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity117 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity118 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity119 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity120 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity121 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity122 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity123 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity124 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity125 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity126 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity127 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity128 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity129 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity130 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity131 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity132 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity133 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity134 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity135 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity136 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity137 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity138 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity139 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity140 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity141 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity142 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity143 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity144 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity145 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity146 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity147 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity148 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity149 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity150 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity151 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity152 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity153 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity154 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity155 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity156 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity157 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity158 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity159 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity160 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity161 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity162 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity163 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity164 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity165 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity166 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity167 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity168 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity169 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity170 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity171 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity172 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity173 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity174 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity175 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity176 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity177 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity178 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity179 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity180 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity181 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity182 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity183 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity184 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity185 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity186 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity187 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity188 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity189 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity190 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity191 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity192 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity193 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity194 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity195 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity196 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity197 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity198 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
  public class TestEntity199 : Entity {

    [Key, Field]
    public long Id{get;set;}

    [Field]
    public bool BooleanField {get;set;}

    [Field]
    public Int16 Int16Field {get;set;}

    [Field]
    public Int32 Int32Field {get;set;}

    [Field]
    public Int64 Int64Field {get;set;}

    [Field]
    public float FloatField {get;set;}

    [Field]
    public double DoubleField {get;set;}

    [Field]
    public DateTime DateTimeField {get;set;}

    [Field]
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
}
