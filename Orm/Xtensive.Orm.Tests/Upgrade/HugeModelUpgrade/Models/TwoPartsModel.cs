using System;
using System.Linq;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.TwoPartsModel
{
  namespace PartOne
  {
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne0 : Entity {

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
    public class TestEntityOne1 : Entity {

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
      public TestEntityOne0 TestEntityOne0{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne2 : Entity {

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
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne3 : Entity {

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
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne4 : Entity {

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
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne5 : Entity {

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
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne6 : Entity {

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
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne7 : Entity {

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
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne8 : Entity {

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
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne9 : Entity {

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
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne10 : Entity {

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
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne11 : Entity {

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
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

      [Field]
      public TestEntityOne0 TestEntityOne0{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne12 : Entity {

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
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

      [Field]
      public TestEntityOne1 TestEntityOne1{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne13 : Entity {

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
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

      [Field]
      public TestEntityOne2 TestEntityOne2{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne14 : Entity {

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
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

      [Field]
      public TestEntityOne3 TestEntityOne3{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne15 : Entity {

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
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

      [Field]
      public TestEntityOne4 TestEntityOne4{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne16 : Entity {

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
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

      [Field]
      public TestEntityOne5 TestEntityOne5{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne17 : Entity {

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
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

      [Field]
      public TestEntityOne6 TestEntityOne6{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne18 : Entity {

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
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

      [Field]
      public TestEntityOne7 TestEntityOne7{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne19 : Entity {

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
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

      [Field]
      public TestEntityOne8 TestEntityOne8{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne20 : Entity {

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
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

      [Field]
      public TestEntityOne9 TestEntityOne9{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne21 : Entity {

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
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

      [Field]
      public TestEntityOne10 TestEntityOne10{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne22 : Entity {

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
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

      [Field]
      public TestEntityOne11 TestEntityOne11{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne23 : Entity {

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
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

      [Field]
      public TestEntityOne12 TestEntityOne12{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne24 : Entity {

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
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

      [Field]
      public TestEntityOne13 TestEntityOne13{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne25 : Entity {

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
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

      [Field]
      public TestEntityOne14 TestEntityOne14{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne26 : Entity {

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
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

      [Field]
      public TestEntityOne15 TestEntityOne15{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne27 : Entity {

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
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

      [Field]
      public TestEntityOne16 TestEntityOne16{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne28 : Entity {

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
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

      [Field]
      public TestEntityOne17 TestEntityOne17{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne29 : Entity {

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
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

      [Field]
      public TestEntityOne18 TestEntityOne18{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne30 : Entity {

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
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

      [Field]
      public TestEntityOne19 TestEntityOne19{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne31 : Entity {

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
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

      [Field]
      public TestEntityOne20 TestEntityOne20{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne32 : Entity {

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
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

      [Field]
      public TestEntityOne21 TestEntityOne21{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne33 : Entity {

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
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

      [Field]
      public TestEntityOne22 TestEntityOne22{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne34 : Entity {

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
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

      [Field]
      public TestEntityOne23 TestEntityOne23{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne35 : Entity {

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
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

      [Field]
      public TestEntityOne24 TestEntityOne24{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne36 : Entity {

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
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

      [Field]
      public TestEntityOne25 TestEntityOne25{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne37 : Entity {

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
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

      [Field]
      public TestEntityOne26 TestEntityOne26{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne38 : Entity {

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
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

      [Field]
      public TestEntityOne27 TestEntityOne27{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne39 : Entity {

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
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

      [Field]
      public TestEntityOne28 TestEntityOne28{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne40 : Entity {

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
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

      [Field]
      public TestEntityOne29 TestEntityOne29{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne41 : Entity {

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
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

      [Field]
      public TestEntityOne30 TestEntityOne30{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne42 : Entity {

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
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

      [Field]
      public TestEntityOne31 TestEntityOne31{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne43 : Entity {

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
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

      [Field]
      public TestEntityOne32 TestEntityOne32{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne44 : Entity {

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
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

      [Field]
      public TestEntityOne33 TestEntityOne33{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne45 : Entity {

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
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

      [Field]
      public TestEntityOne34 TestEntityOne34{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne46 : Entity {

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
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

      [Field]
      public TestEntityOne35 TestEntityOne35{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne47 : Entity {

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
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

      [Field]
      public TestEntityOne36 TestEntityOne36{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne48 : Entity {

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
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

      [Field]
      public TestEntityOne37 TestEntityOne37{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne49 : Entity {

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
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

      [Field]
      public TestEntityOne38 TestEntityOne38{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne50 : Entity {

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
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

      [Field]
      public TestEntityOne39 TestEntityOne39{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne51 : Entity {

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
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

      [Field]
      public TestEntityOne40 TestEntityOne40{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne52 : Entity {

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
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

      [Field]
      public TestEntityOne41 TestEntityOne41{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne53 : Entity {

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
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

      [Field]
      public TestEntityOne42 TestEntityOne42{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne54 : Entity {

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
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

      [Field]
      public TestEntityOne43 TestEntityOne43{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne55 : Entity {

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
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

      [Field]
      public TestEntityOne44 TestEntityOne44{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne56 : Entity {

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
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

      [Field]
      public TestEntityOne45 TestEntityOne45{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne57 : Entity {

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
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

      [Field]
      public TestEntityOne46 TestEntityOne46{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne58 : Entity {

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
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

      [Field]
      public TestEntityOne47 TestEntityOne47{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne59 : Entity {

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
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

      [Field]
      public TestEntityOne48 TestEntityOne48{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne60 : Entity {

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
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

      [Field]
      public TestEntityOne49 TestEntityOne49{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne61 : Entity {

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
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

      [Field]
      public TestEntityOne50 TestEntityOne50{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne62 : Entity {

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
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

      [Field]
      public TestEntityOne51 TestEntityOne51{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne63 : Entity {

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
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

      [Field]
      public TestEntityOne52 TestEntityOne52{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne64 : Entity {

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
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

      [Field]
      public TestEntityOne53 TestEntityOne53{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne65 : Entity {

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
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

      [Field]
      public TestEntityOne54 TestEntityOne54{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne66 : Entity {

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
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

      [Field]
      public TestEntityOne55 TestEntityOne55{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne67 : Entity {

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
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

      [Field]
      public TestEntityOne56 TestEntityOne56{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne68 : Entity {

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
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

      [Field]
      public TestEntityOne57 TestEntityOne57{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne69 : Entity {

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
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

      [Field]
      public TestEntityOne58 TestEntityOne58{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne70 : Entity {

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
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

      [Field]
      public TestEntityOne59 TestEntityOne59{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne71 : Entity {

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
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

      [Field]
      public TestEntityOne60 TestEntityOne60{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne72 : Entity {

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
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

      [Field]
      public TestEntityOne61 TestEntityOne61{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne73 : Entity {

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
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

      [Field]
      public TestEntityOne62 TestEntityOne62{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne74 : Entity {

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
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

      [Field]
      public TestEntityOne63 TestEntityOne63{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne75 : Entity {

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
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

      [Field]
      public TestEntityOne64 TestEntityOne64{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne76 : Entity {

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
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

      [Field]
      public TestEntityOne65 TestEntityOne65{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne77 : Entity {

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
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

      [Field]
      public TestEntityOne66 TestEntityOne66{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne78 : Entity {

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
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

      [Field]
      public TestEntityOne67 TestEntityOne67{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne79 : Entity {

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
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

      [Field]
      public TestEntityOne68 TestEntityOne68{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne80 : Entity {

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
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

      [Field]
      public TestEntityOne69 TestEntityOne69{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne81 : Entity {

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
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

      [Field]
      public TestEntityOne70 TestEntityOne70{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne82 : Entity {

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
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

      [Field]
      public TestEntityOne71 TestEntityOne71{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne83 : Entity {

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
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

      [Field]
      public TestEntityOne72 TestEntityOne72{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne84 : Entity {

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
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

      [Field]
      public TestEntityOne73 TestEntityOne73{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne85 : Entity {

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
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

      [Field]
      public TestEntityOne74 TestEntityOne74{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne86 : Entity {

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
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

      [Field]
      public TestEntityOne75 TestEntityOne75{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne87 : Entity {

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
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

      [Field]
      public TestEntityOne76 TestEntityOne76{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne88 : Entity {

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
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

      [Field]
      public TestEntityOne77 TestEntityOne77{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne89 : Entity {

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
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

      [Field]
      public TestEntityOne78 TestEntityOne78{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne90 : Entity {

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
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

      [Field]
      public TestEntityOne79 TestEntityOne79{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne91 : Entity {

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
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

      [Field]
      public TestEntityOne80 TestEntityOne80{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne92 : Entity {

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
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

      [Field]
      public TestEntityOne81 TestEntityOne81{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne93 : Entity {

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
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

      [Field]
      public TestEntityOne82 TestEntityOne82{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne94 : Entity {

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
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

      [Field]
      public TestEntityOne83 TestEntityOne83{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne95 : Entity {

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
      public TestEntityOne94 TestEntityOne94{get;set;}

      [Field]
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

      [Field]
      public TestEntityOne84 TestEntityOne84{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne96 : Entity {

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
      public TestEntityOne95 TestEntityOne95{get;set;}

      [Field]
      public TestEntityOne94 TestEntityOne94{get;set;}

      [Field]
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

      [Field]
      public TestEntityOne85 TestEntityOne85{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne97 : Entity {

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
      public TestEntityOne96 TestEntityOne96{get;set;}

      [Field]
      public TestEntityOne95 TestEntityOne95{get;set;}

      [Field]
      public TestEntityOne94 TestEntityOne94{get;set;}

      [Field]
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

      [Field]
      public TestEntityOne86 TestEntityOne86{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne98 : Entity {

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
      public TestEntityOne97 TestEntityOne97{get;set;}

      [Field]
      public TestEntityOne96 TestEntityOne96{get;set;}

      [Field]
      public TestEntityOne95 TestEntityOne95{get;set;}

      [Field]
      public TestEntityOne94 TestEntityOne94{get;set;}

      [Field]
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

      [Field]
      public TestEntityOne87 TestEntityOne87{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityOne99 : Entity {

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
      public TestEntityOne98 TestEntityOne98{get;set;}

      [Field]
      public TestEntityOne97 TestEntityOne97{get;set;}

      [Field]
      public TestEntityOne96 TestEntityOne96{get;set;}

      [Field]
      public TestEntityOne95 TestEntityOne95{get;set;}

      [Field]
      public TestEntityOne94 TestEntityOne94{get;set;}

      [Field]
      public TestEntityOne93 TestEntityOne93{get;set;}

      [Field]
      public TestEntityOne92 TestEntityOne92{get;set;}

      [Field]
      public TestEntityOne91 TestEntityOne91{get;set;}

      [Field]
      public TestEntityOne90 TestEntityOne90{get;set;}

      [Field]
      public TestEntityOne89 TestEntityOne89{get;set;}

      [Field]
      public TestEntityOne88 TestEntityOne88{get;set;}

    }
  }

  namespace PartTwo
  {
      [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo0 : Entity {

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
    public class TestEntityTwo1 : Entity {

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
      public TestEntityTwo0 TestEntityTwo0{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo2 : Entity {

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
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo3 : Entity {

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
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo4 : Entity {

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
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo5 : Entity {

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
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo6 : Entity {

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
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo7 : Entity {

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
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo8 : Entity {

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
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo9 : Entity {

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
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo10 : Entity {

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
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}


    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo11 : Entity {

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
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

      [Field]
      public TestEntityTwo0 TestEntityTwo0{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo12 : Entity {

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
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

      [Field]
      public TestEntityTwo1 TestEntityTwo1{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo13 : Entity {

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
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

      [Field]
      public TestEntityTwo2 TestEntityTwo2{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo14 : Entity {

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
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

      [Field]
      public TestEntityTwo3 TestEntityTwo3{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo15 : Entity {

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
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

      [Field]
      public TestEntityTwo4 TestEntityTwo4{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo16 : Entity {

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
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

      [Field]
      public TestEntityTwo5 TestEntityTwo5{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo17 : Entity {

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
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

      [Field]
      public TestEntityTwo6 TestEntityTwo6{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo18 : Entity {

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
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

      [Field]
      public TestEntityTwo7 TestEntityTwo7{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo19 : Entity {

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
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

      [Field]
      public TestEntityTwo8 TestEntityTwo8{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo20 : Entity {

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
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

      [Field]
      public TestEntityTwo9 TestEntityTwo9{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo21 : Entity {

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
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

      [Field]
      public TestEntityTwo10 TestEntityTwo10{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo22 : Entity {

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
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

      [Field]
      public TestEntityTwo11 TestEntityTwo11{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo23 : Entity {

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
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

      [Field]
      public TestEntityTwo12 TestEntityTwo12{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo24 : Entity {

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
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

      [Field]
      public TestEntityTwo13 TestEntityTwo13{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo25 : Entity {

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
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

      [Field]
      public TestEntityTwo14 TestEntityTwo14{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo26 : Entity {

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
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

      [Field]
      public TestEntityTwo15 TestEntityTwo15{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo27 : Entity {

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
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

      [Field]
      public TestEntityTwo16 TestEntityTwo16{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo28 : Entity {

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
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

      [Field]
      public TestEntityTwo17 TestEntityTwo17{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo29 : Entity {

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
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

      [Field]
      public TestEntityTwo18 TestEntityTwo18{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo30 : Entity {

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
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

      [Field]
      public TestEntityTwo19 TestEntityTwo19{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo31 : Entity {

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
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

      [Field]
      public TestEntityTwo20 TestEntityTwo20{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo32 : Entity {

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
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

      [Field]
      public TestEntityTwo21 TestEntityTwo21{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo33 : Entity {

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
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

      [Field]
      public TestEntityTwo22 TestEntityTwo22{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo34 : Entity {

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
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

      [Field]
      public TestEntityTwo23 TestEntityTwo23{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo35 : Entity {

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
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

      [Field]
      public TestEntityTwo24 TestEntityTwo24{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo36 : Entity {

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
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

      [Field]
      public TestEntityTwo25 TestEntityTwo25{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo37 : Entity {

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
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

      [Field]
      public TestEntityTwo26 TestEntityTwo26{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo38 : Entity {

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
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

      [Field]
      public TestEntityTwo27 TestEntityTwo27{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo39 : Entity {

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
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

      [Field]
      public TestEntityTwo28 TestEntityTwo28{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo40 : Entity {

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
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

      [Field]
      public TestEntityTwo29 TestEntityTwo29{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo41 : Entity {

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
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

      [Field]
      public TestEntityTwo30 TestEntityTwo30{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo42 : Entity {

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
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

      [Field]
      public TestEntityTwo31 TestEntityTwo31{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo43 : Entity {

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
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

      [Field]
      public TestEntityTwo32 TestEntityTwo32{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo44 : Entity {

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
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

      [Field]
      public TestEntityTwo33 TestEntityTwo33{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo45 : Entity {

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
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

      [Field]
      public TestEntityTwo34 TestEntityTwo34{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo46 : Entity {

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
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

      [Field]
      public TestEntityTwo35 TestEntityTwo35{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo47 : Entity {

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
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

      [Field]
      public TestEntityTwo36 TestEntityTwo36{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo48 : Entity {

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
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

      [Field]
      public TestEntityTwo37 TestEntityTwo37{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo49 : Entity {

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
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

      [Field]
      public TestEntityTwo38 TestEntityTwo38{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo50 : Entity {

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
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

      [Field]
      public TestEntityTwo39 TestEntityTwo39{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo51 : Entity {

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
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

      [Field]
      public TestEntityTwo40 TestEntityTwo40{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo52 : Entity {

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
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

      [Field]
      public TestEntityTwo41 TestEntityTwo41{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo53 : Entity {

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
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

      [Field]
      public TestEntityTwo42 TestEntityTwo42{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo54 : Entity {

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
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

      [Field]
      public TestEntityTwo43 TestEntityTwo43{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo55 : Entity {

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
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

      [Field]
      public TestEntityTwo44 TestEntityTwo44{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo56 : Entity {

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
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

      [Field]
      public TestEntityTwo45 TestEntityTwo45{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo57 : Entity {

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
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

      [Field]
      public TestEntityTwo46 TestEntityTwo46{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo58 : Entity {

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
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

      [Field]
      public TestEntityTwo47 TestEntityTwo47{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo59 : Entity {

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
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

      [Field]
      public TestEntityTwo48 TestEntityTwo48{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo60 : Entity {

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
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

      [Field]
      public TestEntityTwo49 TestEntityTwo49{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo61 : Entity {

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
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

      [Field]
      public TestEntityTwo50 TestEntityTwo50{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo62 : Entity {

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
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

      [Field]
      public TestEntityTwo51 TestEntityTwo51{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo63 : Entity {

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
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

      [Field]
      public TestEntityTwo52 TestEntityTwo52{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo64 : Entity {

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
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

      [Field]
      public TestEntityTwo53 TestEntityTwo53{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo65 : Entity {

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
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

      [Field]
      public TestEntityTwo54 TestEntityTwo54{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo66 : Entity {

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
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

      [Field]
      public TestEntityTwo55 TestEntityTwo55{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo67 : Entity {

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
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

      [Field]
      public TestEntityTwo56 TestEntityTwo56{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo68 : Entity {

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
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

      [Field]
      public TestEntityTwo57 TestEntityTwo57{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo69 : Entity {

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
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

      [Field]
      public TestEntityTwo58 TestEntityTwo58{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo70 : Entity {

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
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

      [Field]
      public TestEntityTwo59 TestEntityTwo59{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo71 : Entity {

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
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

      [Field]
      public TestEntityTwo60 TestEntityTwo60{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo72 : Entity {

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
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

      [Field]
      public TestEntityTwo61 TestEntityTwo61{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo73 : Entity {

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
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

      [Field]
      public TestEntityTwo62 TestEntityTwo62{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo74 : Entity {

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
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

      [Field]
      public TestEntityTwo63 TestEntityTwo63{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo75 : Entity {

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
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

      [Field]
      public TestEntityTwo64 TestEntityTwo64{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo76 : Entity {

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
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

      [Field]
      public TestEntityTwo65 TestEntityTwo65{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo77 : Entity {

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
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

      [Field]
      public TestEntityTwo66 TestEntityTwo66{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo78 : Entity {

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
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

      [Field]
      public TestEntityTwo67 TestEntityTwo67{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo79 : Entity {

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
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

      [Field]
      public TestEntityTwo68 TestEntityTwo68{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo80 : Entity {

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
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

      [Field]
      public TestEntityTwo69 TestEntityTwo69{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo81 : Entity {

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
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

      [Field]
      public TestEntityTwo70 TestEntityTwo70{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo82 : Entity {

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
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

      [Field]
      public TestEntityTwo71 TestEntityTwo71{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo83 : Entity {

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
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

      [Field]
      public TestEntityTwo72 TestEntityTwo72{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo84 : Entity {

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
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

      [Field]
      public TestEntityTwo73 TestEntityTwo73{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo85 : Entity {

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
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

      [Field]
      public TestEntityTwo74 TestEntityTwo74{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo86 : Entity {

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
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

      [Field]
      public TestEntityTwo75 TestEntityTwo75{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo87 : Entity {

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
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

      [Field]
      public TestEntityTwo76 TestEntityTwo76{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo88 : Entity {

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
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

      [Field]
      public TestEntityTwo77 TestEntityTwo77{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo89 : Entity {

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
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

      [Field]
      public TestEntityTwo78 TestEntityTwo78{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo90 : Entity {

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
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

      [Field]
      public TestEntityTwo79 TestEntityTwo79{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo91 : Entity {

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
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

      [Field]
      public TestEntityTwo80 TestEntityTwo80{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo92 : Entity {

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
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

      [Field]
      public TestEntityTwo81 TestEntityTwo81{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo93 : Entity {

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
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

      [Field]
      public TestEntityTwo82 TestEntityTwo82{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo94 : Entity {

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
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

      [Field]
      public TestEntityTwo83 TestEntityTwo83{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo95 : Entity {

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
      public TestEntityTwo94 TestEntityTwo94{get;set;}

      [Field]
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

      [Field]
      public TestEntityTwo84 TestEntityTwo84{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo96 : Entity {

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
      public TestEntityTwo95 TestEntityTwo95{get;set;}

      [Field]
      public TestEntityTwo94 TestEntityTwo94{get;set;}

      [Field]
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

      [Field]
      public TestEntityTwo85 TestEntityTwo85{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo97 : Entity {

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
      public TestEntityTwo96 TestEntityTwo96{get;set;}

      [Field]
      public TestEntityTwo95 TestEntityTwo95{get;set;}

      [Field]
      public TestEntityTwo94 TestEntityTwo94{get;set;}

      [Field]
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

      [Field]
      public TestEntityTwo86 TestEntityTwo86{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo98 : Entity {

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
      public TestEntityTwo97 TestEntityTwo97{get;set;}

      [Field]
      public TestEntityTwo96 TestEntityTwo96{get;set;}

      [Field]
      public TestEntityTwo95 TestEntityTwo95{get;set;}

      [Field]
      public TestEntityTwo94 TestEntityTwo94{get;set;}

      [Field]
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

      [Field]
      public TestEntityTwo87 TestEntityTwo87{get;set;}

    }
    [HierarchyRoot]
    [Index("Int16Field")]
    [Index("Int32Field")]
    [Index("Int64Field")]
    [Index("FloatField")]
    [Index("DoubleField")]
    public class TestEntityTwo99 : Entity {

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
      public TestEntityTwo98 TestEntityTwo98{get;set;}

      [Field]
      public TestEntityTwo97 TestEntityTwo97{get;set;}

      [Field]
      public TestEntityTwo96 TestEntityTwo96{get;set;}

      [Field]
      public TestEntityTwo95 TestEntityTwo95{get;set;}

      [Field]
      public TestEntityTwo94 TestEntityTwo94{get;set;}

      [Field]
      public TestEntityTwo93 TestEntityTwo93{get;set;}

      [Field]
      public TestEntityTwo92 TestEntityTwo92{get;set;}

      [Field]
      public TestEntityTwo91 TestEntityTwo91{get;set;}

      [Field]
      public TestEntityTwo90 TestEntityTwo90{get;set;}

      [Field]
      public TestEntityTwo89 TestEntityTwo89{get;set;}

      [Field]
      public TestEntityTwo88 TestEntityTwo88{get;set;}

    }
  }

  public class ModelPopulator
  {
    public void Run()
    {
      new PartOne.TestEntityOne0 {
          BooleanField = true,
          Int16Field = 0,
          Int32Field = 0,
          Int64Field = 0,
          FloatField = 0,
          DoubleField = 0,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne0",
          Text = "This is an instance of TestEntityOne0",
      };
      new PartTwo.TestEntityTwo0 {
          BooleanField = true,
          Int16Field = 0,
          Int32Field = 0,
          Int64Field = 0,
          FloatField = 0,
          DoubleField = 0,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo0",
          Text = "This is an instance of TestEntityTwo0",
      };
      new PartOne.TestEntityOne1 {
          BooleanField = true,
          Int16Field = 1,
          Int32Field = 1,
          Int64Field = 1,
          FloatField = 1,
          DoubleField = 1,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne1",
          Text = "This is an instance of TestEntityOne1",
      };
      new PartTwo.TestEntityTwo1 {
          BooleanField = true,
          Int16Field = 1,
          Int32Field = 1,
          Int64Field = 1,
          FloatField = 1,
          DoubleField = 1,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo1",
          Text = "This is an instance of TestEntityTwo1",
      };
      new PartOne.TestEntityOne2 {
          BooleanField = true,
          Int16Field = 2,
          Int32Field = 2,
          Int64Field = 2,
          FloatField = 2,
          DoubleField = 2,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne2",
          Text = "This is an instance of TestEntityOne2",
      };
      new PartTwo.TestEntityTwo2 {
          BooleanField = true,
          Int16Field = 2,
          Int32Field = 2,
          Int64Field = 2,
          FloatField = 2,
          DoubleField = 2,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo2",
          Text = "This is an instance of TestEntityTwo2",
      };
      new PartOne.TestEntityOne3 {
          BooleanField = true,
          Int16Field = 3,
          Int32Field = 3,
          Int64Field = 3,
          FloatField = 3,
          DoubleField = 3,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne3",
          Text = "This is an instance of TestEntityOne3",
      };
      new PartTwo.TestEntityTwo3 {
          BooleanField = true,
          Int16Field = 3,
          Int32Field = 3,
          Int64Field = 3,
          FloatField = 3,
          DoubleField = 3,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo3",
          Text = "This is an instance of TestEntityTwo3",
      };
      new PartOne.TestEntityOne4 {
          BooleanField = true,
          Int16Field = 4,
          Int32Field = 4,
          Int64Field = 4,
          FloatField = 4,
          DoubleField = 4,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne4",
          Text = "This is an instance of TestEntityOne4",
      };
      new PartTwo.TestEntityTwo4 {
          BooleanField = true,
          Int16Field = 4,
          Int32Field = 4,
          Int64Field = 4,
          FloatField = 4,
          DoubleField = 4,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo4",
          Text = "This is an instance of TestEntityTwo4",
      };
      new PartOne.TestEntityOne5 {
          BooleanField = true,
          Int16Field = 5,
          Int32Field = 5,
          Int64Field = 5,
          FloatField = 5,
          DoubleField = 5,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne5",
          Text = "This is an instance of TestEntityOne5",
      };
      new PartTwo.TestEntityTwo5 {
          BooleanField = true,
          Int16Field = 5,
          Int32Field = 5,
          Int64Field = 5,
          FloatField = 5,
          DoubleField = 5,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo5",
          Text = "This is an instance of TestEntityTwo5",
      };
      new PartOne.TestEntityOne6 {
          BooleanField = true,
          Int16Field = 6,
          Int32Field = 6,
          Int64Field = 6,
          FloatField = 6,
          DoubleField = 6,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne6",
          Text = "This is an instance of TestEntityOne6",
      };
      new PartTwo.TestEntityTwo6 {
          BooleanField = true,
          Int16Field = 6,
          Int32Field = 6,
          Int64Field = 6,
          FloatField = 6,
          DoubleField = 6,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo6",
          Text = "This is an instance of TestEntityTwo6",
      };
      new PartOne.TestEntityOne7 {
          BooleanField = true,
          Int16Field = 7,
          Int32Field = 7,
          Int64Field = 7,
          FloatField = 7,
          DoubleField = 7,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne7",
          Text = "This is an instance of TestEntityOne7",
      };
      new PartTwo.TestEntityTwo7 {
          BooleanField = true,
          Int16Field = 7,
          Int32Field = 7,
          Int64Field = 7,
          FloatField = 7,
          DoubleField = 7,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo7",
          Text = "This is an instance of TestEntityTwo7",
      };
      new PartOne.TestEntityOne8 {
          BooleanField = true,
          Int16Field = 8,
          Int32Field = 8,
          Int64Field = 8,
          FloatField = 8,
          DoubleField = 8,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne8",
          Text = "This is an instance of TestEntityOne8",
      };
      new PartTwo.TestEntityTwo8 {
          BooleanField = true,
          Int16Field = 8,
          Int32Field = 8,
          Int64Field = 8,
          FloatField = 8,
          DoubleField = 8,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo8",
          Text = "This is an instance of TestEntityTwo8",
      };
      new PartOne.TestEntityOne9 {
          BooleanField = true,
          Int16Field = 9,
          Int32Field = 9,
          Int64Field = 9,
          FloatField = 9,
          DoubleField = 9,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne9",
          Text = "This is an instance of TestEntityOne9",
      };
      new PartTwo.TestEntityTwo9 {
          BooleanField = true,
          Int16Field = 9,
          Int32Field = 9,
          Int64Field = 9,
          FloatField = 9,
          DoubleField = 9,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo9",
          Text = "This is an instance of TestEntityTwo9",
      };
      new PartOne.TestEntityOne10 {
          BooleanField = true,
          Int16Field = 10,
          Int32Field = 10,
          Int64Field = 10,
          FloatField = 10,
          DoubleField = 10,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne10",
          Text = "This is an instance of TestEntityOne10",
      };
      new PartTwo.TestEntityTwo10 {
          BooleanField = true,
          Int16Field = 10,
          Int32Field = 10,
          Int64Field = 10,
          FloatField = 10,
          DoubleField = 10,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo10",
          Text = "This is an instance of TestEntityTwo10",
      };
      new PartOne.TestEntityOne11 {
          BooleanField = true,
          Int16Field = 11,
          Int32Field = 11,
          Int64Field = 11,
          FloatField = 11,
          DoubleField = 11,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne11",
          Text = "This is an instance of TestEntityOne11",
      };
      new PartTwo.TestEntityTwo11 {
          BooleanField = true,
          Int16Field = 11,
          Int32Field = 11,
          Int64Field = 11,
          FloatField = 11,
          DoubleField = 11,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo11",
          Text = "This is an instance of TestEntityTwo11",
      };
      new PartOne.TestEntityOne12 {
          BooleanField = true,
          Int16Field = 12,
          Int32Field = 12,
          Int64Field = 12,
          FloatField = 12,
          DoubleField = 12,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne12",
          Text = "This is an instance of TestEntityOne12",
      };
      new PartTwo.TestEntityTwo12 {
          BooleanField = true,
          Int16Field = 12,
          Int32Field = 12,
          Int64Field = 12,
          FloatField = 12,
          DoubleField = 12,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo12",
          Text = "This is an instance of TestEntityTwo12",
      };
      new PartOne.TestEntityOne13 {
          BooleanField = true,
          Int16Field = 13,
          Int32Field = 13,
          Int64Field = 13,
          FloatField = 13,
          DoubleField = 13,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne13",
          Text = "This is an instance of TestEntityOne13",
      };
      new PartTwo.TestEntityTwo13 {
          BooleanField = true,
          Int16Field = 13,
          Int32Field = 13,
          Int64Field = 13,
          FloatField = 13,
          DoubleField = 13,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo13",
          Text = "This is an instance of TestEntityTwo13",
      };
      new PartOne.TestEntityOne14 {
          BooleanField = true,
          Int16Field = 14,
          Int32Field = 14,
          Int64Field = 14,
          FloatField = 14,
          DoubleField = 14,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne14",
          Text = "This is an instance of TestEntityOne14",
      };
      new PartTwo.TestEntityTwo14 {
          BooleanField = true,
          Int16Field = 14,
          Int32Field = 14,
          Int64Field = 14,
          FloatField = 14,
          DoubleField = 14,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo14",
          Text = "This is an instance of TestEntityTwo14",
      };
      new PartOne.TestEntityOne15 {
          BooleanField = true,
          Int16Field = 15,
          Int32Field = 15,
          Int64Field = 15,
          FloatField = 15,
          DoubleField = 15,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne15",
          Text = "This is an instance of TestEntityOne15",
      };
      new PartTwo.TestEntityTwo15 {
          BooleanField = true,
          Int16Field = 15,
          Int32Field = 15,
          Int64Field = 15,
          FloatField = 15,
          DoubleField = 15,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo15",
          Text = "This is an instance of TestEntityTwo15",
      };
      new PartOne.TestEntityOne16 {
          BooleanField = true,
          Int16Field = 16,
          Int32Field = 16,
          Int64Field = 16,
          FloatField = 16,
          DoubleField = 16,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne16",
          Text = "This is an instance of TestEntityOne16",
      };
      new PartTwo.TestEntityTwo16 {
          BooleanField = true,
          Int16Field = 16,
          Int32Field = 16,
          Int64Field = 16,
          FloatField = 16,
          DoubleField = 16,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo16",
          Text = "This is an instance of TestEntityTwo16",
      };
      new PartOne.TestEntityOne17 {
          BooleanField = true,
          Int16Field = 17,
          Int32Field = 17,
          Int64Field = 17,
          FloatField = 17,
          DoubleField = 17,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne17",
          Text = "This is an instance of TestEntityOne17",
      };
      new PartTwo.TestEntityTwo17 {
          BooleanField = true,
          Int16Field = 17,
          Int32Field = 17,
          Int64Field = 17,
          FloatField = 17,
          DoubleField = 17,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo17",
          Text = "This is an instance of TestEntityTwo17",
      };
      new PartOne.TestEntityOne18 {
          BooleanField = true,
          Int16Field = 18,
          Int32Field = 18,
          Int64Field = 18,
          FloatField = 18,
          DoubleField = 18,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne18",
          Text = "This is an instance of TestEntityOne18",
      };
      new PartTwo.TestEntityTwo18 {
          BooleanField = true,
          Int16Field = 18,
          Int32Field = 18,
          Int64Field = 18,
          FloatField = 18,
          DoubleField = 18,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo18",
          Text = "This is an instance of TestEntityTwo18",
      };
      new PartOne.TestEntityOne19 {
          BooleanField = true,
          Int16Field = 19,
          Int32Field = 19,
          Int64Field = 19,
          FloatField = 19,
          DoubleField = 19,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne19",
          Text = "This is an instance of TestEntityOne19",
      };
      new PartTwo.TestEntityTwo19 {
          BooleanField = true,
          Int16Field = 19,
          Int32Field = 19,
          Int64Field = 19,
          FloatField = 19,
          DoubleField = 19,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo19",
          Text = "This is an instance of TestEntityTwo19",
      };
      new PartOne.TestEntityOne20 {
          BooleanField = true,
          Int16Field = 20,
          Int32Field = 20,
          Int64Field = 20,
          FloatField = 20,
          DoubleField = 20,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne20",
          Text = "This is an instance of TestEntityOne20",
      };
      new PartTwo.TestEntityTwo20 {
          BooleanField = true,
          Int16Field = 20,
          Int32Field = 20,
          Int64Field = 20,
          FloatField = 20,
          DoubleField = 20,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo20",
          Text = "This is an instance of TestEntityTwo20",
      };
      new PartOne.TestEntityOne21 {
          BooleanField = true,
          Int16Field = 21,
          Int32Field = 21,
          Int64Field = 21,
          FloatField = 21,
          DoubleField = 21,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne21",
          Text = "This is an instance of TestEntityOne21",
      };
      new PartTwo.TestEntityTwo21 {
          BooleanField = true,
          Int16Field = 21,
          Int32Field = 21,
          Int64Field = 21,
          FloatField = 21,
          DoubleField = 21,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo21",
          Text = "This is an instance of TestEntityTwo21",
      };
      new PartOne.TestEntityOne22 {
          BooleanField = true,
          Int16Field = 22,
          Int32Field = 22,
          Int64Field = 22,
          FloatField = 22,
          DoubleField = 22,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne22",
          Text = "This is an instance of TestEntityOne22",
      };
      new PartTwo.TestEntityTwo22 {
          BooleanField = true,
          Int16Field = 22,
          Int32Field = 22,
          Int64Field = 22,
          FloatField = 22,
          DoubleField = 22,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo22",
          Text = "This is an instance of TestEntityTwo22",
      };
      new PartOne.TestEntityOne23 {
          BooleanField = true,
          Int16Field = 23,
          Int32Field = 23,
          Int64Field = 23,
          FloatField = 23,
          DoubleField = 23,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne23",
          Text = "This is an instance of TestEntityOne23",
      };
      new PartTwo.TestEntityTwo23 {
          BooleanField = true,
          Int16Field = 23,
          Int32Field = 23,
          Int64Field = 23,
          FloatField = 23,
          DoubleField = 23,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo23",
          Text = "This is an instance of TestEntityTwo23",
      };
      new PartOne.TestEntityOne24 {
          BooleanField = true,
          Int16Field = 24,
          Int32Field = 24,
          Int64Field = 24,
          FloatField = 24,
          DoubleField = 24,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne24",
          Text = "This is an instance of TestEntityOne24",
      };
      new PartTwo.TestEntityTwo24 {
          BooleanField = true,
          Int16Field = 24,
          Int32Field = 24,
          Int64Field = 24,
          FloatField = 24,
          DoubleField = 24,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo24",
          Text = "This is an instance of TestEntityTwo24",
      };
      new PartOne.TestEntityOne25 {
          BooleanField = true,
          Int16Field = 25,
          Int32Field = 25,
          Int64Field = 25,
          FloatField = 25,
          DoubleField = 25,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne25",
          Text = "This is an instance of TestEntityOne25",
      };
      new PartTwo.TestEntityTwo25 {
          BooleanField = true,
          Int16Field = 25,
          Int32Field = 25,
          Int64Field = 25,
          FloatField = 25,
          DoubleField = 25,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo25",
          Text = "This is an instance of TestEntityTwo25",
      };
      new PartOne.TestEntityOne26 {
          BooleanField = true,
          Int16Field = 26,
          Int32Field = 26,
          Int64Field = 26,
          FloatField = 26,
          DoubleField = 26,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne26",
          Text = "This is an instance of TestEntityOne26",
      };
      new PartTwo.TestEntityTwo26 {
          BooleanField = true,
          Int16Field = 26,
          Int32Field = 26,
          Int64Field = 26,
          FloatField = 26,
          DoubleField = 26,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo26",
          Text = "This is an instance of TestEntityTwo26",
      };
      new PartOne.TestEntityOne27 {
          BooleanField = true,
          Int16Field = 27,
          Int32Field = 27,
          Int64Field = 27,
          FloatField = 27,
          DoubleField = 27,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne27",
          Text = "This is an instance of TestEntityOne27",
      };
      new PartTwo.TestEntityTwo27 {
          BooleanField = true,
          Int16Field = 27,
          Int32Field = 27,
          Int64Field = 27,
          FloatField = 27,
          DoubleField = 27,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo27",
          Text = "This is an instance of TestEntityTwo27",
      };
      new PartOne.TestEntityOne28 {
          BooleanField = true,
          Int16Field = 28,
          Int32Field = 28,
          Int64Field = 28,
          FloatField = 28,
          DoubleField = 28,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne28",
          Text = "This is an instance of TestEntityOne28",
      };
      new PartTwo.TestEntityTwo28 {
          BooleanField = true,
          Int16Field = 28,
          Int32Field = 28,
          Int64Field = 28,
          FloatField = 28,
          DoubleField = 28,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo28",
          Text = "This is an instance of TestEntityTwo28",
      };
      new PartOne.TestEntityOne29 {
          BooleanField = true,
          Int16Field = 29,
          Int32Field = 29,
          Int64Field = 29,
          FloatField = 29,
          DoubleField = 29,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne29",
          Text = "This is an instance of TestEntityOne29",
      };
      new PartTwo.TestEntityTwo29 {
          BooleanField = true,
          Int16Field = 29,
          Int32Field = 29,
          Int64Field = 29,
          FloatField = 29,
          DoubleField = 29,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo29",
          Text = "This is an instance of TestEntityTwo29",
      };
      new PartOne.TestEntityOne30 {
          BooleanField = true,
          Int16Field = 30,
          Int32Field = 30,
          Int64Field = 30,
          FloatField = 30,
          DoubleField = 30,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne30",
          Text = "This is an instance of TestEntityOne30",
      };
      new PartTwo.TestEntityTwo30 {
          BooleanField = true,
          Int16Field = 30,
          Int32Field = 30,
          Int64Field = 30,
          FloatField = 30,
          DoubleField = 30,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo30",
          Text = "This is an instance of TestEntityTwo30",
      };
      new PartOne.TestEntityOne31 {
          BooleanField = true,
          Int16Field = 31,
          Int32Field = 31,
          Int64Field = 31,
          FloatField = 31,
          DoubleField = 31,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne31",
          Text = "This is an instance of TestEntityOne31",
      };
      new PartTwo.TestEntityTwo31 {
          BooleanField = true,
          Int16Field = 31,
          Int32Field = 31,
          Int64Field = 31,
          FloatField = 31,
          DoubleField = 31,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo31",
          Text = "This is an instance of TestEntityTwo31",
      };
      new PartOne.TestEntityOne32 {
          BooleanField = true,
          Int16Field = 32,
          Int32Field = 32,
          Int64Field = 32,
          FloatField = 32,
          DoubleField = 32,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne32",
          Text = "This is an instance of TestEntityOne32",
      };
      new PartTwo.TestEntityTwo32 {
          BooleanField = true,
          Int16Field = 32,
          Int32Field = 32,
          Int64Field = 32,
          FloatField = 32,
          DoubleField = 32,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo32",
          Text = "This is an instance of TestEntityTwo32",
      };
      new PartOne.TestEntityOne33 {
          BooleanField = true,
          Int16Field = 33,
          Int32Field = 33,
          Int64Field = 33,
          FloatField = 33,
          DoubleField = 33,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne33",
          Text = "This is an instance of TestEntityOne33",
      };
      new PartTwo.TestEntityTwo33 {
          BooleanField = true,
          Int16Field = 33,
          Int32Field = 33,
          Int64Field = 33,
          FloatField = 33,
          DoubleField = 33,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo33",
          Text = "This is an instance of TestEntityTwo33",
      };
      new PartOne.TestEntityOne34 {
          BooleanField = true,
          Int16Field = 34,
          Int32Field = 34,
          Int64Field = 34,
          FloatField = 34,
          DoubleField = 34,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne34",
          Text = "This is an instance of TestEntityOne34",
      };
      new PartTwo.TestEntityTwo34 {
          BooleanField = true,
          Int16Field = 34,
          Int32Field = 34,
          Int64Field = 34,
          FloatField = 34,
          DoubleField = 34,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo34",
          Text = "This is an instance of TestEntityTwo34",
      };
      new PartOne.TestEntityOne35 {
          BooleanField = true,
          Int16Field = 35,
          Int32Field = 35,
          Int64Field = 35,
          FloatField = 35,
          DoubleField = 35,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne35",
          Text = "This is an instance of TestEntityOne35",
      };
      new PartTwo.TestEntityTwo35 {
          BooleanField = true,
          Int16Field = 35,
          Int32Field = 35,
          Int64Field = 35,
          FloatField = 35,
          DoubleField = 35,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo35",
          Text = "This is an instance of TestEntityTwo35",
      };
      new PartOne.TestEntityOne36 {
          BooleanField = true,
          Int16Field = 36,
          Int32Field = 36,
          Int64Field = 36,
          FloatField = 36,
          DoubleField = 36,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne36",
          Text = "This is an instance of TestEntityOne36",
      };
      new PartTwo.TestEntityTwo36 {
          BooleanField = true,
          Int16Field = 36,
          Int32Field = 36,
          Int64Field = 36,
          FloatField = 36,
          DoubleField = 36,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo36",
          Text = "This is an instance of TestEntityTwo36",
      };
      new PartOne.TestEntityOne37 {
          BooleanField = true,
          Int16Field = 37,
          Int32Field = 37,
          Int64Field = 37,
          FloatField = 37,
          DoubleField = 37,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne37",
          Text = "This is an instance of TestEntityOne37",
      };
      new PartTwo.TestEntityTwo37 {
          BooleanField = true,
          Int16Field = 37,
          Int32Field = 37,
          Int64Field = 37,
          FloatField = 37,
          DoubleField = 37,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo37",
          Text = "This is an instance of TestEntityTwo37",
      };
      new PartOne.TestEntityOne38 {
          BooleanField = true,
          Int16Field = 38,
          Int32Field = 38,
          Int64Field = 38,
          FloatField = 38,
          DoubleField = 38,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne38",
          Text = "This is an instance of TestEntityOne38",
      };
      new PartTwo.TestEntityTwo38 {
          BooleanField = true,
          Int16Field = 38,
          Int32Field = 38,
          Int64Field = 38,
          FloatField = 38,
          DoubleField = 38,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo38",
          Text = "This is an instance of TestEntityTwo38",
      };
      new PartOne.TestEntityOne39 {
          BooleanField = true,
          Int16Field = 39,
          Int32Field = 39,
          Int64Field = 39,
          FloatField = 39,
          DoubleField = 39,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne39",
          Text = "This is an instance of TestEntityOne39",
      };
      new PartTwo.TestEntityTwo39 {
          BooleanField = true,
          Int16Field = 39,
          Int32Field = 39,
          Int64Field = 39,
          FloatField = 39,
          DoubleField = 39,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo39",
          Text = "This is an instance of TestEntityTwo39",
      };
      new PartOne.TestEntityOne40 {
          BooleanField = true,
          Int16Field = 40,
          Int32Field = 40,
          Int64Field = 40,
          FloatField = 40,
          DoubleField = 40,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne40",
          Text = "This is an instance of TestEntityOne40",
      };
      new PartTwo.TestEntityTwo40 {
          BooleanField = true,
          Int16Field = 40,
          Int32Field = 40,
          Int64Field = 40,
          FloatField = 40,
          DoubleField = 40,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo40",
          Text = "This is an instance of TestEntityTwo40",
      };
      new PartOne.TestEntityOne41 {
          BooleanField = true,
          Int16Field = 41,
          Int32Field = 41,
          Int64Field = 41,
          FloatField = 41,
          DoubleField = 41,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne41",
          Text = "This is an instance of TestEntityOne41",
      };
      new PartTwo.TestEntityTwo41 {
          BooleanField = true,
          Int16Field = 41,
          Int32Field = 41,
          Int64Field = 41,
          FloatField = 41,
          DoubleField = 41,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo41",
          Text = "This is an instance of TestEntityTwo41",
      };
      new PartOne.TestEntityOne42 {
          BooleanField = true,
          Int16Field = 42,
          Int32Field = 42,
          Int64Field = 42,
          FloatField = 42,
          DoubleField = 42,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne42",
          Text = "This is an instance of TestEntityOne42",
      };
      new PartTwo.TestEntityTwo42 {
          BooleanField = true,
          Int16Field = 42,
          Int32Field = 42,
          Int64Field = 42,
          FloatField = 42,
          DoubleField = 42,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo42",
          Text = "This is an instance of TestEntityTwo42",
      };
      new PartOne.TestEntityOne43 {
          BooleanField = true,
          Int16Field = 43,
          Int32Field = 43,
          Int64Field = 43,
          FloatField = 43,
          DoubleField = 43,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne43",
          Text = "This is an instance of TestEntityOne43",
      };
      new PartTwo.TestEntityTwo43 {
          BooleanField = true,
          Int16Field = 43,
          Int32Field = 43,
          Int64Field = 43,
          FloatField = 43,
          DoubleField = 43,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo43",
          Text = "This is an instance of TestEntityTwo43",
      };
      new PartOne.TestEntityOne44 {
          BooleanField = true,
          Int16Field = 44,
          Int32Field = 44,
          Int64Field = 44,
          FloatField = 44,
          DoubleField = 44,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne44",
          Text = "This is an instance of TestEntityOne44",
      };
      new PartTwo.TestEntityTwo44 {
          BooleanField = true,
          Int16Field = 44,
          Int32Field = 44,
          Int64Field = 44,
          FloatField = 44,
          DoubleField = 44,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo44",
          Text = "This is an instance of TestEntityTwo44",
      };
      new PartOne.TestEntityOne45 {
          BooleanField = true,
          Int16Field = 45,
          Int32Field = 45,
          Int64Field = 45,
          FloatField = 45,
          DoubleField = 45,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne45",
          Text = "This is an instance of TestEntityOne45",
      };
      new PartTwo.TestEntityTwo45 {
          BooleanField = true,
          Int16Field = 45,
          Int32Field = 45,
          Int64Field = 45,
          FloatField = 45,
          DoubleField = 45,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo45",
          Text = "This is an instance of TestEntityTwo45",
      };
      new PartOne.TestEntityOne46 {
          BooleanField = true,
          Int16Field = 46,
          Int32Field = 46,
          Int64Field = 46,
          FloatField = 46,
          DoubleField = 46,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne46",
          Text = "This is an instance of TestEntityOne46",
      };
      new PartTwo.TestEntityTwo46 {
          BooleanField = true,
          Int16Field = 46,
          Int32Field = 46,
          Int64Field = 46,
          FloatField = 46,
          DoubleField = 46,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo46",
          Text = "This is an instance of TestEntityTwo46",
      };
      new PartOne.TestEntityOne47 {
          BooleanField = true,
          Int16Field = 47,
          Int32Field = 47,
          Int64Field = 47,
          FloatField = 47,
          DoubleField = 47,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne47",
          Text = "This is an instance of TestEntityOne47",
      };
      new PartTwo.TestEntityTwo47 {
          BooleanField = true,
          Int16Field = 47,
          Int32Field = 47,
          Int64Field = 47,
          FloatField = 47,
          DoubleField = 47,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo47",
          Text = "This is an instance of TestEntityTwo47",
      };
      new PartOne.TestEntityOne48 {
          BooleanField = true,
          Int16Field = 48,
          Int32Field = 48,
          Int64Field = 48,
          FloatField = 48,
          DoubleField = 48,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne48",
          Text = "This is an instance of TestEntityOne48",
      };
      new PartTwo.TestEntityTwo48 {
          BooleanField = true,
          Int16Field = 48,
          Int32Field = 48,
          Int64Field = 48,
          FloatField = 48,
          DoubleField = 48,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo48",
          Text = "This is an instance of TestEntityTwo48",
      };
      new PartOne.TestEntityOne49 {
          BooleanField = true,
          Int16Field = 49,
          Int32Field = 49,
          Int64Field = 49,
          FloatField = 49,
          DoubleField = 49,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne49",
          Text = "This is an instance of TestEntityOne49",
      };
      new PartTwo.TestEntityTwo49 {
          BooleanField = true,
          Int16Field = 49,
          Int32Field = 49,
          Int64Field = 49,
          FloatField = 49,
          DoubleField = 49,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo49",
          Text = "This is an instance of TestEntityTwo49",
      };
      new PartOne.TestEntityOne50 {
          BooleanField = true,
          Int16Field = 50,
          Int32Field = 50,
          Int64Field = 50,
          FloatField = 50,
          DoubleField = 50,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne50",
          Text = "This is an instance of TestEntityOne50",
      };
      new PartTwo.TestEntityTwo50 {
          BooleanField = true,
          Int16Field = 50,
          Int32Field = 50,
          Int64Field = 50,
          FloatField = 50,
          DoubleField = 50,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo50",
          Text = "This is an instance of TestEntityTwo50",
      };
      new PartOne.TestEntityOne51 {
          BooleanField = true,
          Int16Field = 51,
          Int32Field = 51,
          Int64Field = 51,
          FloatField = 51,
          DoubleField = 51,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne51",
          Text = "This is an instance of TestEntityOne51",
      };
      new PartTwo.TestEntityTwo51 {
          BooleanField = true,
          Int16Field = 51,
          Int32Field = 51,
          Int64Field = 51,
          FloatField = 51,
          DoubleField = 51,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo51",
          Text = "This is an instance of TestEntityTwo51",
      };
      new PartOne.TestEntityOne52 {
          BooleanField = true,
          Int16Field = 52,
          Int32Field = 52,
          Int64Field = 52,
          FloatField = 52,
          DoubleField = 52,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne52",
          Text = "This is an instance of TestEntityOne52",
      };
      new PartTwo.TestEntityTwo52 {
          BooleanField = true,
          Int16Field = 52,
          Int32Field = 52,
          Int64Field = 52,
          FloatField = 52,
          DoubleField = 52,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo52",
          Text = "This is an instance of TestEntityTwo52",
      };
      new PartOne.TestEntityOne53 {
          BooleanField = true,
          Int16Field = 53,
          Int32Field = 53,
          Int64Field = 53,
          FloatField = 53,
          DoubleField = 53,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne53",
          Text = "This is an instance of TestEntityOne53",
      };
      new PartTwo.TestEntityTwo53 {
          BooleanField = true,
          Int16Field = 53,
          Int32Field = 53,
          Int64Field = 53,
          FloatField = 53,
          DoubleField = 53,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo53",
          Text = "This is an instance of TestEntityTwo53",
      };
      new PartOne.TestEntityOne54 {
          BooleanField = true,
          Int16Field = 54,
          Int32Field = 54,
          Int64Field = 54,
          FloatField = 54,
          DoubleField = 54,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne54",
          Text = "This is an instance of TestEntityOne54",
      };
      new PartTwo.TestEntityTwo54 {
          BooleanField = true,
          Int16Field = 54,
          Int32Field = 54,
          Int64Field = 54,
          FloatField = 54,
          DoubleField = 54,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo54",
          Text = "This is an instance of TestEntityTwo54",
      };
      new PartOne.TestEntityOne55 {
          BooleanField = true,
          Int16Field = 55,
          Int32Field = 55,
          Int64Field = 55,
          FloatField = 55,
          DoubleField = 55,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne55",
          Text = "This is an instance of TestEntityOne55",
      };
      new PartTwo.TestEntityTwo55 {
          BooleanField = true,
          Int16Field = 55,
          Int32Field = 55,
          Int64Field = 55,
          FloatField = 55,
          DoubleField = 55,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo55",
          Text = "This is an instance of TestEntityTwo55",
      };
      new PartOne.TestEntityOne56 {
          BooleanField = true,
          Int16Field = 56,
          Int32Field = 56,
          Int64Field = 56,
          FloatField = 56,
          DoubleField = 56,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne56",
          Text = "This is an instance of TestEntityOne56",
      };
      new PartTwo.TestEntityTwo56 {
          BooleanField = true,
          Int16Field = 56,
          Int32Field = 56,
          Int64Field = 56,
          FloatField = 56,
          DoubleField = 56,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo56",
          Text = "This is an instance of TestEntityTwo56",
      };
      new PartOne.TestEntityOne57 {
          BooleanField = true,
          Int16Field = 57,
          Int32Field = 57,
          Int64Field = 57,
          FloatField = 57,
          DoubleField = 57,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne57",
          Text = "This is an instance of TestEntityOne57",
      };
      new PartTwo.TestEntityTwo57 {
          BooleanField = true,
          Int16Field = 57,
          Int32Field = 57,
          Int64Field = 57,
          FloatField = 57,
          DoubleField = 57,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo57",
          Text = "This is an instance of TestEntityTwo57",
      };
      new PartOne.TestEntityOne58 {
          BooleanField = true,
          Int16Field = 58,
          Int32Field = 58,
          Int64Field = 58,
          FloatField = 58,
          DoubleField = 58,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne58",
          Text = "This is an instance of TestEntityOne58",
      };
      new PartTwo.TestEntityTwo58 {
          BooleanField = true,
          Int16Field = 58,
          Int32Field = 58,
          Int64Field = 58,
          FloatField = 58,
          DoubleField = 58,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo58",
          Text = "This is an instance of TestEntityTwo58",
      };
      new PartOne.TestEntityOne59 {
          BooleanField = true,
          Int16Field = 59,
          Int32Field = 59,
          Int64Field = 59,
          FloatField = 59,
          DoubleField = 59,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne59",
          Text = "This is an instance of TestEntityOne59",
      };
      new PartTwo.TestEntityTwo59 {
          BooleanField = true,
          Int16Field = 59,
          Int32Field = 59,
          Int64Field = 59,
          FloatField = 59,
          DoubleField = 59,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo59",
          Text = "This is an instance of TestEntityTwo59",
      };
      new PartOne.TestEntityOne60 {
          BooleanField = true,
          Int16Field = 60,
          Int32Field = 60,
          Int64Field = 60,
          FloatField = 60,
          DoubleField = 60,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne60",
          Text = "This is an instance of TestEntityOne60",
      };
      new PartTwo.TestEntityTwo60 {
          BooleanField = true,
          Int16Field = 60,
          Int32Field = 60,
          Int64Field = 60,
          FloatField = 60,
          DoubleField = 60,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo60",
          Text = "This is an instance of TestEntityTwo60",
      };
      new PartOne.TestEntityOne61 {
          BooleanField = true,
          Int16Field = 61,
          Int32Field = 61,
          Int64Field = 61,
          FloatField = 61,
          DoubleField = 61,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne61",
          Text = "This is an instance of TestEntityOne61",
      };
      new PartTwo.TestEntityTwo61 {
          BooleanField = true,
          Int16Field = 61,
          Int32Field = 61,
          Int64Field = 61,
          FloatField = 61,
          DoubleField = 61,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo61",
          Text = "This is an instance of TestEntityTwo61",
      };
      new PartOne.TestEntityOne62 {
          BooleanField = true,
          Int16Field = 62,
          Int32Field = 62,
          Int64Field = 62,
          FloatField = 62,
          DoubleField = 62,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne62",
          Text = "This is an instance of TestEntityOne62",
      };
      new PartTwo.TestEntityTwo62 {
          BooleanField = true,
          Int16Field = 62,
          Int32Field = 62,
          Int64Field = 62,
          FloatField = 62,
          DoubleField = 62,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo62",
          Text = "This is an instance of TestEntityTwo62",
      };
      new PartOne.TestEntityOne63 {
          BooleanField = true,
          Int16Field = 63,
          Int32Field = 63,
          Int64Field = 63,
          FloatField = 63,
          DoubleField = 63,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne63",
          Text = "This is an instance of TestEntityOne63",
      };
      new PartTwo.TestEntityTwo63 {
          BooleanField = true,
          Int16Field = 63,
          Int32Field = 63,
          Int64Field = 63,
          FloatField = 63,
          DoubleField = 63,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo63",
          Text = "This is an instance of TestEntityTwo63",
      };
      new PartOne.TestEntityOne64 {
          BooleanField = true,
          Int16Field = 64,
          Int32Field = 64,
          Int64Field = 64,
          FloatField = 64,
          DoubleField = 64,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne64",
          Text = "This is an instance of TestEntityOne64",
      };
      new PartTwo.TestEntityTwo64 {
          BooleanField = true,
          Int16Field = 64,
          Int32Field = 64,
          Int64Field = 64,
          FloatField = 64,
          DoubleField = 64,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo64",
          Text = "This is an instance of TestEntityTwo64",
      };
      new PartOne.TestEntityOne65 {
          BooleanField = true,
          Int16Field = 65,
          Int32Field = 65,
          Int64Field = 65,
          FloatField = 65,
          DoubleField = 65,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne65",
          Text = "This is an instance of TestEntityOne65",
      };
      new PartTwo.TestEntityTwo65 {
          BooleanField = true,
          Int16Field = 65,
          Int32Field = 65,
          Int64Field = 65,
          FloatField = 65,
          DoubleField = 65,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo65",
          Text = "This is an instance of TestEntityTwo65",
      };
      new PartOne.TestEntityOne66 {
          BooleanField = true,
          Int16Field = 66,
          Int32Field = 66,
          Int64Field = 66,
          FloatField = 66,
          DoubleField = 66,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne66",
          Text = "This is an instance of TestEntityOne66",
      };
      new PartTwo.TestEntityTwo66 {
          BooleanField = true,
          Int16Field = 66,
          Int32Field = 66,
          Int64Field = 66,
          FloatField = 66,
          DoubleField = 66,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo66",
          Text = "This is an instance of TestEntityTwo66",
      };
      new PartOne.TestEntityOne67 {
          BooleanField = true,
          Int16Field = 67,
          Int32Field = 67,
          Int64Field = 67,
          FloatField = 67,
          DoubleField = 67,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne67",
          Text = "This is an instance of TestEntityOne67",
      };
      new PartTwo.TestEntityTwo67 {
          BooleanField = true,
          Int16Field = 67,
          Int32Field = 67,
          Int64Field = 67,
          FloatField = 67,
          DoubleField = 67,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo67",
          Text = "This is an instance of TestEntityTwo67",
      };
      new PartOne.TestEntityOne68 {
          BooleanField = true,
          Int16Field = 68,
          Int32Field = 68,
          Int64Field = 68,
          FloatField = 68,
          DoubleField = 68,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne68",
          Text = "This is an instance of TestEntityOne68",
      };
      new PartTwo.TestEntityTwo68 {
          BooleanField = true,
          Int16Field = 68,
          Int32Field = 68,
          Int64Field = 68,
          FloatField = 68,
          DoubleField = 68,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo68",
          Text = "This is an instance of TestEntityTwo68",
      };
      new PartOne.TestEntityOne69 {
          BooleanField = true,
          Int16Field = 69,
          Int32Field = 69,
          Int64Field = 69,
          FloatField = 69,
          DoubleField = 69,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne69",
          Text = "This is an instance of TestEntityOne69",
      };
      new PartTwo.TestEntityTwo69 {
          BooleanField = true,
          Int16Field = 69,
          Int32Field = 69,
          Int64Field = 69,
          FloatField = 69,
          DoubleField = 69,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo69",
          Text = "This is an instance of TestEntityTwo69",
      };
      new PartOne.TestEntityOne70 {
          BooleanField = true,
          Int16Field = 70,
          Int32Field = 70,
          Int64Field = 70,
          FloatField = 70,
          DoubleField = 70,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne70",
          Text = "This is an instance of TestEntityOne70",
      };
      new PartTwo.TestEntityTwo70 {
          BooleanField = true,
          Int16Field = 70,
          Int32Field = 70,
          Int64Field = 70,
          FloatField = 70,
          DoubleField = 70,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo70",
          Text = "This is an instance of TestEntityTwo70",
      };
      new PartOne.TestEntityOne71 {
          BooleanField = true,
          Int16Field = 71,
          Int32Field = 71,
          Int64Field = 71,
          FloatField = 71,
          DoubleField = 71,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne71",
          Text = "This is an instance of TestEntityOne71",
      };
      new PartTwo.TestEntityTwo71 {
          BooleanField = true,
          Int16Field = 71,
          Int32Field = 71,
          Int64Field = 71,
          FloatField = 71,
          DoubleField = 71,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo71",
          Text = "This is an instance of TestEntityTwo71",
      };
      new PartOne.TestEntityOne72 {
          BooleanField = true,
          Int16Field = 72,
          Int32Field = 72,
          Int64Field = 72,
          FloatField = 72,
          DoubleField = 72,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne72",
          Text = "This is an instance of TestEntityOne72",
      };
      new PartTwo.TestEntityTwo72 {
          BooleanField = true,
          Int16Field = 72,
          Int32Field = 72,
          Int64Field = 72,
          FloatField = 72,
          DoubleField = 72,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo72",
          Text = "This is an instance of TestEntityTwo72",
      };
      new PartOne.TestEntityOne73 {
          BooleanField = true,
          Int16Field = 73,
          Int32Field = 73,
          Int64Field = 73,
          FloatField = 73,
          DoubleField = 73,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne73",
          Text = "This is an instance of TestEntityOne73",
      };
      new PartTwo.TestEntityTwo73 {
          BooleanField = true,
          Int16Field = 73,
          Int32Field = 73,
          Int64Field = 73,
          FloatField = 73,
          DoubleField = 73,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo73",
          Text = "This is an instance of TestEntityTwo73",
      };
      new PartOne.TestEntityOne74 {
          BooleanField = true,
          Int16Field = 74,
          Int32Field = 74,
          Int64Field = 74,
          FloatField = 74,
          DoubleField = 74,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne74",
          Text = "This is an instance of TestEntityOne74",
      };
      new PartTwo.TestEntityTwo74 {
          BooleanField = true,
          Int16Field = 74,
          Int32Field = 74,
          Int64Field = 74,
          FloatField = 74,
          DoubleField = 74,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo74",
          Text = "This is an instance of TestEntityTwo74",
      };
      new PartOne.TestEntityOne75 {
          BooleanField = true,
          Int16Field = 75,
          Int32Field = 75,
          Int64Field = 75,
          FloatField = 75,
          DoubleField = 75,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne75",
          Text = "This is an instance of TestEntityOne75",
      };
      new PartTwo.TestEntityTwo75 {
          BooleanField = true,
          Int16Field = 75,
          Int32Field = 75,
          Int64Field = 75,
          FloatField = 75,
          DoubleField = 75,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo75",
          Text = "This is an instance of TestEntityTwo75",
      };
      new PartOne.TestEntityOne76 {
          BooleanField = true,
          Int16Field = 76,
          Int32Field = 76,
          Int64Field = 76,
          FloatField = 76,
          DoubleField = 76,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne76",
          Text = "This is an instance of TestEntityOne76",
      };
      new PartTwo.TestEntityTwo76 {
          BooleanField = true,
          Int16Field = 76,
          Int32Field = 76,
          Int64Field = 76,
          FloatField = 76,
          DoubleField = 76,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo76",
          Text = "This is an instance of TestEntityTwo76",
      };
      new PartOne.TestEntityOne77 {
          BooleanField = true,
          Int16Field = 77,
          Int32Field = 77,
          Int64Field = 77,
          FloatField = 77,
          DoubleField = 77,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne77",
          Text = "This is an instance of TestEntityOne77",
      };
      new PartTwo.TestEntityTwo77 {
          BooleanField = true,
          Int16Field = 77,
          Int32Field = 77,
          Int64Field = 77,
          FloatField = 77,
          DoubleField = 77,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo77",
          Text = "This is an instance of TestEntityTwo77",
      };
      new PartOne.TestEntityOne78 {
          BooleanField = true,
          Int16Field = 78,
          Int32Field = 78,
          Int64Field = 78,
          FloatField = 78,
          DoubleField = 78,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne78",
          Text = "This is an instance of TestEntityOne78",
      };
      new PartTwo.TestEntityTwo78 {
          BooleanField = true,
          Int16Field = 78,
          Int32Field = 78,
          Int64Field = 78,
          FloatField = 78,
          DoubleField = 78,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo78",
          Text = "This is an instance of TestEntityTwo78",
      };
      new PartOne.TestEntityOne79 {
          BooleanField = true,
          Int16Field = 79,
          Int32Field = 79,
          Int64Field = 79,
          FloatField = 79,
          DoubleField = 79,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne79",
          Text = "This is an instance of TestEntityOne79",
      };
      new PartTwo.TestEntityTwo79 {
          BooleanField = true,
          Int16Field = 79,
          Int32Field = 79,
          Int64Field = 79,
          FloatField = 79,
          DoubleField = 79,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo79",
          Text = "This is an instance of TestEntityTwo79",
      };
      new PartOne.TestEntityOne80 {
          BooleanField = true,
          Int16Field = 80,
          Int32Field = 80,
          Int64Field = 80,
          FloatField = 80,
          DoubleField = 80,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne80",
          Text = "This is an instance of TestEntityOne80",
      };
      new PartTwo.TestEntityTwo80 {
          BooleanField = true,
          Int16Field = 80,
          Int32Field = 80,
          Int64Field = 80,
          FloatField = 80,
          DoubleField = 80,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo80",
          Text = "This is an instance of TestEntityTwo80",
      };
      new PartOne.TestEntityOne81 {
          BooleanField = true,
          Int16Field = 81,
          Int32Field = 81,
          Int64Field = 81,
          FloatField = 81,
          DoubleField = 81,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne81",
          Text = "This is an instance of TestEntityOne81",
      };
      new PartTwo.TestEntityTwo81 {
          BooleanField = true,
          Int16Field = 81,
          Int32Field = 81,
          Int64Field = 81,
          FloatField = 81,
          DoubleField = 81,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo81",
          Text = "This is an instance of TestEntityTwo81",
      };
      new PartOne.TestEntityOne82 {
          BooleanField = true,
          Int16Field = 82,
          Int32Field = 82,
          Int64Field = 82,
          FloatField = 82,
          DoubleField = 82,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne82",
          Text = "This is an instance of TestEntityOne82",
      };
      new PartTwo.TestEntityTwo82 {
          BooleanField = true,
          Int16Field = 82,
          Int32Field = 82,
          Int64Field = 82,
          FloatField = 82,
          DoubleField = 82,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo82",
          Text = "This is an instance of TestEntityTwo82",
      };
      new PartOne.TestEntityOne83 {
          BooleanField = true,
          Int16Field = 83,
          Int32Field = 83,
          Int64Field = 83,
          FloatField = 83,
          DoubleField = 83,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne83",
          Text = "This is an instance of TestEntityOne83",
      };
      new PartTwo.TestEntityTwo83 {
          BooleanField = true,
          Int16Field = 83,
          Int32Field = 83,
          Int64Field = 83,
          FloatField = 83,
          DoubleField = 83,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo83",
          Text = "This is an instance of TestEntityTwo83",
      };
      new PartOne.TestEntityOne84 {
          BooleanField = true,
          Int16Field = 84,
          Int32Field = 84,
          Int64Field = 84,
          FloatField = 84,
          DoubleField = 84,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne84",
          Text = "This is an instance of TestEntityOne84",
      };
      new PartTwo.TestEntityTwo84 {
          BooleanField = true,
          Int16Field = 84,
          Int32Field = 84,
          Int64Field = 84,
          FloatField = 84,
          DoubleField = 84,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo84",
          Text = "This is an instance of TestEntityTwo84",
      };
      new PartOne.TestEntityOne85 {
          BooleanField = true,
          Int16Field = 85,
          Int32Field = 85,
          Int64Field = 85,
          FloatField = 85,
          DoubleField = 85,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne85",
          Text = "This is an instance of TestEntityOne85",
      };
      new PartTwo.TestEntityTwo85 {
          BooleanField = true,
          Int16Field = 85,
          Int32Field = 85,
          Int64Field = 85,
          FloatField = 85,
          DoubleField = 85,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo85",
          Text = "This is an instance of TestEntityTwo85",
      };
      new PartOne.TestEntityOne86 {
          BooleanField = true,
          Int16Field = 86,
          Int32Field = 86,
          Int64Field = 86,
          FloatField = 86,
          DoubleField = 86,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne86",
          Text = "This is an instance of TestEntityOne86",
      };
      new PartTwo.TestEntityTwo86 {
          BooleanField = true,
          Int16Field = 86,
          Int32Field = 86,
          Int64Field = 86,
          FloatField = 86,
          DoubleField = 86,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo86",
          Text = "This is an instance of TestEntityTwo86",
      };
      new PartOne.TestEntityOne87 {
          BooleanField = true,
          Int16Field = 87,
          Int32Field = 87,
          Int64Field = 87,
          FloatField = 87,
          DoubleField = 87,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne87",
          Text = "This is an instance of TestEntityOne87",
      };
      new PartTwo.TestEntityTwo87 {
          BooleanField = true,
          Int16Field = 87,
          Int32Field = 87,
          Int64Field = 87,
          FloatField = 87,
          DoubleField = 87,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo87",
          Text = "This is an instance of TestEntityTwo87",
      };
      new PartOne.TestEntityOne88 {
          BooleanField = true,
          Int16Field = 88,
          Int32Field = 88,
          Int64Field = 88,
          FloatField = 88,
          DoubleField = 88,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne88",
          Text = "This is an instance of TestEntityOne88",
      };
      new PartTwo.TestEntityTwo88 {
          BooleanField = true,
          Int16Field = 88,
          Int32Field = 88,
          Int64Field = 88,
          FloatField = 88,
          DoubleField = 88,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo88",
          Text = "This is an instance of TestEntityTwo88",
      };
      new PartOne.TestEntityOne89 {
          BooleanField = true,
          Int16Field = 89,
          Int32Field = 89,
          Int64Field = 89,
          FloatField = 89,
          DoubleField = 89,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne89",
          Text = "This is an instance of TestEntityOne89",
      };
      new PartTwo.TestEntityTwo89 {
          BooleanField = true,
          Int16Field = 89,
          Int32Field = 89,
          Int64Field = 89,
          FloatField = 89,
          DoubleField = 89,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo89",
          Text = "This is an instance of TestEntityTwo89",
      };
      new PartOne.TestEntityOne90 {
          BooleanField = true,
          Int16Field = 90,
          Int32Field = 90,
          Int64Field = 90,
          FloatField = 90,
          DoubleField = 90,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne90",
          Text = "This is an instance of TestEntityOne90",
      };
      new PartTwo.TestEntityTwo90 {
          BooleanField = true,
          Int16Field = 90,
          Int32Field = 90,
          Int64Field = 90,
          FloatField = 90,
          DoubleField = 90,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo90",
          Text = "This is an instance of TestEntityTwo90",
      };
      new PartOne.TestEntityOne91 {
          BooleanField = true,
          Int16Field = 91,
          Int32Field = 91,
          Int64Field = 91,
          FloatField = 91,
          DoubleField = 91,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne91",
          Text = "This is an instance of TestEntityOne91",
      };
      new PartTwo.TestEntityTwo91 {
          BooleanField = true,
          Int16Field = 91,
          Int32Field = 91,
          Int64Field = 91,
          FloatField = 91,
          DoubleField = 91,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo91",
          Text = "This is an instance of TestEntityTwo91",
      };
      new PartOne.TestEntityOne92 {
          BooleanField = true,
          Int16Field = 92,
          Int32Field = 92,
          Int64Field = 92,
          FloatField = 92,
          DoubleField = 92,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne92",
          Text = "This is an instance of TestEntityOne92",
      };
      new PartTwo.TestEntityTwo92 {
          BooleanField = true,
          Int16Field = 92,
          Int32Field = 92,
          Int64Field = 92,
          FloatField = 92,
          DoubleField = 92,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo92",
          Text = "This is an instance of TestEntityTwo92",
      };
      new PartOne.TestEntityOne93 {
          BooleanField = true,
          Int16Field = 93,
          Int32Field = 93,
          Int64Field = 93,
          FloatField = 93,
          DoubleField = 93,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne93",
          Text = "This is an instance of TestEntityOne93",
      };
      new PartTwo.TestEntityTwo93 {
          BooleanField = true,
          Int16Field = 93,
          Int32Field = 93,
          Int64Field = 93,
          FloatField = 93,
          DoubleField = 93,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo93",
          Text = "This is an instance of TestEntityTwo93",
      };
      new PartOne.TestEntityOne94 {
          BooleanField = true,
          Int16Field = 94,
          Int32Field = 94,
          Int64Field = 94,
          FloatField = 94,
          DoubleField = 94,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne94",
          Text = "This is an instance of TestEntityOne94",
      };
      new PartTwo.TestEntityTwo94 {
          BooleanField = true,
          Int16Field = 94,
          Int32Field = 94,
          Int64Field = 94,
          FloatField = 94,
          DoubleField = 94,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo94",
          Text = "This is an instance of TestEntityTwo94",
      };
      new PartOne.TestEntityOne95 {
          BooleanField = true,
          Int16Field = 95,
          Int32Field = 95,
          Int64Field = 95,
          FloatField = 95,
          DoubleField = 95,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne95",
          Text = "This is an instance of TestEntityOne95",
      };
      new PartTwo.TestEntityTwo95 {
          BooleanField = true,
          Int16Field = 95,
          Int32Field = 95,
          Int64Field = 95,
          FloatField = 95,
          DoubleField = 95,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo95",
          Text = "This is an instance of TestEntityTwo95",
      };
      new PartOne.TestEntityOne96 {
          BooleanField = true,
          Int16Field = 96,
          Int32Field = 96,
          Int64Field = 96,
          FloatField = 96,
          DoubleField = 96,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne96",
          Text = "This is an instance of TestEntityOne96",
      };
      new PartTwo.TestEntityTwo96 {
          BooleanField = true,
          Int16Field = 96,
          Int32Field = 96,
          Int64Field = 96,
          FloatField = 96,
          DoubleField = 96,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo96",
          Text = "This is an instance of TestEntityTwo96",
      };
      new PartOne.TestEntityOne97 {
          BooleanField = true,
          Int16Field = 97,
          Int32Field = 97,
          Int64Field = 97,
          FloatField = 97,
          DoubleField = 97,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne97",
          Text = "This is an instance of TestEntityOne97",
      };
      new PartTwo.TestEntityTwo97 {
          BooleanField = true,
          Int16Field = 97,
          Int32Field = 97,
          Int64Field = 97,
          FloatField = 97,
          DoubleField = 97,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo97",
          Text = "This is an instance of TestEntityTwo97",
      };
      new PartOne.TestEntityOne98 {
          BooleanField = true,
          Int16Field = 98,
          Int32Field = 98,
          Int64Field = 98,
          FloatField = 98,
          DoubleField = 98,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne98",
          Text = "This is an instance of TestEntityOne98",
      };
      new PartTwo.TestEntityTwo98 {
          BooleanField = true,
          Int16Field = 98,
          Int32Field = 98,
          Int64Field = 98,
          FloatField = 98,
          DoubleField = 98,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo98",
          Text = "This is an instance of TestEntityTwo98",
      };
      new PartOne.TestEntityOne99 {
          BooleanField = true,
          Int16Field = 99,
          Int32Field = 99,
          Int64Field = 99,
          FloatField = 99,
          DoubleField = 99,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityOne99",
          Text = "This is an instance of TestEntityOne99",
      };
      new PartTwo.TestEntityTwo99 {
          BooleanField = true,
          Int16Field = 99,
          Int32Field = 99,
          Int64Field = 99,
          FloatField = 99,
          DoubleField = 99,
          DateTimeField = DateTime.Now.Date,
          StringField = "TestEntityTwo99",
          Text = "This is an instance of TestEntityTwo99",
      };
    }
  }

  public class ModelChecker
  {
    public void Run(Session session)
    {
      var result00 = session.Query.All<PartOne.TestEntityOne0>().ToArray();
      Assert.That(result00.Length, Is.EqualTo(1));
      Assert.That(result00[0].BooleanField, Is.True);
      Assert.That(result00[0].Int16Field, Is.EqualTo(0));
      Assert.That(result00[0].Int32Field, Is.EqualTo(0));
      Assert.That(result00[0].Int64Field, Is.EqualTo(0));
      Assert.That(result00[0].FloatField, Is.EqualTo((float)0));
      Assert.That(result00[0].DoubleField, Is.EqualTo((double)0));
      Assert.That(result00[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result00[0].StringField, Is.EqualTo("TestEntityOne0"));
      Assert.That(result00[0].Text, Is.EqualTo("This is an instance of TestEntityOne0"));

      var result01 = session.Query.All<PartTwo.TestEntityTwo0>().ToArray();
      Assert.That(result01.Length, Is.EqualTo(1));
      Assert.That(result01[0].BooleanField, Is.True);
      Assert.That(result01[0].Int16Field, Is.EqualTo(0));
      Assert.That(result01[0].Int32Field, Is.EqualTo(0));
      Assert.That(result01[0].Int64Field, Is.EqualTo(0));
      Assert.That(result01[0].FloatField, Is.EqualTo((float)0));
      Assert.That(result01[0].DoubleField, Is.EqualTo((double)0));
      Assert.That(result01[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result01[0].StringField, Is.EqualTo("TestEntityTwo0"));
      Assert.That(result01[0].Text, Is.EqualTo("This is an instance of TestEntityTwo0"));
      var result10 = session.Query.All<PartOne.TestEntityOne1>().ToArray();
      Assert.That(result10.Length, Is.EqualTo(1));
      Assert.That(result10[0].BooleanField, Is.True);
      Assert.That(result10[0].Int16Field, Is.EqualTo(1));
      Assert.That(result10[0].Int32Field, Is.EqualTo(1));
      Assert.That(result10[0].Int64Field, Is.EqualTo(1));
      Assert.That(result10[0].FloatField, Is.EqualTo((float)1));
      Assert.That(result10[0].DoubleField, Is.EqualTo((double)1));
      Assert.That(result10[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result10[0].StringField, Is.EqualTo("TestEntityOne1"));
      Assert.That(result10[0].Text, Is.EqualTo("This is an instance of TestEntityOne1"));

      var result11 = session.Query.All<PartTwo.TestEntityTwo1>().ToArray();
      Assert.That(result11.Length, Is.EqualTo(1));
      Assert.That(result11[0].BooleanField, Is.True);
      Assert.That(result11[0].Int16Field, Is.EqualTo(1));
      Assert.That(result11[0].Int32Field, Is.EqualTo(1));
      Assert.That(result11[0].Int64Field, Is.EqualTo(1));
      Assert.That(result11[0].FloatField, Is.EqualTo((float)1));
      Assert.That(result11[0].DoubleField, Is.EqualTo((double)1));
      Assert.That(result11[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result11[0].StringField, Is.EqualTo("TestEntityTwo1"));
      Assert.That(result11[0].Text, Is.EqualTo("This is an instance of TestEntityTwo1"));
      var result20 = session.Query.All<PartOne.TestEntityOne2>().ToArray();
      Assert.That(result20.Length, Is.EqualTo(1));
      Assert.That(result20[0].BooleanField, Is.True);
      Assert.That(result20[0].Int16Field, Is.EqualTo(2));
      Assert.That(result20[0].Int32Field, Is.EqualTo(2));
      Assert.That(result20[0].Int64Field, Is.EqualTo(2));
      Assert.That(result20[0].FloatField, Is.EqualTo((float)2));
      Assert.That(result20[0].DoubleField, Is.EqualTo((double)2));
      Assert.That(result20[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result20[0].StringField, Is.EqualTo("TestEntityOne2"));
      Assert.That(result20[0].Text, Is.EqualTo("This is an instance of TestEntityOne2"));

      var result21 = session.Query.All<PartTwo.TestEntityTwo2>().ToArray();
      Assert.That(result21.Length, Is.EqualTo(1));
      Assert.That(result21[0].BooleanField, Is.True);
      Assert.That(result21[0].Int16Field, Is.EqualTo(2));
      Assert.That(result21[0].Int32Field, Is.EqualTo(2));
      Assert.That(result21[0].Int64Field, Is.EqualTo(2));
      Assert.That(result21[0].FloatField, Is.EqualTo((float)2));
      Assert.That(result21[0].DoubleField, Is.EqualTo((double)2));
      Assert.That(result21[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result21[0].StringField, Is.EqualTo("TestEntityTwo2"));
      Assert.That(result21[0].Text, Is.EqualTo("This is an instance of TestEntityTwo2"));
      var result30 = session.Query.All<PartOne.TestEntityOne3>().ToArray();
      Assert.That(result30.Length, Is.EqualTo(1));
      Assert.That(result30[0].BooleanField, Is.True);
      Assert.That(result30[0].Int16Field, Is.EqualTo(3));
      Assert.That(result30[0].Int32Field, Is.EqualTo(3));
      Assert.That(result30[0].Int64Field, Is.EqualTo(3));
      Assert.That(result30[0].FloatField, Is.EqualTo((float)3));
      Assert.That(result30[0].DoubleField, Is.EqualTo((double)3));
      Assert.That(result30[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result30[0].StringField, Is.EqualTo("TestEntityOne3"));
      Assert.That(result30[0].Text, Is.EqualTo("This is an instance of TestEntityOne3"));

      var result31 = session.Query.All<PartTwo.TestEntityTwo3>().ToArray();
      Assert.That(result31.Length, Is.EqualTo(1));
      Assert.That(result31[0].BooleanField, Is.True);
      Assert.That(result31[0].Int16Field, Is.EqualTo(3));
      Assert.That(result31[0].Int32Field, Is.EqualTo(3));
      Assert.That(result31[0].Int64Field, Is.EqualTo(3));
      Assert.That(result31[0].FloatField, Is.EqualTo((float)3));
      Assert.That(result31[0].DoubleField, Is.EqualTo((double)3));
      Assert.That(result31[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result31[0].StringField, Is.EqualTo("TestEntityTwo3"));
      Assert.That(result31[0].Text, Is.EqualTo("This is an instance of TestEntityTwo3"));
      var result40 = session.Query.All<PartOne.TestEntityOne4>().ToArray();
      Assert.That(result40.Length, Is.EqualTo(1));
      Assert.That(result40[0].BooleanField, Is.True);
      Assert.That(result40[0].Int16Field, Is.EqualTo(4));
      Assert.That(result40[0].Int32Field, Is.EqualTo(4));
      Assert.That(result40[0].Int64Field, Is.EqualTo(4));
      Assert.That(result40[0].FloatField, Is.EqualTo((float)4));
      Assert.That(result40[0].DoubleField, Is.EqualTo((double)4));
      Assert.That(result40[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result40[0].StringField, Is.EqualTo("TestEntityOne4"));
      Assert.That(result40[0].Text, Is.EqualTo("This is an instance of TestEntityOne4"));

      var result41 = session.Query.All<PartTwo.TestEntityTwo4>().ToArray();
      Assert.That(result41.Length, Is.EqualTo(1));
      Assert.That(result41[0].BooleanField, Is.True);
      Assert.That(result41[0].Int16Field, Is.EqualTo(4));
      Assert.That(result41[0].Int32Field, Is.EqualTo(4));
      Assert.That(result41[0].Int64Field, Is.EqualTo(4));
      Assert.That(result41[0].FloatField, Is.EqualTo((float)4));
      Assert.That(result41[0].DoubleField, Is.EqualTo((double)4));
      Assert.That(result41[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result41[0].StringField, Is.EqualTo("TestEntityTwo4"));
      Assert.That(result41[0].Text, Is.EqualTo("This is an instance of TestEntityTwo4"));
      var result50 = session.Query.All<PartOne.TestEntityOne5>().ToArray();
      Assert.That(result50.Length, Is.EqualTo(1));
      Assert.That(result50[0].BooleanField, Is.True);
      Assert.That(result50[0].Int16Field, Is.EqualTo(5));
      Assert.That(result50[0].Int32Field, Is.EqualTo(5));
      Assert.That(result50[0].Int64Field, Is.EqualTo(5));
      Assert.That(result50[0].FloatField, Is.EqualTo((float)5));
      Assert.That(result50[0].DoubleField, Is.EqualTo((double)5));
      Assert.That(result50[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result50[0].StringField, Is.EqualTo("TestEntityOne5"));
      Assert.That(result50[0].Text, Is.EqualTo("This is an instance of TestEntityOne5"));

      var result51 = session.Query.All<PartTwo.TestEntityTwo5>().ToArray();
      Assert.That(result51.Length, Is.EqualTo(1));
      Assert.That(result51[0].BooleanField, Is.True);
      Assert.That(result51[0].Int16Field, Is.EqualTo(5));
      Assert.That(result51[0].Int32Field, Is.EqualTo(5));
      Assert.That(result51[0].Int64Field, Is.EqualTo(5));
      Assert.That(result51[0].FloatField, Is.EqualTo((float)5));
      Assert.That(result51[0].DoubleField, Is.EqualTo((double)5));
      Assert.That(result51[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result51[0].StringField, Is.EqualTo("TestEntityTwo5"));
      Assert.That(result51[0].Text, Is.EqualTo("This is an instance of TestEntityTwo5"));
      var result60 = session.Query.All<PartOne.TestEntityOne6>().ToArray();
      Assert.That(result60.Length, Is.EqualTo(1));
      Assert.That(result60[0].BooleanField, Is.True);
      Assert.That(result60[0].Int16Field, Is.EqualTo(6));
      Assert.That(result60[0].Int32Field, Is.EqualTo(6));
      Assert.That(result60[0].Int64Field, Is.EqualTo(6));
      Assert.That(result60[0].FloatField, Is.EqualTo((float)6));
      Assert.That(result60[0].DoubleField, Is.EqualTo((double)6));
      Assert.That(result60[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result60[0].StringField, Is.EqualTo("TestEntityOne6"));
      Assert.That(result60[0].Text, Is.EqualTo("This is an instance of TestEntityOne6"));

      var result61 = session.Query.All<PartTwo.TestEntityTwo6>().ToArray();
      Assert.That(result61.Length, Is.EqualTo(1));
      Assert.That(result61[0].BooleanField, Is.True);
      Assert.That(result61[0].Int16Field, Is.EqualTo(6));
      Assert.That(result61[0].Int32Field, Is.EqualTo(6));
      Assert.That(result61[0].Int64Field, Is.EqualTo(6));
      Assert.That(result61[0].FloatField, Is.EqualTo((float)6));
      Assert.That(result61[0].DoubleField, Is.EqualTo((double)6));
      Assert.That(result61[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result61[0].StringField, Is.EqualTo("TestEntityTwo6"));
      Assert.That(result61[0].Text, Is.EqualTo("This is an instance of TestEntityTwo6"));
      var result70 = session.Query.All<PartOne.TestEntityOne7>().ToArray();
      Assert.That(result70.Length, Is.EqualTo(1));
      Assert.That(result70[0].BooleanField, Is.True);
      Assert.That(result70[0].Int16Field, Is.EqualTo(7));
      Assert.That(result70[0].Int32Field, Is.EqualTo(7));
      Assert.That(result70[0].Int64Field, Is.EqualTo(7));
      Assert.That(result70[0].FloatField, Is.EqualTo((float)7));
      Assert.That(result70[0].DoubleField, Is.EqualTo((double)7));
      Assert.That(result70[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result70[0].StringField, Is.EqualTo("TestEntityOne7"));
      Assert.That(result70[0].Text, Is.EqualTo("This is an instance of TestEntityOne7"));

      var result71 = session.Query.All<PartTwo.TestEntityTwo7>().ToArray();
      Assert.That(result71.Length, Is.EqualTo(1));
      Assert.That(result71[0].BooleanField, Is.True);
      Assert.That(result71[0].Int16Field, Is.EqualTo(7));
      Assert.That(result71[0].Int32Field, Is.EqualTo(7));
      Assert.That(result71[0].Int64Field, Is.EqualTo(7));
      Assert.That(result71[0].FloatField, Is.EqualTo((float)7));
      Assert.That(result71[0].DoubleField, Is.EqualTo((double)7));
      Assert.That(result71[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result71[0].StringField, Is.EqualTo("TestEntityTwo7"));
      Assert.That(result71[0].Text, Is.EqualTo("This is an instance of TestEntityTwo7"));
      var result80 = session.Query.All<PartOne.TestEntityOne8>().ToArray();
      Assert.That(result80.Length, Is.EqualTo(1));
      Assert.That(result80[0].BooleanField, Is.True);
      Assert.That(result80[0].Int16Field, Is.EqualTo(8));
      Assert.That(result80[0].Int32Field, Is.EqualTo(8));
      Assert.That(result80[0].Int64Field, Is.EqualTo(8));
      Assert.That(result80[0].FloatField, Is.EqualTo((float)8));
      Assert.That(result80[0].DoubleField, Is.EqualTo((double)8));
      Assert.That(result80[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result80[0].StringField, Is.EqualTo("TestEntityOne8"));
      Assert.That(result80[0].Text, Is.EqualTo("This is an instance of TestEntityOne8"));

      var result81 = session.Query.All<PartTwo.TestEntityTwo8>().ToArray();
      Assert.That(result81.Length, Is.EqualTo(1));
      Assert.That(result81[0].BooleanField, Is.True);
      Assert.That(result81[0].Int16Field, Is.EqualTo(8));
      Assert.That(result81[0].Int32Field, Is.EqualTo(8));
      Assert.That(result81[0].Int64Field, Is.EqualTo(8));
      Assert.That(result81[0].FloatField, Is.EqualTo((float)8));
      Assert.That(result81[0].DoubleField, Is.EqualTo((double)8));
      Assert.That(result81[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result81[0].StringField, Is.EqualTo("TestEntityTwo8"));
      Assert.That(result81[0].Text, Is.EqualTo("This is an instance of TestEntityTwo8"));
      var result90 = session.Query.All<PartOne.TestEntityOne9>().ToArray();
      Assert.That(result90.Length, Is.EqualTo(1));
      Assert.That(result90[0].BooleanField, Is.True);
      Assert.That(result90[0].Int16Field, Is.EqualTo(9));
      Assert.That(result90[0].Int32Field, Is.EqualTo(9));
      Assert.That(result90[0].Int64Field, Is.EqualTo(9));
      Assert.That(result90[0].FloatField, Is.EqualTo((float)9));
      Assert.That(result90[0].DoubleField, Is.EqualTo((double)9));
      Assert.That(result90[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result90[0].StringField, Is.EqualTo("TestEntityOne9"));
      Assert.That(result90[0].Text, Is.EqualTo("This is an instance of TestEntityOne9"));

      var result91 = session.Query.All<PartTwo.TestEntityTwo9>().ToArray();
      Assert.That(result91.Length, Is.EqualTo(1));
      Assert.That(result91[0].BooleanField, Is.True);
      Assert.That(result91[0].Int16Field, Is.EqualTo(9));
      Assert.That(result91[0].Int32Field, Is.EqualTo(9));
      Assert.That(result91[0].Int64Field, Is.EqualTo(9));
      Assert.That(result91[0].FloatField, Is.EqualTo((float)9));
      Assert.That(result91[0].DoubleField, Is.EqualTo((double)9));
      Assert.That(result91[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result91[0].StringField, Is.EqualTo("TestEntityTwo9"));
      Assert.That(result91[0].Text, Is.EqualTo("This is an instance of TestEntityTwo9"));
      var result100 = session.Query.All<PartOne.TestEntityOne10>().ToArray();
      Assert.That(result100.Length, Is.EqualTo(1));
      Assert.That(result100[0].BooleanField, Is.True);
      Assert.That(result100[0].Int16Field, Is.EqualTo(10));
      Assert.That(result100[0].Int32Field, Is.EqualTo(10));
      Assert.That(result100[0].Int64Field, Is.EqualTo(10));
      Assert.That(result100[0].FloatField, Is.EqualTo((float)10));
      Assert.That(result100[0].DoubleField, Is.EqualTo((double)10));
      Assert.That(result100[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result100[0].StringField, Is.EqualTo("TestEntityOne10"));
      Assert.That(result100[0].Text, Is.EqualTo("This is an instance of TestEntityOne10"));

      var result101 = session.Query.All<PartTwo.TestEntityTwo10>().ToArray();
      Assert.That(result101.Length, Is.EqualTo(1));
      Assert.That(result101[0].BooleanField, Is.True);
      Assert.That(result101[0].Int16Field, Is.EqualTo(10));
      Assert.That(result101[0].Int32Field, Is.EqualTo(10));
      Assert.That(result101[0].Int64Field, Is.EqualTo(10));
      Assert.That(result101[0].FloatField, Is.EqualTo((float)10));
      Assert.That(result101[0].DoubleField, Is.EqualTo((double)10));
      Assert.That(result101[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result101[0].StringField, Is.EqualTo("TestEntityTwo10"));
      Assert.That(result101[0].Text, Is.EqualTo("This is an instance of TestEntityTwo10"));
      var result110 = session.Query.All<PartOne.TestEntityOne11>().ToArray();
      Assert.That(result110.Length, Is.EqualTo(1));
      Assert.That(result110[0].BooleanField, Is.True);
      Assert.That(result110[0].Int16Field, Is.EqualTo(11));
      Assert.That(result110[0].Int32Field, Is.EqualTo(11));
      Assert.That(result110[0].Int64Field, Is.EqualTo(11));
      Assert.That(result110[0].FloatField, Is.EqualTo((float)11));
      Assert.That(result110[0].DoubleField, Is.EqualTo((double)11));
      Assert.That(result110[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result110[0].StringField, Is.EqualTo("TestEntityOne11"));
      Assert.That(result110[0].Text, Is.EqualTo("This is an instance of TestEntityOne11"));

      var result111 = session.Query.All<PartTwo.TestEntityTwo11>().ToArray();
      Assert.That(result111.Length, Is.EqualTo(1));
      Assert.That(result111[0].BooleanField, Is.True);
      Assert.That(result111[0].Int16Field, Is.EqualTo(11));
      Assert.That(result111[0].Int32Field, Is.EqualTo(11));
      Assert.That(result111[0].Int64Field, Is.EqualTo(11));
      Assert.That(result111[0].FloatField, Is.EqualTo((float)11));
      Assert.That(result111[0].DoubleField, Is.EqualTo((double)11));
      Assert.That(result111[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result111[0].StringField, Is.EqualTo("TestEntityTwo11"));
      Assert.That(result111[0].Text, Is.EqualTo("This is an instance of TestEntityTwo11"));
      var result120 = session.Query.All<PartOne.TestEntityOne12>().ToArray();
      Assert.That(result120.Length, Is.EqualTo(1));
      Assert.That(result120[0].BooleanField, Is.True);
      Assert.That(result120[0].Int16Field, Is.EqualTo(12));
      Assert.That(result120[0].Int32Field, Is.EqualTo(12));
      Assert.That(result120[0].Int64Field, Is.EqualTo(12));
      Assert.That(result120[0].FloatField, Is.EqualTo((float)12));
      Assert.That(result120[0].DoubleField, Is.EqualTo((double)12));
      Assert.That(result120[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result120[0].StringField, Is.EqualTo("TestEntityOne12"));
      Assert.That(result120[0].Text, Is.EqualTo("This is an instance of TestEntityOne12"));

      var result121 = session.Query.All<PartTwo.TestEntityTwo12>().ToArray();
      Assert.That(result121.Length, Is.EqualTo(1));
      Assert.That(result121[0].BooleanField, Is.True);
      Assert.That(result121[0].Int16Field, Is.EqualTo(12));
      Assert.That(result121[0].Int32Field, Is.EqualTo(12));
      Assert.That(result121[0].Int64Field, Is.EqualTo(12));
      Assert.That(result121[0].FloatField, Is.EqualTo((float)12));
      Assert.That(result121[0].DoubleField, Is.EqualTo((double)12));
      Assert.That(result121[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result121[0].StringField, Is.EqualTo("TestEntityTwo12"));
      Assert.That(result121[0].Text, Is.EqualTo("This is an instance of TestEntityTwo12"));
      var result130 = session.Query.All<PartOne.TestEntityOne13>().ToArray();
      Assert.That(result130.Length, Is.EqualTo(1));
      Assert.That(result130[0].BooleanField, Is.True);
      Assert.That(result130[0].Int16Field, Is.EqualTo(13));
      Assert.That(result130[0].Int32Field, Is.EqualTo(13));
      Assert.That(result130[0].Int64Field, Is.EqualTo(13));
      Assert.That(result130[0].FloatField, Is.EqualTo((float)13));
      Assert.That(result130[0].DoubleField, Is.EqualTo((double)13));
      Assert.That(result130[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result130[0].StringField, Is.EqualTo("TestEntityOne13"));
      Assert.That(result130[0].Text, Is.EqualTo("This is an instance of TestEntityOne13"));

      var result131 = session.Query.All<PartTwo.TestEntityTwo13>().ToArray();
      Assert.That(result131.Length, Is.EqualTo(1));
      Assert.That(result131[0].BooleanField, Is.True);
      Assert.That(result131[0].Int16Field, Is.EqualTo(13));
      Assert.That(result131[0].Int32Field, Is.EqualTo(13));
      Assert.That(result131[0].Int64Field, Is.EqualTo(13));
      Assert.That(result131[0].FloatField, Is.EqualTo((float)13));
      Assert.That(result131[0].DoubleField, Is.EqualTo((double)13));
      Assert.That(result131[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result131[0].StringField, Is.EqualTo("TestEntityTwo13"));
      Assert.That(result131[0].Text, Is.EqualTo("This is an instance of TestEntityTwo13"));
      var result140 = session.Query.All<PartOne.TestEntityOne14>().ToArray();
      Assert.That(result140.Length, Is.EqualTo(1));
      Assert.That(result140[0].BooleanField, Is.True);
      Assert.That(result140[0].Int16Field, Is.EqualTo(14));
      Assert.That(result140[0].Int32Field, Is.EqualTo(14));
      Assert.That(result140[0].Int64Field, Is.EqualTo(14));
      Assert.That(result140[0].FloatField, Is.EqualTo((float)14));
      Assert.That(result140[0].DoubleField, Is.EqualTo((double)14));
      Assert.That(result140[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result140[0].StringField, Is.EqualTo("TestEntityOne14"));
      Assert.That(result140[0].Text, Is.EqualTo("This is an instance of TestEntityOne14"));

      var result141 = session.Query.All<PartTwo.TestEntityTwo14>().ToArray();
      Assert.That(result141.Length, Is.EqualTo(1));
      Assert.That(result141[0].BooleanField, Is.True);
      Assert.That(result141[0].Int16Field, Is.EqualTo(14));
      Assert.That(result141[0].Int32Field, Is.EqualTo(14));
      Assert.That(result141[0].Int64Field, Is.EqualTo(14));
      Assert.That(result141[0].FloatField, Is.EqualTo((float)14));
      Assert.That(result141[0].DoubleField, Is.EqualTo((double)14));
      Assert.That(result141[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result141[0].StringField, Is.EqualTo("TestEntityTwo14"));
      Assert.That(result141[0].Text, Is.EqualTo("This is an instance of TestEntityTwo14"));
      var result150 = session.Query.All<PartOne.TestEntityOne15>().ToArray();
      Assert.That(result150.Length, Is.EqualTo(1));
      Assert.That(result150[0].BooleanField, Is.True);
      Assert.That(result150[0].Int16Field, Is.EqualTo(15));
      Assert.That(result150[0].Int32Field, Is.EqualTo(15));
      Assert.That(result150[0].Int64Field, Is.EqualTo(15));
      Assert.That(result150[0].FloatField, Is.EqualTo((float)15));
      Assert.That(result150[0].DoubleField, Is.EqualTo((double)15));
      Assert.That(result150[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result150[0].StringField, Is.EqualTo("TestEntityOne15"));
      Assert.That(result150[0].Text, Is.EqualTo("This is an instance of TestEntityOne15"));

      var result151 = session.Query.All<PartTwo.TestEntityTwo15>().ToArray();
      Assert.That(result151.Length, Is.EqualTo(1));
      Assert.That(result151[0].BooleanField, Is.True);
      Assert.That(result151[0].Int16Field, Is.EqualTo(15));
      Assert.That(result151[0].Int32Field, Is.EqualTo(15));
      Assert.That(result151[0].Int64Field, Is.EqualTo(15));
      Assert.That(result151[0].FloatField, Is.EqualTo((float)15));
      Assert.That(result151[0].DoubleField, Is.EqualTo((double)15));
      Assert.That(result151[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result151[0].StringField, Is.EqualTo("TestEntityTwo15"));
      Assert.That(result151[0].Text, Is.EqualTo("This is an instance of TestEntityTwo15"));
      var result160 = session.Query.All<PartOne.TestEntityOne16>().ToArray();
      Assert.That(result160.Length, Is.EqualTo(1));
      Assert.That(result160[0].BooleanField, Is.True);
      Assert.That(result160[0].Int16Field, Is.EqualTo(16));
      Assert.That(result160[0].Int32Field, Is.EqualTo(16));
      Assert.That(result160[0].Int64Field, Is.EqualTo(16));
      Assert.That(result160[0].FloatField, Is.EqualTo((float)16));
      Assert.That(result160[0].DoubleField, Is.EqualTo((double)16));
      Assert.That(result160[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result160[0].StringField, Is.EqualTo("TestEntityOne16"));
      Assert.That(result160[0].Text, Is.EqualTo("This is an instance of TestEntityOne16"));

      var result161 = session.Query.All<PartTwo.TestEntityTwo16>().ToArray();
      Assert.That(result161.Length, Is.EqualTo(1));
      Assert.That(result161[0].BooleanField, Is.True);
      Assert.That(result161[0].Int16Field, Is.EqualTo(16));
      Assert.That(result161[0].Int32Field, Is.EqualTo(16));
      Assert.That(result161[0].Int64Field, Is.EqualTo(16));
      Assert.That(result161[0].FloatField, Is.EqualTo((float)16));
      Assert.That(result161[0].DoubleField, Is.EqualTo((double)16));
      Assert.That(result161[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result161[0].StringField, Is.EqualTo("TestEntityTwo16"));
      Assert.That(result161[0].Text, Is.EqualTo("This is an instance of TestEntityTwo16"));
      var result170 = session.Query.All<PartOne.TestEntityOne17>().ToArray();
      Assert.That(result170.Length, Is.EqualTo(1));
      Assert.That(result170[0].BooleanField, Is.True);
      Assert.That(result170[0].Int16Field, Is.EqualTo(17));
      Assert.That(result170[0].Int32Field, Is.EqualTo(17));
      Assert.That(result170[0].Int64Field, Is.EqualTo(17));
      Assert.That(result170[0].FloatField, Is.EqualTo((float)17));
      Assert.That(result170[0].DoubleField, Is.EqualTo((double)17));
      Assert.That(result170[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result170[0].StringField, Is.EqualTo("TestEntityOne17"));
      Assert.That(result170[0].Text, Is.EqualTo("This is an instance of TestEntityOne17"));

      var result171 = session.Query.All<PartTwo.TestEntityTwo17>().ToArray();
      Assert.That(result171.Length, Is.EqualTo(1));
      Assert.That(result171[0].BooleanField, Is.True);
      Assert.That(result171[0].Int16Field, Is.EqualTo(17));
      Assert.That(result171[0].Int32Field, Is.EqualTo(17));
      Assert.That(result171[0].Int64Field, Is.EqualTo(17));
      Assert.That(result171[0].FloatField, Is.EqualTo((float)17));
      Assert.That(result171[0].DoubleField, Is.EqualTo((double)17));
      Assert.That(result171[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result171[0].StringField, Is.EqualTo("TestEntityTwo17"));
      Assert.That(result171[0].Text, Is.EqualTo("This is an instance of TestEntityTwo17"));
      var result180 = session.Query.All<PartOne.TestEntityOne18>().ToArray();
      Assert.That(result180.Length, Is.EqualTo(1));
      Assert.That(result180[0].BooleanField, Is.True);
      Assert.That(result180[0].Int16Field, Is.EqualTo(18));
      Assert.That(result180[0].Int32Field, Is.EqualTo(18));
      Assert.That(result180[0].Int64Field, Is.EqualTo(18));
      Assert.That(result180[0].FloatField, Is.EqualTo((float)18));
      Assert.That(result180[0].DoubleField, Is.EqualTo((double)18));
      Assert.That(result180[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result180[0].StringField, Is.EqualTo("TestEntityOne18"));
      Assert.That(result180[0].Text, Is.EqualTo("This is an instance of TestEntityOne18"));

      var result181 = session.Query.All<PartTwo.TestEntityTwo18>().ToArray();
      Assert.That(result181.Length, Is.EqualTo(1));
      Assert.That(result181[0].BooleanField, Is.True);
      Assert.That(result181[0].Int16Field, Is.EqualTo(18));
      Assert.That(result181[0].Int32Field, Is.EqualTo(18));
      Assert.That(result181[0].Int64Field, Is.EqualTo(18));
      Assert.That(result181[0].FloatField, Is.EqualTo((float)18));
      Assert.That(result181[0].DoubleField, Is.EqualTo((double)18));
      Assert.That(result181[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result181[0].StringField, Is.EqualTo("TestEntityTwo18"));
      Assert.That(result181[0].Text, Is.EqualTo("This is an instance of TestEntityTwo18"));
      var result190 = session.Query.All<PartOne.TestEntityOne19>().ToArray();
      Assert.That(result190.Length, Is.EqualTo(1));
      Assert.That(result190[0].BooleanField, Is.True);
      Assert.That(result190[0].Int16Field, Is.EqualTo(19));
      Assert.That(result190[0].Int32Field, Is.EqualTo(19));
      Assert.That(result190[0].Int64Field, Is.EqualTo(19));
      Assert.That(result190[0].FloatField, Is.EqualTo((float)19));
      Assert.That(result190[0].DoubleField, Is.EqualTo((double)19));
      Assert.That(result190[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result190[0].StringField, Is.EqualTo("TestEntityOne19"));
      Assert.That(result190[0].Text, Is.EqualTo("This is an instance of TestEntityOne19"));

      var result191 = session.Query.All<PartTwo.TestEntityTwo19>().ToArray();
      Assert.That(result191.Length, Is.EqualTo(1));
      Assert.That(result191[0].BooleanField, Is.True);
      Assert.That(result191[0].Int16Field, Is.EqualTo(19));
      Assert.That(result191[0].Int32Field, Is.EqualTo(19));
      Assert.That(result191[0].Int64Field, Is.EqualTo(19));
      Assert.That(result191[0].FloatField, Is.EqualTo((float)19));
      Assert.That(result191[0].DoubleField, Is.EqualTo((double)19));
      Assert.That(result191[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result191[0].StringField, Is.EqualTo("TestEntityTwo19"));
      Assert.That(result191[0].Text, Is.EqualTo("This is an instance of TestEntityTwo19"));
      var result200 = session.Query.All<PartOne.TestEntityOne20>().ToArray();
      Assert.That(result200.Length, Is.EqualTo(1));
      Assert.That(result200[0].BooleanField, Is.True);
      Assert.That(result200[0].Int16Field, Is.EqualTo(20));
      Assert.That(result200[0].Int32Field, Is.EqualTo(20));
      Assert.That(result200[0].Int64Field, Is.EqualTo(20));
      Assert.That(result200[0].FloatField, Is.EqualTo((float)20));
      Assert.That(result200[0].DoubleField, Is.EqualTo((double)20));
      Assert.That(result200[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result200[0].StringField, Is.EqualTo("TestEntityOne20"));
      Assert.That(result200[0].Text, Is.EqualTo("This is an instance of TestEntityOne20"));

      var result201 = session.Query.All<PartTwo.TestEntityTwo20>().ToArray();
      Assert.That(result201.Length, Is.EqualTo(1));
      Assert.That(result201[0].BooleanField, Is.True);
      Assert.That(result201[0].Int16Field, Is.EqualTo(20));
      Assert.That(result201[0].Int32Field, Is.EqualTo(20));
      Assert.That(result201[0].Int64Field, Is.EqualTo(20));
      Assert.That(result201[0].FloatField, Is.EqualTo((float)20));
      Assert.That(result201[0].DoubleField, Is.EqualTo((double)20));
      Assert.That(result201[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result201[0].StringField, Is.EqualTo("TestEntityTwo20"));
      Assert.That(result201[0].Text, Is.EqualTo("This is an instance of TestEntityTwo20"));
      var result210 = session.Query.All<PartOne.TestEntityOne21>().ToArray();
      Assert.That(result210.Length, Is.EqualTo(1));
      Assert.That(result210[0].BooleanField, Is.True);
      Assert.That(result210[0].Int16Field, Is.EqualTo(21));
      Assert.That(result210[0].Int32Field, Is.EqualTo(21));
      Assert.That(result210[0].Int64Field, Is.EqualTo(21));
      Assert.That(result210[0].FloatField, Is.EqualTo((float)21));
      Assert.That(result210[0].DoubleField, Is.EqualTo((double)21));
      Assert.That(result210[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result210[0].StringField, Is.EqualTo("TestEntityOne21"));
      Assert.That(result210[0].Text, Is.EqualTo("This is an instance of TestEntityOne21"));

      var result211 = session.Query.All<PartTwo.TestEntityTwo21>().ToArray();
      Assert.That(result211.Length, Is.EqualTo(1));
      Assert.That(result211[0].BooleanField, Is.True);
      Assert.That(result211[0].Int16Field, Is.EqualTo(21));
      Assert.That(result211[0].Int32Field, Is.EqualTo(21));
      Assert.That(result211[0].Int64Field, Is.EqualTo(21));
      Assert.That(result211[0].FloatField, Is.EqualTo((float)21));
      Assert.That(result211[0].DoubleField, Is.EqualTo((double)21));
      Assert.That(result211[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result211[0].StringField, Is.EqualTo("TestEntityTwo21"));
      Assert.That(result211[0].Text, Is.EqualTo("This is an instance of TestEntityTwo21"));
      var result220 = session.Query.All<PartOne.TestEntityOne22>().ToArray();
      Assert.That(result220.Length, Is.EqualTo(1));
      Assert.That(result220[0].BooleanField, Is.True);
      Assert.That(result220[0].Int16Field, Is.EqualTo(22));
      Assert.That(result220[0].Int32Field, Is.EqualTo(22));
      Assert.That(result220[0].Int64Field, Is.EqualTo(22));
      Assert.That(result220[0].FloatField, Is.EqualTo((float)22));
      Assert.That(result220[0].DoubleField, Is.EqualTo((double)22));
      Assert.That(result220[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result220[0].StringField, Is.EqualTo("TestEntityOne22"));
      Assert.That(result220[0].Text, Is.EqualTo("This is an instance of TestEntityOne22"));

      var result221 = session.Query.All<PartTwo.TestEntityTwo22>().ToArray();
      Assert.That(result221.Length, Is.EqualTo(1));
      Assert.That(result221[0].BooleanField, Is.True);
      Assert.That(result221[0].Int16Field, Is.EqualTo(22));
      Assert.That(result221[0].Int32Field, Is.EqualTo(22));
      Assert.That(result221[0].Int64Field, Is.EqualTo(22));
      Assert.That(result221[0].FloatField, Is.EqualTo((float)22));
      Assert.That(result221[0].DoubleField, Is.EqualTo((double)22));
      Assert.That(result221[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result221[0].StringField, Is.EqualTo("TestEntityTwo22"));
      Assert.That(result221[0].Text, Is.EqualTo("This is an instance of TestEntityTwo22"));
      var result230 = session.Query.All<PartOne.TestEntityOne23>().ToArray();
      Assert.That(result230.Length, Is.EqualTo(1));
      Assert.That(result230[0].BooleanField, Is.True);
      Assert.That(result230[0].Int16Field, Is.EqualTo(23));
      Assert.That(result230[0].Int32Field, Is.EqualTo(23));
      Assert.That(result230[0].Int64Field, Is.EqualTo(23));
      Assert.That(result230[0].FloatField, Is.EqualTo((float)23));
      Assert.That(result230[0].DoubleField, Is.EqualTo((double)23));
      Assert.That(result230[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result230[0].StringField, Is.EqualTo("TestEntityOne23"));
      Assert.That(result230[0].Text, Is.EqualTo("This is an instance of TestEntityOne23"));

      var result231 = session.Query.All<PartTwo.TestEntityTwo23>().ToArray();
      Assert.That(result231.Length, Is.EqualTo(1));
      Assert.That(result231[0].BooleanField, Is.True);
      Assert.That(result231[0].Int16Field, Is.EqualTo(23));
      Assert.That(result231[0].Int32Field, Is.EqualTo(23));
      Assert.That(result231[0].Int64Field, Is.EqualTo(23));
      Assert.That(result231[0].FloatField, Is.EqualTo((float)23));
      Assert.That(result231[0].DoubleField, Is.EqualTo((double)23));
      Assert.That(result231[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result231[0].StringField, Is.EqualTo("TestEntityTwo23"));
      Assert.That(result231[0].Text, Is.EqualTo("This is an instance of TestEntityTwo23"));
      var result240 = session.Query.All<PartOne.TestEntityOne24>().ToArray();
      Assert.That(result240.Length, Is.EqualTo(1));
      Assert.That(result240[0].BooleanField, Is.True);
      Assert.That(result240[0].Int16Field, Is.EqualTo(24));
      Assert.That(result240[0].Int32Field, Is.EqualTo(24));
      Assert.That(result240[0].Int64Field, Is.EqualTo(24));
      Assert.That(result240[0].FloatField, Is.EqualTo((float)24));
      Assert.That(result240[0].DoubleField, Is.EqualTo((double)24));
      Assert.That(result240[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result240[0].StringField, Is.EqualTo("TestEntityOne24"));
      Assert.That(result240[0].Text, Is.EqualTo("This is an instance of TestEntityOne24"));

      var result241 = session.Query.All<PartTwo.TestEntityTwo24>().ToArray();
      Assert.That(result241.Length, Is.EqualTo(1));
      Assert.That(result241[0].BooleanField, Is.True);
      Assert.That(result241[0].Int16Field, Is.EqualTo(24));
      Assert.That(result241[0].Int32Field, Is.EqualTo(24));
      Assert.That(result241[0].Int64Field, Is.EqualTo(24));
      Assert.That(result241[0].FloatField, Is.EqualTo((float)24));
      Assert.That(result241[0].DoubleField, Is.EqualTo((double)24));
      Assert.That(result241[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result241[0].StringField, Is.EqualTo("TestEntityTwo24"));
      Assert.That(result241[0].Text, Is.EqualTo("This is an instance of TestEntityTwo24"));
      var result250 = session.Query.All<PartOne.TestEntityOne25>().ToArray();
      Assert.That(result250.Length, Is.EqualTo(1));
      Assert.That(result250[0].BooleanField, Is.True);
      Assert.That(result250[0].Int16Field, Is.EqualTo(25));
      Assert.That(result250[0].Int32Field, Is.EqualTo(25));
      Assert.That(result250[0].Int64Field, Is.EqualTo(25));
      Assert.That(result250[0].FloatField, Is.EqualTo((float)25));
      Assert.That(result250[0].DoubleField, Is.EqualTo((double)25));
      Assert.That(result250[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result250[0].StringField, Is.EqualTo("TestEntityOne25"));
      Assert.That(result250[0].Text, Is.EqualTo("This is an instance of TestEntityOne25"));

      var result251 = session.Query.All<PartTwo.TestEntityTwo25>().ToArray();
      Assert.That(result251.Length, Is.EqualTo(1));
      Assert.That(result251[0].BooleanField, Is.True);
      Assert.That(result251[0].Int16Field, Is.EqualTo(25));
      Assert.That(result251[0].Int32Field, Is.EqualTo(25));
      Assert.That(result251[0].Int64Field, Is.EqualTo(25));
      Assert.That(result251[0].FloatField, Is.EqualTo((float)25));
      Assert.That(result251[0].DoubleField, Is.EqualTo((double)25));
      Assert.That(result251[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result251[0].StringField, Is.EqualTo("TestEntityTwo25"));
      Assert.That(result251[0].Text, Is.EqualTo("This is an instance of TestEntityTwo25"));
      var result260 = session.Query.All<PartOne.TestEntityOne26>().ToArray();
      Assert.That(result260.Length, Is.EqualTo(1));
      Assert.That(result260[0].BooleanField, Is.True);
      Assert.That(result260[0].Int16Field, Is.EqualTo(26));
      Assert.That(result260[0].Int32Field, Is.EqualTo(26));
      Assert.That(result260[0].Int64Field, Is.EqualTo(26));
      Assert.That(result260[0].FloatField, Is.EqualTo((float)26));
      Assert.That(result260[0].DoubleField, Is.EqualTo((double)26));
      Assert.That(result260[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result260[0].StringField, Is.EqualTo("TestEntityOne26"));
      Assert.That(result260[0].Text, Is.EqualTo("This is an instance of TestEntityOne26"));

      var result261 = session.Query.All<PartTwo.TestEntityTwo26>().ToArray();
      Assert.That(result261.Length, Is.EqualTo(1));
      Assert.That(result261[0].BooleanField, Is.True);
      Assert.That(result261[0].Int16Field, Is.EqualTo(26));
      Assert.That(result261[0].Int32Field, Is.EqualTo(26));
      Assert.That(result261[0].Int64Field, Is.EqualTo(26));
      Assert.That(result261[0].FloatField, Is.EqualTo((float)26));
      Assert.That(result261[0].DoubleField, Is.EqualTo((double)26));
      Assert.That(result261[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result261[0].StringField, Is.EqualTo("TestEntityTwo26"));
      Assert.That(result261[0].Text, Is.EqualTo("This is an instance of TestEntityTwo26"));
      var result270 = session.Query.All<PartOne.TestEntityOne27>().ToArray();
      Assert.That(result270.Length, Is.EqualTo(1));
      Assert.That(result270[0].BooleanField, Is.True);
      Assert.That(result270[0].Int16Field, Is.EqualTo(27));
      Assert.That(result270[0].Int32Field, Is.EqualTo(27));
      Assert.That(result270[0].Int64Field, Is.EqualTo(27));
      Assert.That(result270[0].FloatField, Is.EqualTo((float)27));
      Assert.That(result270[0].DoubleField, Is.EqualTo((double)27));
      Assert.That(result270[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result270[0].StringField, Is.EqualTo("TestEntityOne27"));
      Assert.That(result270[0].Text, Is.EqualTo("This is an instance of TestEntityOne27"));

      var result271 = session.Query.All<PartTwo.TestEntityTwo27>().ToArray();
      Assert.That(result271.Length, Is.EqualTo(1));
      Assert.That(result271[0].BooleanField, Is.True);
      Assert.That(result271[0].Int16Field, Is.EqualTo(27));
      Assert.That(result271[0].Int32Field, Is.EqualTo(27));
      Assert.That(result271[0].Int64Field, Is.EqualTo(27));
      Assert.That(result271[0].FloatField, Is.EqualTo((float)27));
      Assert.That(result271[0].DoubleField, Is.EqualTo((double)27));
      Assert.That(result271[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result271[0].StringField, Is.EqualTo("TestEntityTwo27"));
      Assert.That(result271[0].Text, Is.EqualTo("This is an instance of TestEntityTwo27"));
      var result280 = session.Query.All<PartOne.TestEntityOne28>().ToArray();
      Assert.That(result280.Length, Is.EqualTo(1));
      Assert.That(result280[0].BooleanField, Is.True);
      Assert.That(result280[0].Int16Field, Is.EqualTo(28));
      Assert.That(result280[0].Int32Field, Is.EqualTo(28));
      Assert.That(result280[0].Int64Field, Is.EqualTo(28));
      Assert.That(result280[0].FloatField, Is.EqualTo((float)28));
      Assert.That(result280[0].DoubleField, Is.EqualTo((double)28));
      Assert.That(result280[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result280[0].StringField, Is.EqualTo("TestEntityOne28"));
      Assert.That(result280[0].Text, Is.EqualTo("This is an instance of TestEntityOne28"));

      var result281 = session.Query.All<PartTwo.TestEntityTwo28>().ToArray();
      Assert.That(result281.Length, Is.EqualTo(1));
      Assert.That(result281[0].BooleanField, Is.True);
      Assert.That(result281[0].Int16Field, Is.EqualTo(28));
      Assert.That(result281[0].Int32Field, Is.EqualTo(28));
      Assert.That(result281[0].Int64Field, Is.EqualTo(28));
      Assert.That(result281[0].FloatField, Is.EqualTo((float)28));
      Assert.That(result281[0].DoubleField, Is.EqualTo((double)28));
      Assert.That(result281[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result281[0].StringField, Is.EqualTo("TestEntityTwo28"));
      Assert.That(result281[0].Text, Is.EqualTo("This is an instance of TestEntityTwo28"));
      var result290 = session.Query.All<PartOne.TestEntityOne29>().ToArray();
      Assert.That(result290.Length, Is.EqualTo(1));
      Assert.That(result290[0].BooleanField, Is.True);
      Assert.That(result290[0].Int16Field, Is.EqualTo(29));
      Assert.That(result290[0].Int32Field, Is.EqualTo(29));
      Assert.That(result290[0].Int64Field, Is.EqualTo(29));
      Assert.That(result290[0].FloatField, Is.EqualTo((float)29));
      Assert.That(result290[0].DoubleField, Is.EqualTo((double)29));
      Assert.That(result290[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result290[0].StringField, Is.EqualTo("TestEntityOne29"));
      Assert.That(result290[0].Text, Is.EqualTo("This is an instance of TestEntityOne29"));

      var result291 = session.Query.All<PartTwo.TestEntityTwo29>().ToArray();
      Assert.That(result291.Length, Is.EqualTo(1));
      Assert.That(result291[0].BooleanField, Is.True);
      Assert.That(result291[0].Int16Field, Is.EqualTo(29));
      Assert.That(result291[0].Int32Field, Is.EqualTo(29));
      Assert.That(result291[0].Int64Field, Is.EqualTo(29));
      Assert.That(result291[0].FloatField, Is.EqualTo((float)29));
      Assert.That(result291[0].DoubleField, Is.EqualTo((double)29));
      Assert.That(result291[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result291[0].StringField, Is.EqualTo("TestEntityTwo29"));
      Assert.That(result291[0].Text, Is.EqualTo("This is an instance of TestEntityTwo29"));
      var result300 = session.Query.All<PartOne.TestEntityOne30>().ToArray();
      Assert.That(result300.Length, Is.EqualTo(1));
      Assert.That(result300[0].BooleanField, Is.True);
      Assert.That(result300[0].Int16Field, Is.EqualTo(30));
      Assert.That(result300[0].Int32Field, Is.EqualTo(30));
      Assert.That(result300[0].Int64Field, Is.EqualTo(30));
      Assert.That(result300[0].FloatField, Is.EqualTo((float)30));
      Assert.That(result300[0].DoubleField, Is.EqualTo((double)30));
      Assert.That(result300[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result300[0].StringField, Is.EqualTo("TestEntityOne30"));
      Assert.That(result300[0].Text, Is.EqualTo("This is an instance of TestEntityOne30"));

      var result301 = session.Query.All<PartTwo.TestEntityTwo30>().ToArray();
      Assert.That(result301.Length, Is.EqualTo(1));
      Assert.That(result301[0].BooleanField, Is.True);
      Assert.That(result301[0].Int16Field, Is.EqualTo(30));
      Assert.That(result301[0].Int32Field, Is.EqualTo(30));
      Assert.That(result301[0].Int64Field, Is.EqualTo(30));
      Assert.That(result301[0].FloatField, Is.EqualTo((float)30));
      Assert.That(result301[0].DoubleField, Is.EqualTo((double)30));
      Assert.That(result301[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result301[0].StringField, Is.EqualTo("TestEntityTwo30"));
      Assert.That(result301[0].Text, Is.EqualTo("This is an instance of TestEntityTwo30"));
      var result310 = session.Query.All<PartOne.TestEntityOne31>().ToArray();
      Assert.That(result310.Length, Is.EqualTo(1));
      Assert.That(result310[0].BooleanField, Is.True);
      Assert.That(result310[0].Int16Field, Is.EqualTo(31));
      Assert.That(result310[0].Int32Field, Is.EqualTo(31));
      Assert.That(result310[0].Int64Field, Is.EqualTo(31));
      Assert.That(result310[0].FloatField, Is.EqualTo((float)31));
      Assert.That(result310[0].DoubleField, Is.EqualTo((double)31));
      Assert.That(result310[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result310[0].StringField, Is.EqualTo("TestEntityOne31"));
      Assert.That(result310[0].Text, Is.EqualTo("This is an instance of TestEntityOne31"));

      var result311 = session.Query.All<PartTwo.TestEntityTwo31>().ToArray();
      Assert.That(result311.Length, Is.EqualTo(1));
      Assert.That(result311[0].BooleanField, Is.True);
      Assert.That(result311[0].Int16Field, Is.EqualTo(31));
      Assert.That(result311[0].Int32Field, Is.EqualTo(31));
      Assert.That(result311[0].Int64Field, Is.EqualTo(31));
      Assert.That(result311[0].FloatField, Is.EqualTo((float)31));
      Assert.That(result311[0].DoubleField, Is.EqualTo((double)31));
      Assert.That(result311[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result311[0].StringField, Is.EqualTo("TestEntityTwo31"));
      Assert.That(result311[0].Text, Is.EqualTo("This is an instance of TestEntityTwo31"));
      var result320 = session.Query.All<PartOne.TestEntityOne32>().ToArray();
      Assert.That(result320.Length, Is.EqualTo(1));
      Assert.That(result320[0].BooleanField, Is.True);
      Assert.That(result320[0].Int16Field, Is.EqualTo(32));
      Assert.That(result320[0].Int32Field, Is.EqualTo(32));
      Assert.That(result320[0].Int64Field, Is.EqualTo(32));
      Assert.That(result320[0].FloatField, Is.EqualTo((float)32));
      Assert.That(result320[0].DoubleField, Is.EqualTo((double)32));
      Assert.That(result320[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result320[0].StringField, Is.EqualTo("TestEntityOne32"));
      Assert.That(result320[0].Text, Is.EqualTo("This is an instance of TestEntityOne32"));

      var result321 = session.Query.All<PartTwo.TestEntityTwo32>().ToArray();
      Assert.That(result321.Length, Is.EqualTo(1));
      Assert.That(result321[0].BooleanField, Is.True);
      Assert.That(result321[0].Int16Field, Is.EqualTo(32));
      Assert.That(result321[0].Int32Field, Is.EqualTo(32));
      Assert.That(result321[0].Int64Field, Is.EqualTo(32));
      Assert.That(result321[0].FloatField, Is.EqualTo((float)32));
      Assert.That(result321[0].DoubleField, Is.EqualTo((double)32));
      Assert.That(result321[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result321[0].StringField, Is.EqualTo("TestEntityTwo32"));
      Assert.That(result321[0].Text, Is.EqualTo("This is an instance of TestEntityTwo32"));
      var result330 = session.Query.All<PartOne.TestEntityOne33>().ToArray();
      Assert.That(result330.Length, Is.EqualTo(1));
      Assert.That(result330[0].BooleanField, Is.True);
      Assert.That(result330[0].Int16Field, Is.EqualTo(33));
      Assert.That(result330[0].Int32Field, Is.EqualTo(33));
      Assert.That(result330[0].Int64Field, Is.EqualTo(33));
      Assert.That(result330[0].FloatField, Is.EqualTo((float)33));
      Assert.That(result330[0].DoubleField, Is.EqualTo((double)33));
      Assert.That(result330[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result330[0].StringField, Is.EqualTo("TestEntityOne33"));
      Assert.That(result330[0].Text, Is.EqualTo("This is an instance of TestEntityOne33"));

      var result331 = session.Query.All<PartTwo.TestEntityTwo33>().ToArray();
      Assert.That(result331.Length, Is.EqualTo(1));
      Assert.That(result331[0].BooleanField, Is.True);
      Assert.That(result331[0].Int16Field, Is.EqualTo(33));
      Assert.That(result331[0].Int32Field, Is.EqualTo(33));
      Assert.That(result331[0].Int64Field, Is.EqualTo(33));
      Assert.That(result331[0].FloatField, Is.EqualTo((float)33));
      Assert.That(result331[0].DoubleField, Is.EqualTo((double)33));
      Assert.That(result331[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result331[0].StringField, Is.EqualTo("TestEntityTwo33"));
      Assert.That(result331[0].Text, Is.EqualTo("This is an instance of TestEntityTwo33"));
      var result340 = session.Query.All<PartOne.TestEntityOne34>().ToArray();
      Assert.That(result340.Length, Is.EqualTo(1));
      Assert.That(result340[0].BooleanField, Is.True);
      Assert.That(result340[0].Int16Field, Is.EqualTo(34));
      Assert.That(result340[0].Int32Field, Is.EqualTo(34));
      Assert.That(result340[0].Int64Field, Is.EqualTo(34));
      Assert.That(result340[0].FloatField, Is.EqualTo((float)34));
      Assert.That(result340[0].DoubleField, Is.EqualTo((double)34));
      Assert.That(result340[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result340[0].StringField, Is.EqualTo("TestEntityOne34"));
      Assert.That(result340[0].Text, Is.EqualTo("This is an instance of TestEntityOne34"));

      var result341 = session.Query.All<PartTwo.TestEntityTwo34>().ToArray();
      Assert.That(result341.Length, Is.EqualTo(1));
      Assert.That(result341[0].BooleanField, Is.True);
      Assert.That(result341[0].Int16Field, Is.EqualTo(34));
      Assert.That(result341[0].Int32Field, Is.EqualTo(34));
      Assert.That(result341[0].Int64Field, Is.EqualTo(34));
      Assert.That(result341[0].FloatField, Is.EqualTo((float)34));
      Assert.That(result341[0].DoubleField, Is.EqualTo((double)34));
      Assert.That(result341[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result341[0].StringField, Is.EqualTo("TestEntityTwo34"));
      Assert.That(result341[0].Text, Is.EqualTo("This is an instance of TestEntityTwo34"));
      var result350 = session.Query.All<PartOne.TestEntityOne35>().ToArray();
      Assert.That(result350.Length, Is.EqualTo(1));
      Assert.That(result350[0].BooleanField, Is.True);
      Assert.That(result350[0].Int16Field, Is.EqualTo(35));
      Assert.That(result350[0].Int32Field, Is.EqualTo(35));
      Assert.That(result350[0].Int64Field, Is.EqualTo(35));
      Assert.That(result350[0].FloatField, Is.EqualTo((float)35));
      Assert.That(result350[0].DoubleField, Is.EqualTo((double)35));
      Assert.That(result350[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result350[0].StringField, Is.EqualTo("TestEntityOne35"));
      Assert.That(result350[0].Text, Is.EqualTo("This is an instance of TestEntityOne35"));

      var result351 = session.Query.All<PartTwo.TestEntityTwo35>().ToArray();
      Assert.That(result351.Length, Is.EqualTo(1));
      Assert.That(result351[0].BooleanField, Is.True);
      Assert.That(result351[0].Int16Field, Is.EqualTo(35));
      Assert.That(result351[0].Int32Field, Is.EqualTo(35));
      Assert.That(result351[0].Int64Field, Is.EqualTo(35));
      Assert.That(result351[0].FloatField, Is.EqualTo((float)35));
      Assert.That(result351[0].DoubleField, Is.EqualTo((double)35));
      Assert.That(result351[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result351[0].StringField, Is.EqualTo("TestEntityTwo35"));
      Assert.That(result351[0].Text, Is.EqualTo("This is an instance of TestEntityTwo35"));
      var result360 = session.Query.All<PartOne.TestEntityOne36>().ToArray();
      Assert.That(result360.Length, Is.EqualTo(1));
      Assert.That(result360[0].BooleanField, Is.True);
      Assert.That(result360[0].Int16Field, Is.EqualTo(36));
      Assert.That(result360[0].Int32Field, Is.EqualTo(36));
      Assert.That(result360[0].Int64Field, Is.EqualTo(36));
      Assert.That(result360[0].FloatField, Is.EqualTo((float)36));
      Assert.That(result360[0].DoubleField, Is.EqualTo((double)36));
      Assert.That(result360[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result360[0].StringField, Is.EqualTo("TestEntityOne36"));
      Assert.That(result360[0].Text, Is.EqualTo("This is an instance of TestEntityOne36"));

      var result361 = session.Query.All<PartTwo.TestEntityTwo36>().ToArray();
      Assert.That(result361.Length, Is.EqualTo(1));
      Assert.That(result361[0].BooleanField, Is.True);
      Assert.That(result361[0].Int16Field, Is.EqualTo(36));
      Assert.That(result361[0].Int32Field, Is.EqualTo(36));
      Assert.That(result361[0].Int64Field, Is.EqualTo(36));
      Assert.That(result361[0].FloatField, Is.EqualTo((float)36));
      Assert.That(result361[0].DoubleField, Is.EqualTo((double)36));
      Assert.That(result361[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result361[0].StringField, Is.EqualTo("TestEntityTwo36"));
      Assert.That(result361[0].Text, Is.EqualTo("This is an instance of TestEntityTwo36"));
      var result370 = session.Query.All<PartOne.TestEntityOne37>().ToArray();
      Assert.That(result370.Length, Is.EqualTo(1));
      Assert.That(result370[0].BooleanField, Is.True);
      Assert.That(result370[0].Int16Field, Is.EqualTo(37));
      Assert.That(result370[0].Int32Field, Is.EqualTo(37));
      Assert.That(result370[0].Int64Field, Is.EqualTo(37));
      Assert.That(result370[0].FloatField, Is.EqualTo((float)37));
      Assert.That(result370[0].DoubleField, Is.EqualTo((double)37));
      Assert.That(result370[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result370[0].StringField, Is.EqualTo("TestEntityOne37"));
      Assert.That(result370[0].Text, Is.EqualTo("This is an instance of TestEntityOne37"));

      var result371 = session.Query.All<PartTwo.TestEntityTwo37>().ToArray();
      Assert.That(result371.Length, Is.EqualTo(1));
      Assert.That(result371[0].BooleanField, Is.True);
      Assert.That(result371[0].Int16Field, Is.EqualTo(37));
      Assert.That(result371[0].Int32Field, Is.EqualTo(37));
      Assert.That(result371[0].Int64Field, Is.EqualTo(37));
      Assert.That(result371[0].FloatField, Is.EqualTo((float)37));
      Assert.That(result371[0].DoubleField, Is.EqualTo((double)37));
      Assert.That(result371[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result371[0].StringField, Is.EqualTo("TestEntityTwo37"));
      Assert.That(result371[0].Text, Is.EqualTo("This is an instance of TestEntityTwo37"));
      var result380 = session.Query.All<PartOne.TestEntityOne38>().ToArray();
      Assert.That(result380.Length, Is.EqualTo(1));
      Assert.That(result380[0].BooleanField, Is.True);
      Assert.That(result380[0].Int16Field, Is.EqualTo(38));
      Assert.That(result380[0].Int32Field, Is.EqualTo(38));
      Assert.That(result380[0].Int64Field, Is.EqualTo(38));
      Assert.That(result380[0].FloatField, Is.EqualTo((float)38));
      Assert.That(result380[0].DoubleField, Is.EqualTo((double)38));
      Assert.That(result380[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result380[0].StringField, Is.EqualTo("TestEntityOne38"));
      Assert.That(result380[0].Text, Is.EqualTo("This is an instance of TestEntityOne38"));

      var result381 = session.Query.All<PartTwo.TestEntityTwo38>().ToArray();
      Assert.That(result381.Length, Is.EqualTo(1));
      Assert.That(result381[0].BooleanField, Is.True);
      Assert.That(result381[0].Int16Field, Is.EqualTo(38));
      Assert.That(result381[0].Int32Field, Is.EqualTo(38));
      Assert.That(result381[0].Int64Field, Is.EqualTo(38));
      Assert.That(result381[0].FloatField, Is.EqualTo((float)38));
      Assert.That(result381[0].DoubleField, Is.EqualTo((double)38));
      Assert.That(result381[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result381[0].StringField, Is.EqualTo("TestEntityTwo38"));
      Assert.That(result381[0].Text, Is.EqualTo("This is an instance of TestEntityTwo38"));
      var result390 = session.Query.All<PartOne.TestEntityOne39>().ToArray();
      Assert.That(result390.Length, Is.EqualTo(1));
      Assert.That(result390[0].BooleanField, Is.True);
      Assert.That(result390[0].Int16Field, Is.EqualTo(39));
      Assert.That(result390[0].Int32Field, Is.EqualTo(39));
      Assert.That(result390[0].Int64Field, Is.EqualTo(39));
      Assert.That(result390[0].FloatField, Is.EqualTo((float)39));
      Assert.That(result390[0].DoubleField, Is.EqualTo((double)39));
      Assert.That(result390[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result390[0].StringField, Is.EqualTo("TestEntityOne39"));
      Assert.That(result390[0].Text, Is.EqualTo("This is an instance of TestEntityOne39"));

      var result391 = session.Query.All<PartTwo.TestEntityTwo39>().ToArray();
      Assert.That(result391.Length, Is.EqualTo(1));
      Assert.That(result391[0].BooleanField, Is.True);
      Assert.That(result391[0].Int16Field, Is.EqualTo(39));
      Assert.That(result391[0].Int32Field, Is.EqualTo(39));
      Assert.That(result391[0].Int64Field, Is.EqualTo(39));
      Assert.That(result391[0].FloatField, Is.EqualTo((float)39));
      Assert.That(result391[0].DoubleField, Is.EqualTo((double)39));
      Assert.That(result391[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result391[0].StringField, Is.EqualTo("TestEntityTwo39"));
      Assert.That(result391[0].Text, Is.EqualTo("This is an instance of TestEntityTwo39"));
      var result400 = session.Query.All<PartOne.TestEntityOne40>().ToArray();
      Assert.That(result400.Length, Is.EqualTo(1));
      Assert.That(result400[0].BooleanField, Is.True);
      Assert.That(result400[0].Int16Field, Is.EqualTo(40));
      Assert.That(result400[0].Int32Field, Is.EqualTo(40));
      Assert.That(result400[0].Int64Field, Is.EqualTo(40));
      Assert.That(result400[0].FloatField, Is.EqualTo((float)40));
      Assert.That(result400[0].DoubleField, Is.EqualTo((double)40));
      Assert.That(result400[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result400[0].StringField, Is.EqualTo("TestEntityOne40"));
      Assert.That(result400[0].Text, Is.EqualTo("This is an instance of TestEntityOne40"));

      var result401 = session.Query.All<PartTwo.TestEntityTwo40>().ToArray();
      Assert.That(result401.Length, Is.EqualTo(1));
      Assert.That(result401[0].BooleanField, Is.True);
      Assert.That(result401[0].Int16Field, Is.EqualTo(40));
      Assert.That(result401[0].Int32Field, Is.EqualTo(40));
      Assert.That(result401[0].Int64Field, Is.EqualTo(40));
      Assert.That(result401[0].FloatField, Is.EqualTo((float)40));
      Assert.That(result401[0].DoubleField, Is.EqualTo((double)40));
      Assert.That(result401[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result401[0].StringField, Is.EqualTo("TestEntityTwo40"));
      Assert.That(result401[0].Text, Is.EqualTo("This is an instance of TestEntityTwo40"));
      var result410 = session.Query.All<PartOne.TestEntityOne41>().ToArray();
      Assert.That(result410.Length, Is.EqualTo(1));
      Assert.That(result410[0].BooleanField, Is.True);
      Assert.That(result410[0].Int16Field, Is.EqualTo(41));
      Assert.That(result410[0].Int32Field, Is.EqualTo(41));
      Assert.That(result410[0].Int64Field, Is.EqualTo(41));
      Assert.That(result410[0].FloatField, Is.EqualTo((float)41));
      Assert.That(result410[0].DoubleField, Is.EqualTo((double)41));
      Assert.That(result410[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result410[0].StringField, Is.EqualTo("TestEntityOne41"));
      Assert.That(result410[0].Text, Is.EqualTo("This is an instance of TestEntityOne41"));

      var result411 = session.Query.All<PartTwo.TestEntityTwo41>().ToArray();
      Assert.That(result411.Length, Is.EqualTo(1));
      Assert.That(result411[0].BooleanField, Is.True);
      Assert.That(result411[0].Int16Field, Is.EqualTo(41));
      Assert.That(result411[0].Int32Field, Is.EqualTo(41));
      Assert.That(result411[0].Int64Field, Is.EqualTo(41));
      Assert.That(result411[0].FloatField, Is.EqualTo((float)41));
      Assert.That(result411[0].DoubleField, Is.EqualTo((double)41));
      Assert.That(result411[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result411[0].StringField, Is.EqualTo("TestEntityTwo41"));
      Assert.That(result411[0].Text, Is.EqualTo("This is an instance of TestEntityTwo41"));
      var result420 = session.Query.All<PartOne.TestEntityOne42>().ToArray();
      Assert.That(result420.Length, Is.EqualTo(1));
      Assert.That(result420[0].BooleanField, Is.True);
      Assert.That(result420[0].Int16Field, Is.EqualTo(42));
      Assert.That(result420[0].Int32Field, Is.EqualTo(42));
      Assert.That(result420[0].Int64Field, Is.EqualTo(42));
      Assert.That(result420[0].FloatField, Is.EqualTo((float)42));
      Assert.That(result420[0].DoubleField, Is.EqualTo((double)42));
      Assert.That(result420[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result420[0].StringField, Is.EqualTo("TestEntityOne42"));
      Assert.That(result420[0].Text, Is.EqualTo("This is an instance of TestEntityOne42"));

      var result421 = session.Query.All<PartTwo.TestEntityTwo42>().ToArray();
      Assert.That(result421.Length, Is.EqualTo(1));
      Assert.That(result421[0].BooleanField, Is.True);
      Assert.That(result421[0].Int16Field, Is.EqualTo(42));
      Assert.That(result421[0].Int32Field, Is.EqualTo(42));
      Assert.That(result421[0].Int64Field, Is.EqualTo(42));
      Assert.That(result421[0].FloatField, Is.EqualTo((float)42));
      Assert.That(result421[0].DoubleField, Is.EqualTo((double)42));
      Assert.That(result421[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result421[0].StringField, Is.EqualTo("TestEntityTwo42"));
      Assert.That(result421[0].Text, Is.EqualTo("This is an instance of TestEntityTwo42"));
      var result430 = session.Query.All<PartOne.TestEntityOne43>().ToArray();
      Assert.That(result430.Length, Is.EqualTo(1));
      Assert.That(result430[0].BooleanField, Is.True);
      Assert.That(result430[0].Int16Field, Is.EqualTo(43));
      Assert.That(result430[0].Int32Field, Is.EqualTo(43));
      Assert.That(result430[0].Int64Field, Is.EqualTo(43));
      Assert.That(result430[0].FloatField, Is.EqualTo((float)43));
      Assert.That(result430[0].DoubleField, Is.EqualTo((double)43));
      Assert.That(result430[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result430[0].StringField, Is.EqualTo("TestEntityOne43"));
      Assert.That(result430[0].Text, Is.EqualTo("This is an instance of TestEntityOne43"));

      var result431 = session.Query.All<PartTwo.TestEntityTwo43>().ToArray();
      Assert.That(result431.Length, Is.EqualTo(1));
      Assert.That(result431[0].BooleanField, Is.True);
      Assert.That(result431[0].Int16Field, Is.EqualTo(43));
      Assert.That(result431[0].Int32Field, Is.EqualTo(43));
      Assert.That(result431[0].Int64Field, Is.EqualTo(43));
      Assert.That(result431[0].FloatField, Is.EqualTo((float)43));
      Assert.That(result431[0].DoubleField, Is.EqualTo((double)43));
      Assert.That(result431[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result431[0].StringField, Is.EqualTo("TestEntityTwo43"));
      Assert.That(result431[0].Text, Is.EqualTo("This is an instance of TestEntityTwo43"));
      var result440 = session.Query.All<PartOne.TestEntityOne44>().ToArray();
      Assert.That(result440.Length, Is.EqualTo(1));
      Assert.That(result440[0].BooleanField, Is.True);
      Assert.That(result440[0].Int16Field, Is.EqualTo(44));
      Assert.That(result440[0].Int32Field, Is.EqualTo(44));
      Assert.That(result440[0].Int64Field, Is.EqualTo(44));
      Assert.That(result440[0].FloatField, Is.EqualTo((float)44));
      Assert.That(result440[0].DoubleField, Is.EqualTo((double)44));
      Assert.That(result440[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result440[0].StringField, Is.EqualTo("TestEntityOne44"));
      Assert.That(result440[0].Text, Is.EqualTo("This is an instance of TestEntityOne44"));

      var result441 = session.Query.All<PartTwo.TestEntityTwo44>().ToArray();
      Assert.That(result441.Length, Is.EqualTo(1));
      Assert.That(result441[0].BooleanField, Is.True);
      Assert.That(result441[0].Int16Field, Is.EqualTo(44));
      Assert.That(result441[0].Int32Field, Is.EqualTo(44));
      Assert.That(result441[0].Int64Field, Is.EqualTo(44));
      Assert.That(result441[0].FloatField, Is.EqualTo((float)44));
      Assert.That(result441[0].DoubleField, Is.EqualTo((double)44));
      Assert.That(result441[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result441[0].StringField, Is.EqualTo("TestEntityTwo44"));
      Assert.That(result441[0].Text, Is.EqualTo("This is an instance of TestEntityTwo44"));
      var result450 = session.Query.All<PartOne.TestEntityOne45>().ToArray();
      Assert.That(result450.Length, Is.EqualTo(1));
      Assert.That(result450[0].BooleanField, Is.True);
      Assert.That(result450[0].Int16Field, Is.EqualTo(45));
      Assert.That(result450[0].Int32Field, Is.EqualTo(45));
      Assert.That(result450[0].Int64Field, Is.EqualTo(45));
      Assert.That(result450[0].FloatField, Is.EqualTo((float)45));
      Assert.That(result450[0].DoubleField, Is.EqualTo((double)45));
      Assert.That(result450[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result450[0].StringField, Is.EqualTo("TestEntityOne45"));
      Assert.That(result450[0].Text, Is.EqualTo("This is an instance of TestEntityOne45"));

      var result451 = session.Query.All<PartTwo.TestEntityTwo45>().ToArray();
      Assert.That(result451.Length, Is.EqualTo(1));
      Assert.That(result451[0].BooleanField, Is.True);
      Assert.That(result451[0].Int16Field, Is.EqualTo(45));
      Assert.That(result451[0].Int32Field, Is.EqualTo(45));
      Assert.That(result451[0].Int64Field, Is.EqualTo(45));
      Assert.That(result451[0].FloatField, Is.EqualTo((float)45));
      Assert.That(result451[0].DoubleField, Is.EqualTo((double)45));
      Assert.That(result451[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result451[0].StringField, Is.EqualTo("TestEntityTwo45"));
      Assert.That(result451[0].Text, Is.EqualTo("This is an instance of TestEntityTwo45"));
      var result460 = session.Query.All<PartOne.TestEntityOne46>().ToArray();
      Assert.That(result460.Length, Is.EqualTo(1));
      Assert.That(result460[0].BooleanField, Is.True);
      Assert.That(result460[0].Int16Field, Is.EqualTo(46));
      Assert.That(result460[0].Int32Field, Is.EqualTo(46));
      Assert.That(result460[0].Int64Field, Is.EqualTo(46));
      Assert.That(result460[0].FloatField, Is.EqualTo((float)46));
      Assert.That(result460[0].DoubleField, Is.EqualTo((double)46));
      Assert.That(result460[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result460[0].StringField, Is.EqualTo("TestEntityOne46"));
      Assert.That(result460[0].Text, Is.EqualTo("This is an instance of TestEntityOne46"));

      var result461 = session.Query.All<PartTwo.TestEntityTwo46>().ToArray();
      Assert.That(result461.Length, Is.EqualTo(1));
      Assert.That(result461[0].BooleanField, Is.True);
      Assert.That(result461[0].Int16Field, Is.EqualTo(46));
      Assert.That(result461[0].Int32Field, Is.EqualTo(46));
      Assert.That(result461[0].Int64Field, Is.EqualTo(46));
      Assert.That(result461[0].FloatField, Is.EqualTo((float)46));
      Assert.That(result461[0].DoubleField, Is.EqualTo((double)46));
      Assert.That(result461[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result461[0].StringField, Is.EqualTo("TestEntityTwo46"));
      Assert.That(result461[0].Text, Is.EqualTo("This is an instance of TestEntityTwo46"));
      var result470 = session.Query.All<PartOne.TestEntityOne47>().ToArray();
      Assert.That(result470.Length, Is.EqualTo(1));
      Assert.That(result470[0].BooleanField, Is.True);
      Assert.That(result470[0].Int16Field, Is.EqualTo(47));
      Assert.That(result470[0].Int32Field, Is.EqualTo(47));
      Assert.That(result470[0].Int64Field, Is.EqualTo(47));
      Assert.That(result470[0].FloatField, Is.EqualTo((float)47));
      Assert.That(result470[0].DoubleField, Is.EqualTo((double)47));
      Assert.That(result470[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result470[0].StringField, Is.EqualTo("TestEntityOne47"));
      Assert.That(result470[0].Text, Is.EqualTo("This is an instance of TestEntityOne47"));

      var result471 = session.Query.All<PartTwo.TestEntityTwo47>().ToArray();
      Assert.That(result471.Length, Is.EqualTo(1));
      Assert.That(result471[0].BooleanField, Is.True);
      Assert.That(result471[0].Int16Field, Is.EqualTo(47));
      Assert.That(result471[0].Int32Field, Is.EqualTo(47));
      Assert.That(result471[0].Int64Field, Is.EqualTo(47));
      Assert.That(result471[0].FloatField, Is.EqualTo((float)47));
      Assert.That(result471[0].DoubleField, Is.EqualTo((double)47));
      Assert.That(result471[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result471[0].StringField, Is.EqualTo("TestEntityTwo47"));
      Assert.That(result471[0].Text, Is.EqualTo("This is an instance of TestEntityTwo47"));
      var result480 = session.Query.All<PartOne.TestEntityOne48>().ToArray();
      Assert.That(result480.Length, Is.EqualTo(1));
      Assert.That(result480[0].BooleanField, Is.True);
      Assert.That(result480[0].Int16Field, Is.EqualTo(48));
      Assert.That(result480[0].Int32Field, Is.EqualTo(48));
      Assert.That(result480[0].Int64Field, Is.EqualTo(48));
      Assert.That(result480[0].FloatField, Is.EqualTo((float)48));
      Assert.That(result480[0].DoubleField, Is.EqualTo((double)48));
      Assert.That(result480[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result480[0].StringField, Is.EqualTo("TestEntityOne48"));
      Assert.That(result480[0].Text, Is.EqualTo("This is an instance of TestEntityOne48"));

      var result481 = session.Query.All<PartTwo.TestEntityTwo48>().ToArray();
      Assert.That(result481.Length, Is.EqualTo(1));
      Assert.That(result481[0].BooleanField, Is.True);
      Assert.That(result481[0].Int16Field, Is.EqualTo(48));
      Assert.That(result481[0].Int32Field, Is.EqualTo(48));
      Assert.That(result481[0].Int64Field, Is.EqualTo(48));
      Assert.That(result481[0].FloatField, Is.EqualTo((float)48));
      Assert.That(result481[0].DoubleField, Is.EqualTo((double)48));
      Assert.That(result481[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result481[0].StringField, Is.EqualTo("TestEntityTwo48"));
      Assert.That(result481[0].Text, Is.EqualTo("This is an instance of TestEntityTwo48"));
      var result490 = session.Query.All<PartOne.TestEntityOne49>().ToArray();
      Assert.That(result490.Length, Is.EqualTo(1));
      Assert.That(result490[0].BooleanField, Is.True);
      Assert.That(result490[0].Int16Field, Is.EqualTo(49));
      Assert.That(result490[0].Int32Field, Is.EqualTo(49));
      Assert.That(result490[0].Int64Field, Is.EqualTo(49));
      Assert.That(result490[0].FloatField, Is.EqualTo((float)49));
      Assert.That(result490[0].DoubleField, Is.EqualTo((double)49));
      Assert.That(result490[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result490[0].StringField, Is.EqualTo("TestEntityOne49"));
      Assert.That(result490[0].Text, Is.EqualTo("This is an instance of TestEntityOne49"));

      var result491 = session.Query.All<PartTwo.TestEntityTwo49>().ToArray();
      Assert.That(result491.Length, Is.EqualTo(1));
      Assert.That(result491[0].BooleanField, Is.True);
      Assert.That(result491[0].Int16Field, Is.EqualTo(49));
      Assert.That(result491[0].Int32Field, Is.EqualTo(49));
      Assert.That(result491[0].Int64Field, Is.EqualTo(49));
      Assert.That(result491[0].FloatField, Is.EqualTo((float)49));
      Assert.That(result491[0].DoubleField, Is.EqualTo((double)49));
      Assert.That(result491[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result491[0].StringField, Is.EqualTo("TestEntityTwo49"));
      Assert.That(result491[0].Text, Is.EqualTo("This is an instance of TestEntityTwo49"));
      var result500 = session.Query.All<PartOne.TestEntityOne50>().ToArray();
      Assert.That(result500.Length, Is.EqualTo(1));
      Assert.That(result500[0].BooleanField, Is.True);
      Assert.That(result500[0].Int16Field, Is.EqualTo(50));
      Assert.That(result500[0].Int32Field, Is.EqualTo(50));
      Assert.That(result500[0].Int64Field, Is.EqualTo(50));
      Assert.That(result500[0].FloatField, Is.EqualTo((float)50));
      Assert.That(result500[0].DoubleField, Is.EqualTo((double)50));
      Assert.That(result500[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result500[0].StringField, Is.EqualTo("TestEntityOne50"));
      Assert.That(result500[0].Text, Is.EqualTo("This is an instance of TestEntityOne50"));

      var result501 = session.Query.All<PartTwo.TestEntityTwo50>().ToArray();
      Assert.That(result501.Length, Is.EqualTo(1));
      Assert.That(result501[0].BooleanField, Is.True);
      Assert.That(result501[0].Int16Field, Is.EqualTo(50));
      Assert.That(result501[0].Int32Field, Is.EqualTo(50));
      Assert.That(result501[0].Int64Field, Is.EqualTo(50));
      Assert.That(result501[0].FloatField, Is.EqualTo((float)50));
      Assert.That(result501[0].DoubleField, Is.EqualTo((double)50));
      Assert.That(result501[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result501[0].StringField, Is.EqualTo("TestEntityTwo50"));
      Assert.That(result501[0].Text, Is.EqualTo("This is an instance of TestEntityTwo50"));
      var result510 = session.Query.All<PartOne.TestEntityOne51>().ToArray();
      Assert.That(result510.Length, Is.EqualTo(1));
      Assert.That(result510[0].BooleanField, Is.True);
      Assert.That(result510[0].Int16Field, Is.EqualTo(51));
      Assert.That(result510[0].Int32Field, Is.EqualTo(51));
      Assert.That(result510[0].Int64Field, Is.EqualTo(51));
      Assert.That(result510[0].FloatField, Is.EqualTo((float)51));
      Assert.That(result510[0].DoubleField, Is.EqualTo((double)51));
      Assert.That(result510[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result510[0].StringField, Is.EqualTo("TestEntityOne51"));
      Assert.That(result510[0].Text, Is.EqualTo("This is an instance of TestEntityOne51"));

      var result511 = session.Query.All<PartTwo.TestEntityTwo51>().ToArray();
      Assert.That(result511.Length, Is.EqualTo(1));
      Assert.That(result511[0].BooleanField, Is.True);
      Assert.That(result511[0].Int16Field, Is.EqualTo(51));
      Assert.That(result511[0].Int32Field, Is.EqualTo(51));
      Assert.That(result511[0].Int64Field, Is.EqualTo(51));
      Assert.That(result511[0].FloatField, Is.EqualTo((float)51));
      Assert.That(result511[0].DoubleField, Is.EqualTo((double)51));
      Assert.That(result511[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result511[0].StringField, Is.EqualTo("TestEntityTwo51"));
      Assert.That(result511[0].Text, Is.EqualTo("This is an instance of TestEntityTwo51"));
      var result520 = session.Query.All<PartOne.TestEntityOne52>().ToArray();
      Assert.That(result520.Length, Is.EqualTo(1));
      Assert.That(result520[0].BooleanField, Is.True);
      Assert.That(result520[0].Int16Field, Is.EqualTo(52));
      Assert.That(result520[0].Int32Field, Is.EqualTo(52));
      Assert.That(result520[0].Int64Field, Is.EqualTo(52));
      Assert.That(result520[0].FloatField, Is.EqualTo((float)52));
      Assert.That(result520[0].DoubleField, Is.EqualTo((double)52));
      Assert.That(result520[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result520[0].StringField, Is.EqualTo("TestEntityOne52"));
      Assert.That(result520[0].Text, Is.EqualTo("This is an instance of TestEntityOne52"));

      var result521 = session.Query.All<PartTwo.TestEntityTwo52>().ToArray();
      Assert.That(result521.Length, Is.EqualTo(1));
      Assert.That(result521[0].BooleanField, Is.True);
      Assert.That(result521[0].Int16Field, Is.EqualTo(52));
      Assert.That(result521[0].Int32Field, Is.EqualTo(52));
      Assert.That(result521[0].Int64Field, Is.EqualTo(52));
      Assert.That(result521[0].FloatField, Is.EqualTo((float)52));
      Assert.That(result521[0].DoubleField, Is.EqualTo((double)52));
      Assert.That(result521[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result521[0].StringField, Is.EqualTo("TestEntityTwo52"));
      Assert.That(result521[0].Text, Is.EqualTo("This is an instance of TestEntityTwo52"));
      var result530 = session.Query.All<PartOne.TestEntityOne53>().ToArray();
      Assert.That(result530.Length, Is.EqualTo(1));
      Assert.That(result530[0].BooleanField, Is.True);
      Assert.That(result530[0].Int16Field, Is.EqualTo(53));
      Assert.That(result530[0].Int32Field, Is.EqualTo(53));
      Assert.That(result530[0].Int64Field, Is.EqualTo(53));
      Assert.That(result530[0].FloatField, Is.EqualTo((float)53));
      Assert.That(result530[0].DoubleField, Is.EqualTo((double)53));
      Assert.That(result530[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result530[0].StringField, Is.EqualTo("TestEntityOne53"));
      Assert.That(result530[0].Text, Is.EqualTo("This is an instance of TestEntityOne53"));

      var result531 = session.Query.All<PartTwo.TestEntityTwo53>().ToArray();
      Assert.That(result531.Length, Is.EqualTo(1));
      Assert.That(result531[0].BooleanField, Is.True);
      Assert.That(result531[0].Int16Field, Is.EqualTo(53));
      Assert.That(result531[0].Int32Field, Is.EqualTo(53));
      Assert.That(result531[0].Int64Field, Is.EqualTo(53));
      Assert.That(result531[0].FloatField, Is.EqualTo((float)53));
      Assert.That(result531[0].DoubleField, Is.EqualTo((double)53));
      Assert.That(result531[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result531[0].StringField, Is.EqualTo("TestEntityTwo53"));
      Assert.That(result531[0].Text, Is.EqualTo("This is an instance of TestEntityTwo53"));
      var result540 = session.Query.All<PartOne.TestEntityOne54>().ToArray();
      Assert.That(result540.Length, Is.EqualTo(1));
      Assert.That(result540[0].BooleanField, Is.True);
      Assert.That(result540[0].Int16Field, Is.EqualTo(54));
      Assert.That(result540[0].Int32Field, Is.EqualTo(54));
      Assert.That(result540[0].Int64Field, Is.EqualTo(54));
      Assert.That(result540[0].FloatField, Is.EqualTo((float)54));
      Assert.That(result540[0].DoubleField, Is.EqualTo((double)54));
      Assert.That(result540[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result540[0].StringField, Is.EqualTo("TestEntityOne54"));
      Assert.That(result540[0].Text, Is.EqualTo("This is an instance of TestEntityOne54"));

      var result541 = session.Query.All<PartTwo.TestEntityTwo54>().ToArray();
      Assert.That(result541.Length, Is.EqualTo(1));
      Assert.That(result541[0].BooleanField, Is.True);
      Assert.That(result541[0].Int16Field, Is.EqualTo(54));
      Assert.That(result541[0].Int32Field, Is.EqualTo(54));
      Assert.That(result541[0].Int64Field, Is.EqualTo(54));
      Assert.That(result541[0].FloatField, Is.EqualTo((float)54));
      Assert.That(result541[0].DoubleField, Is.EqualTo((double)54));
      Assert.That(result541[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result541[0].StringField, Is.EqualTo("TestEntityTwo54"));
      Assert.That(result541[0].Text, Is.EqualTo("This is an instance of TestEntityTwo54"));
      var result550 = session.Query.All<PartOne.TestEntityOne55>().ToArray();
      Assert.That(result550.Length, Is.EqualTo(1));
      Assert.That(result550[0].BooleanField, Is.True);
      Assert.That(result550[0].Int16Field, Is.EqualTo(55));
      Assert.That(result550[0].Int32Field, Is.EqualTo(55));
      Assert.That(result550[0].Int64Field, Is.EqualTo(55));
      Assert.That(result550[0].FloatField, Is.EqualTo((float)55));
      Assert.That(result550[0].DoubleField, Is.EqualTo((double)55));
      Assert.That(result550[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result550[0].StringField, Is.EqualTo("TestEntityOne55"));
      Assert.That(result550[0].Text, Is.EqualTo("This is an instance of TestEntityOne55"));

      var result551 = session.Query.All<PartTwo.TestEntityTwo55>().ToArray();
      Assert.That(result551.Length, Is.EqualTo(1));
      Assert.That(result551[0].BooleanField, Is.True);
      Assert.That(result551[0].Int16Field, Is.EqualTo(55));
      Assert.That(result551[0].Int32Field, Is.EqualTo(55));
      Assert.That(result551[0].Int64Field, Is.EqualTo(55));
      Assert.That(result551[0].FloatField, Is.EqualTo((float)55));
      Assert.That(result551[0].DoubleField, Is.EqualTo((double)55));
      Assert.That(result551[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result551[0].StringField, Is.EqualTo("TestEntityTwo55"));
      Assert.That(result551[0].Text, Is.EqualTo("This is an instance of TestEntityTwo55"));
      var result560 = session.Query.All<PartOne.TestEntityOne56>().ToArray();
      Assert.That(result560.Length, Is.EqualTo(1));
      Assert.That(result560[0].BooleanField, Is.True);
      Assert.That(result560[0].Int16Field, Is.EqualTo(56));
      Assert.That(result560[0].Int32Field, Is.EqualTo(56));
      Assert.That(result560[0].Int64Field, Is.EqualTo(56));
      Assert.That(result560[0].FloatField, Is.EqualTo((float)56));
      Assert.That(result560[0].DoubleField, Is.EqualTo((double)56));
      Assert.That(result560[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result560[0].StringField, Is.EqualTo("TestEntityOne56"));
      Assert.That(result560[0].Text, Is.EqualTo("This is an instance of TestEntityOne56"));

      var result561 = session.Query.All<PartTwo.TestEntityTwo56>().ToArray();
      Assert.That(result561.Length, Is.EqualTo(1));
      Assert.That(result561[0].BooleanField, Is.True);
      Assert.That(result561[0].Int16Field, Is.EqualTo(56));
      Assert.That(result561[0].Int32Field, Is.EqualTo(56));
      Assert.That(result561[0].Int64Field, Is.EqualTo(56));
      Assert.That(result561[0].FloatField, Is.EqualTo((float)56));
      Assert.That(result561[0].DoubleField, Is.EqualTo((double)56));
      Assert.That(result561[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result561[0].StringField, Is.EqualTo("TestEntityTwo56"));
      Assert.That(result561[0].Text, Is.EqualTo("This is an instance of TestEntityTwo56"));
      var result570 = session.Query.All<PartOne.TestEntityOne57>().ToArray();
      Assert.That(result570.Length, Is.EqualTo(1));
      Assert.That(result570[0].BooleanField, Is.True);
      Assert.That(result570[0].Int16Field, Is.EqualTo(57));
      Assert.That(result570[0].Int32Field, Is.EqualTo(57));
      Assert.That(result570[0].Int64Field, Is.EqualTo(57));
      Assert.That(result570[0].FloatField, Is.EqualTo((float)57));
      Assert.That(result570[0].DoubleField, Is.EqualTo((double)57));
      Assert.That(result570[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result570[0].StringField, Is.EqualTo("TestEntityOne57"));
      Assert.That(result570[0].Text, Is.EqualTo("This is an instance of TestEntityOne57"));

      var result571 = session.Query.All<PartTwo.TestEntityTwo57>().ToArray();
      Assert.That(result571.Length, Is.EqualTo(1));
      Assert.That(result571[0].BooleanField, Is.True);
      Assert.That(result571[0].Int16Field, Is.EqualTo(57));
      Assert.That(result571[0].Int32Field, Is.EqualTo(57));
      Assert.That(result571[0].Int64Field, Is.EqualTo(57));
      Assert.That(result571[0].FloatField, Is.EqualTo((float)57));
      Assert.That(result571[0].DoubleField, Is.EqualTo((double)57));
      Assert.That(result571[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result571[0].StringField, Is.EqualTo("TestEntityTwo57"));
      Assert.That(result571[0].Text, Is.EqualTo("This is an instance of TestEntityTwo57"));
      var result580 = session.Query.All<PartOne.TestEntityOne58>().ToArray();
      Assert.That(result580.Length, Is.EqualTo(1));
      Assert.That(result580[0].BooleanField, Is.True);
      Assert.That(result580[0].Int16Field, Is.EqualTo(58));
      Assert.That(result580[0].Int32Field, Is.EqualTo(58));
      Assert.That(result580[0].Int64Field, Is.EqualTo(58));
      Assert.That(result580[0].FloatField, Is.EqualTo((float)58));
      Assert.That(result580[0].DoubleField, Is.EqualTo((double)58));
      Assert.That(result580[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result580[0].StringField, Is.EqualTo("TestEntityOne58"));
      Assert.That(result580[0].Text, Is.EqualTo("This is an instance of TestEntityOne58"));

      var result581 = session.Query.All<PartTwo.TestEntityTwo58>().ToArray();
      Assert.That(result581.Length, Is.EqualTo(1));
      Assert.That(result581[0].BooleanField, Is.True);
      Assert.That(result581[0].Int16Field, Is.EqualTo(58));
      Assert.That(result581[0].Int32Field, Is.EqualTo(58));
      Assert.That(result581[0].Int64Field, Is.EqualTo(58));
      Assert.That(result581[0].FloatField, Is.EqualTo((float)58));
      Assert.That(result581[0].DoubleField, Is.EqualTo((double)58));
      Assert.That(result581[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result581[0].StringField, Is.EqualTo("TestEntityTwo58"));
      Assert.That(result581[0].Text, Is.EqualTo("This is an instance of TestEntityTwo58"));
      var result590 = session.Query.All<PartOne.TestEntityOne59>().ToArray();
      Assert.That(result590.Length, Is.EqualTo(1));
      Assert.That(result590[0].BooleanField, Is.True);
      Assert.That(result590[0].Int16Field, Is.EqualTo(59));
      Assert.That(result590[0].Int32Field, Is.EqualTo(59));
      Assert.That(result590[0].Int64Field, Is.EqualTo(59));
      Assert.That(result590[0].FloatField, Is.EqualTo((float)59));
      Assert.That(result590[0].DoubleField, Is.EqualTo((double)59));
      Assert.That(result590[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result590[0].StringField, Is.EqualTo("TestEntityOne59"));
      Assert.That(result590[0].Text, Is.EqualTo("This is an instance of TestEntityOne59"));

      var result591 = session.Query.All<PartTwo.TestEntityTwo59>().ToArray();
      Assert.That(result591.Length, Is.EqualTo(1));
      Assert.That(result591[0].BooleanField, Is.True);
      Assert.That(result591[0].Int16Field, Is.EqualTo(59));
      Assert.That(result591[0].Int32Field, Is.EqualTo(59));
      Assert.That(result591[0].Int64Field, Is.EqualTo(59));
      Assert.That(result591[0].FloatField, Is.EqualTo((float)59));
      Assert.That(result591[0].DoubleField, Is.EqualTo((double)59));
      Assert.That(result591[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result591[0].StringField, Is.EqualTo("TestEntityTwo59"));
      Assert.That(result591[0].Text, Is.EqualTo("This is an instance of TestEntityTwo59"));
      var result600 = session.Query.All<PartOne.TestEntityOne60>().ToArray();
      Assert.That(result600.Length, Is.EqualTo(1));
      Assert.That(result600[0].BooleanField, Is.True);
      Assert.That(result600[0].Int16Field, Is.EqualTo(60));
      Assert.That(result600[0].Int32Field, Is.EqualTo(60));
      Assert.That(result600[0].Int64Field, Is.EqualTo(60));
      Assert.That(result600[0].FloatField, Is.EqualTo((float)60));
      Assert.That(result600[0].DoubleField, Is.EqualTo((double)60));
      Assert.That(result600[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result600[0].StringField, Is.EqualTo("TestEntityOne60"));
      Assert.That(result600[0].Text, Is.EqualTo("This is an instance of TestEntityOne60"));

      var result601 = session.Query.All<PartTwo.TestEntityTwo60>().ToArray();
      Assert.That(result601.Length, Is.EqualTo(1));
      Assert.That(result601[0].BooleanField, Is.True);
      Assert.That(result601[0].Int16Field, Is.EqualTo(60));
      Assert.That(result601[0].Int32Field, Is.EqualTo(60));
      Assert.That(result601[0].Int64Field, Is.EqualTo(60));
      Assert.That(result601[0].FloatField, Is.EqualTo((float)60));
      Assert.That(result601[0].DoubleField, Is.EqualTo((double)60));
      Assert.That(result601[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result601[0].StringField, Is.EqualTo("TestEntityTwo60"));
      Assert.That(result601[0].Text, Is.EqualTo("This is an instance of TestEntityTwo60"));
      var result610 = session.Query.All<PartOne.TestEntityOne61>().ToArray();
      Assert.That(result610.Length, Is.EqualTo(1));
      Assert.That(result610[0].BooleanField, Is.True);
      Assert.That(result610[0].Int16Field, Is.EqualTo(61));
      Assert.That(result610[0].Int32Field, Is.EqualTo(61));
      Assert.That(result610[0].Int64Field, Is.EqualTo(61));
      Assert.That(result610[0].FloatField, Is.EqualTo((float)61));
      Assert.That(result610[0].DoubleField, Is.EqualTo((double)61));
      Assert.That(result610[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result610[0].StringField, Is.EqualTo("TestEntityOne61"));
      Assert.That(result610[0].Text, Is.EqualTo("This is an instance of TestEntityOne61"));

      var result611 = session.Query.All<PartTwo.TestEntityTwo61>().ToArray();
      Assert.That(result611.Length, Is.EqualTo(1));
      Assert.That(result611[0].BooleanField, Is.True);
      Assert.That(result611[0].Int16Field, Is.EqualTo(61));
      Assert.That(result611[0].Int32Field, Is.EqualTo(61));
      Assert.That(result611[0].Int64Field, Is.EqualTo(61));
      Assert.That(result611[0].FloatField, Is.EqualTo((float)61));
      Assert.That(result611[0].DoubleField, Is.EqualTo((double)61));
      Assert.That(result611[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result611[0].StringField, Is.EqualTo("TestEntityTwo61"));
      Assert.That(result611[0].Text, Is.EqualTo("This is an instance of TestEntityTwo61"));
      var result620 = session.Query.All<PartOne.TestEntityOne62>().ToArray();
      Assert.That(result620.Length, Is.EqualTo(1));
      Assert.That(result620[0].BooleanField, Is.True);
      Assert.That(result620[0].Int16Field, Is.EqualTo(62));
      Assert.That(result620[0].Int32Field, Is.EqualTo(62));
      Assert.That(result620[0].Int64Field, Is.EqualTo(62));
      Assert.That(result620[0].FloatField, Is.EqualTo((float)62));
      Assert.That(result620[0].DoubleField, Is.EqualTo((double)62));
      Assert.That(result620[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result620[0].StringField, Is.EqualTo("TestEntityOne62"));
      Assert.That(result620[0].Text, Is.EqualTo("This is an instance of TestEntityOne62"));

      var result621 = session.Query.All<PartTwo.TestEntityTwo62>().ToArray();
      Assert.That(result621.Length, Is.EqualTo(1));
      Assert.That(result621[0].BooleanField, Is.True);
      Assert.That(result621[0].Int16Field, Is.EqualTo(62));
      Assert.That(result621[0].Int32Field, Is.EqualTo(62));
      Assert.That(result621[0].Int64Field, Is.EqualTo(62));
      Assert.That(result621[0].FloatField, Is.EqualTo((float)62));
      Assert.That(result621[0].DoubleField, Is.EqualTo((double)62));
      Assert.That(result621[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result621[0].StringField, Is.EqualTo("TestEntityTwo62"));
      Assert.That(result621[0].Text, Is.EqualTo("This is an instance of TestEntityTwo62"));
      var result630 = session.Query.All<PartOne.TestEntityOne63>().ToArray();
      Assert.That(result630.Length, Is.EqualTo(1));
      Assert.That(result630[0].BooleanField, Is.True);
      Assert.That(result630[0].Int16Field, Is.EqualTo(63));
      Assert.That(result630[0].Int32Field, Is.EqualTo(63));
      Assert.That(result630[0].Int64Field, Is.EqualTo(63));
      Assert.That(result630[0].FloatField, Is.EqualTo((float)63));
      Assert.That(result630[0].DoubleField, Is.EqualTo((double)63));
      Assert.That(result630[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result630[0].StringField, Is.EqualTo("TestEntityOne63"));
      Assert.That(result630[0].Text, Is.EqualTo("This is an instance of TestEntityOne63"));

      var result631 = session.Query.All<PartTwo.TestEntityTwo63>().ToArray();
      Assert.That(result631.Length, Is.EqualTo(1));
      Assert.That(result631[0].BooleanField, Is.True);
      Assert.That(result631[0].Int16Field, Is.EqualTo(63));
      Assert.That(result631[0].Int32Field, Is.EqualTo(63));
      Assert.That(result631[0].Int64Field, Is.EqualTo(63));
      Assert.That(result631[0].FloatField, Is.EqualTo((float)63));
      Assert.That(result631[0].DoubleField, Is.EqualTo((double)63));
      Assert.That(result631[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result631[0].StringField, Is.EqualTo("TestEntityTwo63"));
      Assert.That(result631[0].Text, Is.EqualTo("This is an instance of TestEntityTwo63"));
      var result640 = session.Query.All<PartOne.TestEntityOne64>().ToArray();
      Assert.That(result640.Length, Is.EqualTo(1));
      Assert.That(result640[0].BooleanField, Is.True);
      Assert.That(result640[0].Int16Field, Is.EqualTo(64));
      Assert.That(result640[0].Int32Field, Is.EqualTo(64));
      Assert.That(result640[0].Int64Field, Is.EqualTo(64));
      Assert.That(result640[0].FloatField, Is.EqualTo((float)64));
      Assert.That(result640[0].DoubleField, Is.EqualTo((double)64));
      Assert.That(result640[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result640[0].StringField, Is.EqualTo("TestEntityOne64"));
      Assert.That(result640[0].Text, Is.EqualTo("This is an instance of TestEntityOne64"));

      var result641 = session.Query.All<PartTwo.TestEntityTwo64>().ToArray();
      Assert.That(result641.Length, Is.EqualTo(1));
      Assert.That(result641[0].BooleanField, Is.True);
      Assert.That(result641[0].Int16Field, Is.EqualTo(64));
      Assert.That(result641[0].Int32Field, Is.EqualTo(64));
      Assert.That(result641[0].Int64Field, Is.EqualTo(64));
      Assert.That(result641[0].FloatField, Is.EqualTo((float)64));
      Assert.That(result641[0].DoubleField, Is.EqualTo((double)64));
      Assert.That(result641[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result641[0].StringField, Is.EqualTo("TestEntityTwo64"));
      Assert.That(result641[0].Text, Is.EqualTo("This is an instance of TestEntityTwo64"));
      var result650 = session.Query.All<PartOne.TestEntityOne65>().ToArray();
      Assert.That(result650.Length, Is.EqualTo(1));
      Assert.That(result650[0].BooleanField, Is.True);
      Assert.That(result650[0].Int16Field, Is.EqualTo(65));
      Assert.That(result650[0].Int32Field, Is.EqualTo(65));
      Assert.That(result650[0].Int64Field, Is.EqualTo(65));
      Assert.That(result650[0].FloatField, Is.EqualTo((float)65));
      Assert.That(result650[0].DoubleField, Is.EqualTo((double)65));
      Assert.That(result650[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result650[0].StringField, Is.EqualTo("TestEntityOne65"));
      Assert.That(result650[0].Text, Is.EqualTo("This is an instance of TestEntityOne65"));

      var result651 = session.Query.All<PartTwo.TestEntityTwo65>().ToArray();
      Assert.That(result651.Length, Is.EqualTo(1));
      Assert.That(result651[0].BooleanField, Is.True);
      Assert.That(result651[0].Int16Field, Is.EqualTo(65));
      Assert.That(result651[0].Int32Field, Is.EqualTo(65));
      Assert.That(result651[0].Int64Field, Is.EqualTo(65));
      Assert.That(result651[0].FloatField, Is.EqualTo((float)65));
      Assert.That(result651[0].DoubleField, Is.EqualTo((double)65));
      Assert.That(result651[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result651[0].StringField, Is.EqualTo("TestEntityTwo65"));
      Assert.That(result651[0].Text, Is.EqualTo("This is an instance of TestEntityTwo65"));
      var result660 = session.Query.All<PartOne.TestEntityOne66>().ToArray();
      Assert.That(result660.Length, Is.EqualTo(1));
      Assert.That(result660[0].BooleanField, Is.True);
      Assert.That(result660[0].Int16Field, Is.EqualTo(66));
      Assert.That(result660[0].Int32Field, Is.EqualTo(66));
      Assert.That(result660[0].Int64Field, Is.EqualTo(66));
      Assert.That(result660[0].FloatField, Is.EqualTo((float)66));
      Assert.That(result660[0].DoubleField, Is.EqualTo((double)66));
      Assert.That(result660[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result660[0].StringField, Is.EqualTo("TestEntityOne66"));
      Assert.That(result660[0].Text, Is.EqualTo("This is an instance of TestEntityOne66"));

      var result661 = session.Query.All<PartTwo.TestEntityTwo66>().ToArray();
      Assert.That(result661.Length, Is.EqualTo(1));
      Assert.That(result661[0].BooleanField, Is.True);
      Assert.That(result661[0].Int16Field, Is.EqualTo(66));
      Assert.That(result661[0].Int32Field, Is.EqualTo(66));
      Assert.That(result661[0].Int64Field, Is.EqualTo(66));
      Assert.That(result661[0].FloatField, Is.EqualTo((float)66));
      Assert.That(result661[0].DoubleField, Is.EqualTo((double)66));
      Assert.That(result661[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result661[0].StringField, Is.EqualTo("TestEntityTwo66"));
      Assert.That(result661[0].Text, Is.EqualTo("This is an instance of TestEntityTwo66"));
      var result670 = session.Query.All<PartOne.TestEntityOne67>().ToArray();
      Assert.That(result670.Length, Is.EqualTo(1));
      Assert.That(result670[0].BooleanField, Is.True);
      Assert.That(result670[0].Int16Field, Is.EqualTo(67));
      Assert.That(result670[0].Int32Field, Is.EqualTo(67));
      Assert.That(result670[0].Int64Field, Is.EqualTo(67));
      Assert.That(result670[0].FloatField, Is.EqualTo((float)67));
      Assert.That(result670[0].DoubleField, Is.EqualTo((double)67));
      Assert.That(result670[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result670[0].StringField, Is.EqualTo("TestEntityOne67"));
      Assert.That(result670[0].Text, Is.EqualTo("This is an instance of TestEntityOne67"));

      var result671 = session.Query.All<PartTwo.TestEntityTwo67>().ToArray();
      Assert.That(result671.Length, Is.EqualTo(1));
      Assert.That(result671[0].BooleanField, Is.True);
      Assert.That(result671[0].Int16Field, Is.EqualTo(67));
      Assert.That(result671[0].Int32Field, Is.EqualTo(67));
      Assert.That(result671[0].Int64Field, Is.EqualTo(67));
      Assert.That(result671[0].FloatField, Is.EqualTo((float)67));
      Assert.That(result671[0].DoubleField, Is.EqualTo((double)67));
      Assert.That(result671[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result671[0].StringField, Is.EqualTo("TestEntityTwo67"));
      Assert.That(result671[0].Text, Is.EqualTo("This is an instance of TestEntityTwo67"));
      var result680 = session.Query.All<PartOne.TestEntityOne68>().ToArray();
      Assert.That(result680.Length, Is.EqualTo(1));
      Assert.That(result680[0].BooleanField, Is.True);
      Assert.That(result680[0].Int16Field, Is.EqualTo(68));
      Assert.That(result680[0].Int32Field, Is.EqualTo(68));
      Assert.That(result680[0].Int64Field, Is.EqualTo(68));
      Assert.That(result680[0].FloatField, Is.EqualTo((float)68));
      Assert.That(result680[0].DoubleField, Is.EqualTo((double)68));
      Assert.That(result680[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result680[0].StringField, Is.EqualTo("TestEntityOne68"));
      Assert.That(result680[0].Text, Is.EqualTo("This is an instance of TestEntityOne68"));

      var result681 = session.Query.All<PartTwo.TestEntityTwo68>().ToArray();
      Assert.That(result681.Length, Is.EqualTo(1));
      Assert.That(result681[0].BooleanField, Is.True);
      Assert.That(result681[0].Int16Field, Is.EqualTo(68));
      Assert.That(result681[0].Int32Field, Is.EqualTo(68));
      Assert.That(result681[0].Int64Field, Is.EqualTo(68));
      Assert.That(result681[0].FloatField, Is.EqualTo((float)68));
      Assert.That(result681[0].DoubleField, Is.EqualTo((double)68));
      Assert.That(result681[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result681[0].StringField, Is.EqualTo("TestEntityTwo68"));
      Assert.That(result681[0].Text, Is.EqualTo("This is an instance of TestEntityTwo68"));
      var result690 = session.Query.All<PartOne.TestEntityOne69>().ToArray();
      Assert.That(result690.Length, Is.EqualTo(1));
      Assert.That(result690[0].BooleanField, Is.True);
      Assert.That(result690[0].Int16Field, Is.EqualTo(69));
      Assert.That(result690[0].Int32Field, Is.EqualTo(69));
      Assert.That(result690[0].Int64Field, Is.EqualTo(69));
      Assert.That(result690[0].FloatField, Is.EqualTo((float)69));
      Assert.That(result690[0].DoubleField, Is.EqualTo((double)69));
      Assert.That(result690[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result690[0].StringField, Is.EqualTo("TestEntityOne69"));
      Assert.That(result690[0].Text, Is.EqualTo("This is an instance of TestEntityOne69"));

      var result691 = session.Query.All<PartTwo.TestEntityTwo69>().ToArray();
      Assert.That(result691.Length, Is.EqualTo(1));
      Assert.That(result691[0].BooleanField, Is.True);
      Assert.That(result691[0].Int16Field, Is.EqualTo(69));
      Assert.That(result691[0].Int32Field, Is.EqualTo(69));
      Assert.That(result691[0].Int64Field, Is.EqualTo(69));
      Assert.That(result691[0].FloatField, Is.EqualTo((float)69));
      Assert.That(result691[0].DoubleField, Is.EqualTo((double)69));
      Assert.That(result691[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result691[0].StringField, Is.EqualTo("TestEntityTwo69"));
      Assert.That(result691[0].Text, Is.EqualTo("This is an instance of TestEntityTwo69"));
      var result700 = session.Query.All<PartOne.TestEntityOne70>().ToArray();
      Assert.That(result700.Length, Is.EqualTo(1));
      Assert.That(result700[0].BooleanField, Is.True);
      Assert.That(result700[0].Int16Field, Is.EqualTo(70));
      Assert.That(result700[0].Int32Field, Is.EqualTo(70));
      Assert.That(result700[0].Int64Field, Is.EqualTo(70));
      Assert.That(result700[0].FloatField, Is.EqualTo((float)70));
      Assert.That(result700[0].DoubleField, Is.EqualTo((double)70));
      Assert.That(result700[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result700[0].StringField, Is.EqualTo("TestEntityOne70"));
      Assert.That(result700[0].Text, Is.EqualTo("This is an instance of TestEntityOne70"));

      var result701 = session.Query.All<PartTwo.TestEntityTwo70>().ToArray();
      Assert.That(result701.Length, Is.EqualTo(1));
      Assert.That(result701[0].BooleanField, Is.True);
      Assert.That(result701[0].Int16Field, Is.EqualTo(70));
      Assert.That(result701[0].Int32Field, Is.EqualTo(70));
      Assert.That(result701[0].Int64Field, Is.EqualTo(70));
      Assert.That(result701[0].FloatField, Is.EqualTo((float)70));
      Assert.That(result701[0].DoubleField, Is.EqualTo((double)70));
      Assert.That(result701[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result701[0].StringField, Is.EqualTo("TestEntityTwo70"));
      Assert.That(result701[0].Text, Is.EqualTo("This is an instance of TestEntityTwo70"));
      var result710 = session.Query.All<PartOne.TestEntityOne71>().ToArray();
      Assert.That(result710.Length, Is.EqualTo(1));
      Assert.That(result710[0].BooleanField, Is.True);
      Assert.That(result710[0].Int16Field, Is.EqualTo(71));
      Assert.That(result710[0].Int32Field, Is.EqualTo(71));
      Assert.That(result710[0].Int64Field, Is.EqualTo(71));
      Assert.That(result710[0].FloatField, Is.EqualTo((float)71));
      Assert.That(result710[0].DoubleField, Is.EqualTo((double)71));
      Assert.That(result710[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result710[0].StringField, Is.EqualTo("TestEntityOne71"));
      Assert.That(result710[0].Text, Is.EqualTo("This is an instance of TestEntityOne71"));

      var result711 = session.Query.All<PartTwo.TestEntityTwo71>().ToArray();
      Assert.That(result711.Length, Is.EqualTo(1));
      Assert.That(result711[0].BooleanField, Is.True);
      Assert.That(result711[0].Int16Field, Is.EqualTo(71));
      Assert.That(result711[0].Int32Field, Is.EqualTo(71));
      Assert.That(result711[0].Int64Field, Is.EqualTo(71));
      Assert.That(result711[0].FloatField, Is.EqualTo((float)71));
      Assert.That(result711[0].DoubleField, Is.EqualTo((double)71));
      Assert.That(result711[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result711[0].StringField, Is.EqualTo("TestEntityTwo71"));
      Assert.That(result711[0].Text, Is.EqualTo("This is an instance of TestEntityTwo71"));
      var result720 = session.Query.All<PartOne.TestEntityOne72>().ToArray();
      Assert.That(result720.Length, Is.EqualTo(1));
      Assert.That(result720[0].BooleanField, Is.True);
      Assert.That(result720[0].Int16Field, Is.EqualTo(72));
      Assert.That(result720[0].Int32Field, Is.EqualTo(72));
      Assert.That(result720[0].Int64Field, Is.EqualTo(72));
      Assert.That(result720[0].FloatField, Is.EqualTo((float)72));
      Assert.That(result720[0].DoubleField, Is.EqualTo((double)72));
      Assert.That(result720[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result720[0].StringField, Is.EqualTo("TestEntityOne72"));
      Assert.That(result720[0].Text, Is.EqualTo("This is an instance of TestEntityOne72"));

      var result721 = session.Query.All<PartTwo.TestEntityTwo72>().ToArray();
      Assert.That(result721.Length, Is.EqualTo(1));
      Assert.That(result721[0].BooleanField, Is.True);
      Assert.That(result721[0].Int16Field, Is.EqualTo(72));
      Assert.That(result721[0].Int32Field, Is.EqualTo(72));
      Assert.That(result721[0].Int64Field, Is.EqualTo(72));
      Assert.That(result721[0].FloatField, Is.EqualTo((float)72));
      Assert.That(result721[0].DoubleField, Is.EqualTo((double)72));
      Assert.That(result721[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result721[0].StringField, Is.EqualTo("TestEntityTwo72"));
      Assert.That(result721[0].Text, Is.EqualTo("This is an instance of TestEntityTwo72"));
      var result730 = session.Query.All<PartOne.TestEntityOne73>().ToArray();
      Assert.That(result730.Length, Is.EqualTo(1));
      Assert.That(result730[0].BooleanField, Is.True);
      Assert.That(result730[0].Int16Field, Is.EqualTo(73));
      Assert.That(result730[0].Int32Field, Is.EqualTo(73));
      Assert.That(result730[0].Int64Field, Is.EqualTo(73));
      Assert.That(result730[0].FloatField, Is.EqualTo((float)73));
      Assert.That(result730[0].DoubleField, Is.EqualTo((double)73));
      Assert.That(result730[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result730[0].StringField, Is.EqualTo("TestEntityOne73"));
      Assert.That(result730[0].Text, Is.EqualTo("This is an instance of TestEntityOne73"));

      var result731 = session.Query.All<PartTwo.TestEntityTwo73>().ToArray();
      Assert.That(result731.Length, Is.EqualTo(1));
      Assert.That(result731[0].BooleanField, Is.True);
      Assert.That(result731[0].Int16Field, Is.EqualTo(73));
      Assert.That(result731[0].Int32Field, Is.EqualTo(73));
      Assert.That(result731[0].Int64Field, Is.EqualTo(73));
      Assert.That(result731[0].FloatField, Is.EqualTo((float)73));
      Assert.That(result731[0].DoubleField, Is.EqualTo((double)73));
      Assert.That(result731[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result731[0].StringField, Is.EqualTo("TestEntityTwo73"));
      Assert.That(result731[0].Text, Is.EqualTo("This is an instance of TestEntityTwo73"));
      var result740 = session.Query.All<PartOne.TestEntityOne74>().ToArray();
      Assert.That(result740.Length, Is.EqualTo(1));
      Assert.That(result740[0].BooleanField, Is.True);
      Assert.That(result740[0].Int16Field, Is.EqualTo(74));
      Assert.That(result740[0].Int32Field, Is.EqualTo(74));
      Assert.That(result740[0].Int64Field, Is.EqualTo(74));
      Assert.That(result740[0].FloatField, Is.EqualTo((float)74));
      Assert.That(result740[0].DoubleField, Is.EqualTo((double)74));
      Assert.That(result740[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result740[0].StringField, Is.EqualTo("TestEntityOne74"));
      Assert.That(result740[0].Text, Is.EqualTo("This is an instance of TestEntityOne74"));

      var result741 = session.Query.All<PartTwo.TestEntityTwo74>().ToArray();
      Assert.That(result741.Length, Is.EqualTo(1));
      Assert.That(result741[0].BooleanField, Is.True);
      Assert.That(result741[0].Int16Field, Is.EqualTo(74));
      Assert.That(result741[0].Int32Field, Is.EqualTo(74));
      Assert.That(result741[0].Int64Field, Is.EqualTo(74));
      Assert.That(result741[0].FloatField, Is.EqualTo((float)74));
      Assert.That(result741[0].DoubleField, Is.EqualTo((double)74));
      Assert.That(result741[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result741[0].StringField, Is.EqualTo("TestEntityTwo74"));
      Assert.That(result741[0].Text, Is.EqualTo("This is an instance of TestEntityTwo74"));
      var result750 = session.Query.All<PartOne.TestEntityOne75>().ToArray();
      Assert.That(result750.Length, Is.EqualTo(1));
      Assert.That(result750[0].BooleanField, Is.True);
      Assert.That(result750[0].Int16Field, Is.EqualTo(75));
      Assert.That(result750[0].Int32Field, Is.EqualTo(75));
      Assert.That(result750[0].Int64Field, Is.EqualTo(75));
      Assert.That(result750[0].FloatField, Is.EqualTo((float)75));
      Assert.That(result750[0].DoubleField, Is.EqualTo((double)75));
      Assert.That(result750[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result750[0].StringField, Is.EqualTo("TestEntityOne75"));
      Assert.That(result750[0].Text, Is.EqualTo("This is an instance of TestEntityOne75"));

      var result751 = session.Query.All<PartTwo.TestEntityTwo75>().ToArray();
      Assert.That(result751.Length, Is.EqualTo(1));
      Assert.That(result751[0].BooleanField, Is.True);
      Assert.That(result751[0].Int16Field, Is.EqualTo(75));
      Assert.That(result751[0].Int32Field, Is.EqualTo(75));
      Assert.That(result751[0].Int64Field, Is.EqualTo(75));
      Assert.That(result751[0].FloatField, Is.EqualTo((float)75));
      Assert.That(result751[0].DoubleField, Is.EqualTo((double)75));
      Assert.That(result751[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result751[0].StringField, Is.EqualTo("TestEntityTwo75"));
      Assert.That(result751[0].Text, Is.EqualTo("This is an instance of TestEntityTwo75"));
      var result760 = session.Query.All<PartOne.TestEntityOne76>().ToArray();
      Assert.That(result760.Length, Is.EqualTo(1));
      Assert.That(result760[0].BooleanField, Is.True);
      Assert.That(result760[0].Int16Field, Is.EqualTo(76));
      Assert.That(result760[0].Int32Field, Is.EqualTo(76));
      Assert.That(result760[0].Int64Field, Is.EqualTo(76));
      Assert.That(result760[0].FloatField, Is.EqualTo((float)76));
      Assert.That(result760[0].DoubleField, Is.EqualTo((double)76));
      Assert.That(result760[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result760[0].StringField, Is.EqualTo("TestEntityOne76"));
      Assert.That(result760[0].Text, Is.EqualTo("This is an instance of TestEntityOne76"));

      var result761 = session.Query.All<PartTwo.TestEntityTwo76>().ToArray();
      Assert.That(result761.Length, Is.EqualTo(1));
      Assert.That(result761[0].BooleanField, Is.True);
      Assert.That(result761[0].Int16Field, Is.EqualTo(76));
      Assert.That(result761[0].Int32Field, Is.EqualTo(76));
      Assert.That(result761[0].Int64Field, Is.EqualTo(76));
      Assert.That(result761[0].FloatField, Is.EqualTo((float)76));
      Assert.That(result761[0].DoubleField, Is.EqualTo((double)76));
      Assert.That(result761[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result761[0].StringField, Is.EqualTo("TestEntityTwo76"));
      Assert.That(result761[0].Text, Is.EqualTo("This is an instance of TestEntityTwo76"));
      var result770 = session.Query.All<PartOne.TestEntityOne77>().ToArray();
      Assert.That(result770.Length, Is.EqualTo(1));
      Assert.That(result770[0].BooleanField, Is.True);
      Assert.That(result770[0].Int16Field, Is.EqualTo(77));
      Assert.That(result770[0].Int32Field, Is.EqualTo(77));
      Assert.That(result770[0].Int64Field, Is.EqualTo(77));
      Assert.That(result770[0].FloatField, Is.EqualTo((float)77));
      Assert.That(result770[0].DoubleField, Is.EqualTo((double)77));
      Assert.That(result770[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result770[0].StringField, Is.EqualTo("TestEntityOne77"));
      Assert.That(result770[0].Text, Is.EqualTo("This is an instance of TestEntityOne77"));

      var result771 = session.Query.All<PartTwo.TestEntityTwo77>().ToArray();
      Assert.That(result771.Length, Is.EqualTo(1));
      Assert.That(result771[0].BooleanField, Is.True);
      Assert.That(result771[0].Int16Field, Is.EqualTo(77));
      Assert.That(result771[0].Int32Field, Is.EqualTo(77));
      Assert.That(result771[0].Int64Field, Is.EqualTo(77));
      Assert.That(result771[0].FloatField, Is.EqualTo((float)77));
      Assert.That(result771[0].DoubleField, Is.EqualTo((double)77));
      Assert.That(result771[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result771[0].StringField, Is.EqualTo("TestEntityTwo77"));
      Assert.That(result771[0].Text, Is.EqualTo("This is an instance of TestEntityTwo77"));
      var result780 = session.Query.All<PartOne.TestEntityOne78>().ToArray();
      Assert.That(result780.Length, Is.EqualTo(1));
      Assert.That(result780[0].BooleanField, Is.True);
      Assert.That(result780[0].Int16Field, Is.EqualTo(78));
      Assert.That(result780[0].Int32Field, Is.EqualTo(78));
      Assert.That(result780[0].Int64Field, Is.EqualTo(78));
      Assert.That(result780[0].FloatField, Is.EqualTo((float)78));
      Assert.That(result780[0].DoubleField, Is.EqualTo((double)78));
      Assert.That(result780[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result780[0].StringField, Is.EqualTo("TestEntityOne78"));
      Assert.That(result780[0].Text, Is.EqualTo("This is an instance of TestEntityOne78"));

      var result781 = session.Query.All<PartTwo.TestEntityTwo78>().ToArray();
      Assert.That(result781.Length, Is.EqualTo(1));
      Assert.That(result781[0].BooleanField, Is.True);
      Assert.That(result781[0].Int16Field, Is.EqualTo(78));
      Assert.That(result781[0].Int32Field, Is.EqualTo(78));
      Assert.That(result781[0].Int64Field, Is.EqualTo(78));
      Assert.That(result781[0].FloatField, Is.EqualTo((float)78));
      Assert.That(result781[0].DoubleField, Is.EqualTo((double)78));
      Assert.That(result781[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result781[0].StringField, Is.EqualTo("TestEntityTwo78"));
      Assert.That(result781[0].Text, Is.EqualTo("This is an instance of TestEntityTwo78"));
      var result790 = session.Query.All<PartOne.TestEntityOne79>().ToArray();
      Assert.That(result790.Length, Is.EqualTo(1));
      Assert.That(result790[0].BooleanField, Is.True);
      Assert.That(result790[0].Int16Field, Is.EqualTo(79));
      Assert.That(result790[0].Int32Field, Is.EqualTo(79));
      Assert.That(result790[0].Int64Field, Is.EqualTo(79));
      Assert.That(result790[0].FloatField, Is.EqualTo((float)79));
      Assert.That(result790[0].DoubleField, Is.EqualTo((double)79));
      Assert.That(result790[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result790[0].StringField, Is.EqualTo("TestEntityOne79"));
      Assert.That(result790[0].Text, Is.EqualTo("This is an instance of TestEntityOne79"));

      var result791 = session.Query.All<PartTwo.TestEntityTwo79>().ToArray();
      Assert.That(result791.Length, Is.EqualTo(1));
      Assert.That(result791[0].BooleanField, Is.True);
      Assert.That(result791[0].Int16Field, Is.EqualTo(79));
      Assert.That(result791[0].Int32Field, Is.EqualTo(79));
      Assert.That(result791[0].Int64Field, Is.EqualTo(79));
      Assert.That(result791[0].FloatField, Is.EqualTo((float)79));
      Assert.That(result791[0].DoubleField, Is.EqualTo((double)79));
      Assert.That(result791[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result791[0].StringField, Is.EqualTo("TestEntityTwo79"));
      Assert.That(result791[0].Text, Is.EqualTo("This is an instance of TestEntityTwo79"));
      var result800 = session.Query.All<PartOne.TestEntityOne80>().ToArray();
      Assert.That(result800.Length, Is.EqualTo(1));
      Assert.That(result800[0].BooleanField, Is.True);
      Assert.That(result800[0].Int16Field, Is.EqualTo(80));
      Assert.That(result800[0].Int32Field, Is.EqualTo(80));
      Assert.That(result800[0].Int64Field, Is.EqualTo(80));
      Assert.That(result800[0].FloatField, Is.EqualTo((float)80));
      Assert.That(result800[0].DoubleField, Is.EqualTo((double)80));
      Assert.That(result800[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result800[0].StringField, Is.EqualTo("TestEntityOne80"));
      Assert.That(result800[0].Text, Is.EqualTo("This is an instance of TestEntityOne80"));

      var result801 = session.Query.All<PartTwo.TestEntityTwo80>().ToArray();
      Assert.That(result801.Length, Is.EqualTo(1));
      Assert.That(result801[0].BooleanField, Is.True);
      Assert.That(result801[0].Int16Field, Is.EqualTo(80));
      Assert.That(result801[0].Int32Field, Is.EqualTo(80));
      Assert.That(result801[0].Int64Field, Is.EqualTo(80));
      Assert.That(result801[0].FloatField, Is.EqualTo((float)80));
      Assert.That(result801[0].DoubleField, Is.EqualTo((double)80));
      Assert.That(result801[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result801[0].StringField, Is.EqualTo("TestEntityTwo80"));
      Assert.That(result801[0].Text, Is.EqualTo("This is an instance of TestEntityTwo80"));
      var result810 = session.Query.All<PartOne.TestEntityOne81>().ToArray();
      Assert.That(result810.Length, Is.EqualTo(1));
      Assert.That(result810[0].BooleanField, Is.True);
      Assert.That(result810[0].Int16Field, Is.EqualTo(81));
      Assert.That(result810[0].Int32Field, Is.EqualTo(81));
      Assert.That(result810[0].Int64Field, Is.EqualTo(81));
      Assert.That(result810[0].FloatField, Is.EqualTo((float)81));
      Assert.That(result810[0].DoubleField, Is.EqualTo((double)81));
      Assert.That(result810[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result810[0].StringField, Is.EqualTo("TestEntityOne81"));
      Assert.That(result810[0].Text, Is.EqualTo("This is an instance of TestEntityOne81"));

      var result811 = session.Query.All<PartTwo.TestEntityTwo81>().ToArray();
      Assert.That(result811.Length, Is.EqualTo(1));
      Assert.That(result811[0].BooleanField, Is.True);
      Assert.That(result811[0].Int16Field, Is.EqualTo(81));
      Assert.That(result811[0].Int32Field, Is.EqualTo(81));
      Assert.That(result811[0].Int64Field, Is.EqualTo(81));
      Assert.That(result811[0].FloatField, Is.EqualTo((float)81));
      Assert.That(result811[0].DoubleField, Is.EqualTo((double)81));
      Assert.That(result811[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result811[0].StringField, Is.EqualTo("TestEntityTwo81"));
      Assert.That(result811[0].Text, Is.EqualTo("This is an instance of TestEntityTwo81"));
      var result820 = session.Query.All<PartOne.TestEntityOne82>().ToArray();
      Assert.That(result820.Length, Is.EqualTo(1));
      Assert.That(result820[0].BooleanField, Is.True);
      Assert.That(result820[0].Int16Field, Is.EqualTo(82));
      Assert.That(result820[0].Int32Field, Is.EqualTo(82));
      Assert.That(result820[0].Int64Field, Is.EqualTo(82));
      Assert.That(result820[0].FloatField, Is.EqualTo((float)82));
      Assert.That(result820[0].DoubleField, Is.EqualTo((double)82));
      Assert.That(result820[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result820[0].StringField, Is.EqualTo("TestEntityOne82"));
      Assert.That(result820[0].Text, Is.EqualTo("This is an instance of TestEntityOne82"));

      var result821 = session.Query.All<PartTwo.TestEntityTwo82>().ToArray();
      Assert.That(result821.Length, Is.EqualTo(1));
      Assert.That(result821[0].BooleanField, Is.True);
      Assert.That(result821[0].Int16Field, Is.EqualTo(82));
      Assert.That(result821[0].Int32Field, Is.EqualTo(82));
      Assert.That(result821[0].Int64Field, Is.EqualTo(82));
      Assert.That(result821[0].FloatField, Is.EqualTo((float)82));
      Assert.That(result821[0].DoubleField, Is.EqualTo((double)82));
      Assert.That(result821[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result821[0].StringField, Is.EqualTo("TestEntityTwo82"));
      Assert.That(result821[0].Text, Is.EqualTo("This is an instance of TestEntityTwo82"));
      var result830 = session.Query.All<PartOne.TestEntityOne83>().ToArray();
      Assert.That(result830.Length, Is.EqualTo(1));
      Assert.That(result830[0].BooleanField, Is.True);
      Assert.That(result830[0].Int16Field, Is.EqualTo(83));
      Assert.That(result830[0].Int32Field, Is.EqualTo(83));
      Assert.That(result830[0].Int64Field, Is.EqualTo(83));
      Assert.That(result830[0].FloatField, Is.EqualTo((float)83));
      Assert.That(result830[0].DoubleField, Is.EqualTo((double)83));
      Assert.That(result830[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result830[0].StringField, Is.EqualTo("TestEntityOne83"));
      Assert.That(result830[0].Text, Is.EqualTo("This is an instance of TestEntityOne83"));

      var result831 = session.Query.All<PartTwo.TestEntityTwo83>().ToArray();
      Assert.That(result831.Length, Is.EqualTo(1));
      Assert.That(result831[0].BooleanField, Is.True);
      Assert.That(result831[0].Int16Field, Is.EqualTo(83));
      Assert.That(result831[0].Int32Field, Is.EqualTo(83));
      Assert.That(result831[0].Int64Field, Is.EqualTo(83));
      Assert.That(result831[0].FloatField, Is.EqualTo((float)83));
      Assert.That(result831[0].DoubleField, Is.EqualTo((double)83));
      Assert.That(result831[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result831[0].StringField, Is.EqualTo("TestEntityTwo83"));
      Assert.That(result831[0].Text, Is.EqualTo("This is an instance of TestEntityTwo83"));
      var result840 = session.Query.All<PartOne.TestEntityOne84>().ToArray();
      Assert.That(result840.Length, Is.EqualTo(1));
      Assert.That(result840[0].BooleanField, Is.True);
      Assert.That(result840[0].Int16Field, Is.EqualTo(84));
      Assert.That(result840[0].Int32Field, Is.EqualTo(84));
      Assert.That(result840[0].Int64Field, Is.EqualTo(84));
      Assert.That(result840[0].FloatField, Is.EqualTo((float)84));
      Assert.That(result840[0].DoubleField, Is.EqualTo((double)84));
      Assert.That(result840[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result840[0].StringField, Is.EqualTo("TestEntityOne84"));
      Assert.That(result840[0].Text, Is.EqualTo("This is an instance of TestEntityOne84"));

      var result841 = session.Query.All<PartTwo.TestEntityTwo84>().ToArray();
      Assert.That(result841.Length, Is.EqualTo(1));
      Assert.That(result841[0].BooleanField, Is.True);
      Assert.That(result841[0].Int16Field, Is.EqualTo(84));
      Assert.That(result841[0].Int32Field, Is.EqualTo(84));
      Assert.That(result841[0].Int64Field, Is.EqualTo(84));
      Assert.That(result841[0].FloatField, Is.EqualTo((float)84));
      Assert.That(result841[0].DoubleField, Is.EqualTo((double)84));
      Assert.That(result841[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result841[0].StringField, Is.EqualTo("TestEntityTwo84"));
      Assert.That(result841[0].Text, Is.EqualTo("This is an instance of TestEntityTwo84"));
      var result850 = session.Query.All<PartOne.TestEntityOne85>().ToArray();
      Assert.That(result850.Length, Is.EqualTo(1));
      Assert.That(result850[0].BooleanField, Is.True);
      Assert.That(result850[0].Int16Field, Is.EqualTo(85));
      Assert.That(result850[0].Int32Field, Is.EqualTo(85));
      Assert.That(result850[0].Int64Field, Is.EqualTo(85));
      Assert.That(result850[0].FloatField, Is.EqualTo((float)85));
      Assert.That(result850[0].DoubleField, Is.EqualTo((double)85));
      Assert.That(result850[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result850[0].StringField, Is.EqualTo("TestEntityOne85"));
      Assert.That(result850[0].Text, Is.EqualTo("This is an instance of TestEntityOne85"));

      var result851 = session.Query.All<PartTwo.TestEntityTwo85>().ToArray();
      Assert.That(result851.Length, Is.EqualTo(1));
      Assert.That(result851[0].BooleanField, Is.True);
      Assert.That(result851[0].Int16Field, Is.EqualTo(85));
      Assert.That(result851[0].Int32Field, Is.EqualTo(85));
      Assert.That(result851[0].Int64Field, Is.EqualTo(85));
      Assert.That(result851[0].FloatField, Is.EqualTo((float)85));
      Assert.That(result851[0].DoubleField, Is.EqualTo((double)85));
      Assert.That(result851[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result851[0].StringField, Is.EqualTo("TestEntityTwo85"));
      Assert.That(result851[0].Text, Is.EqualTo("This is an instance of TestEntityTwo85"));
      var result860 = session.Query.All<PartOne.TestEntityOne86>().ToArray();
      Assert.That(result860.Length, Is.EqualTo(1));
      Assert.That(result860[0].BooleanField, Is.True);
      Assert.That(result860[0].Int16Field, Is.EqualTo(86));
      Assert.That(result860[0].Int32Field, Is.EqualTo(86));
      Assert.That(result860[0].Int64Field, Is.EqualTo(86));
      Assert.That(result860[0].FloatField, Is.EqualTo((float)86));
      Assert.That(result860[0].DoubleField, Is.EqualTo((double)86));
      Assert.That(result860[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result860[0].StringField, Is.EqualTo("TestEntityOne86"));
      Assert.That(result860[0].Text, Is.EqualTo("This is an instance of TestEntityOne86"));

      var result861 = session.Query.All<PartTwo.TestEntityTwo86>().ToArray();
      Assert.That(result861.Length, Is.EqualTo(1));
      Assert.That(result861[0].BooleanField, Is.True);
      Assert.That(result861[0].Int16Field, Is.EqualTo(86));
      Assert.That(result861[0].Int32Field, Is.EqualTo(86));
      Assert.That(result861[0].Int64Field, Is.EqualTo(86));
      Assert.That(result861[0].FloatField, Is.EqualTo((float)86));
      Assert.That(result861[0].DoubleField, Is.EqualTo((double)86));
      Assert.That(result861[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result861[0].StringField, Is.EqualTo("TestEntityTwo86"));
      Assert.That(result861[0].Text, Is.EqualTo("This is an instance of TestEntityTwo86"));
      var result870 = session.Query.All<PartOne.TestEntityOne87>().ToArray();
      Assert.That(result870.Length, Is.EqualTo(1));
      Assert.That(result870[0].BooleanField, Is.True);
      Assert.That(result870[0].Int16Field, Is.EqualTo(87));
      Assert.That(result870[0].Int32Field, Is.EqualTo(87));
      Assert.That(result870[0].Int64Field, Is.EqualTo(87));
      Assert.That(result870[0].FloatField, Is.EqualTo((float)87));
      Assert.That(result870[0].DoubleField, Is.EqualTo((double)87));
      Assert.That(result870[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result870[0].StringField, Is.EqualTo("TestEntityOne87"));
      Assert.That(result870[0].Text, Is.EqualTo("This is an instance of TestEntityOne87"));

      var result871 = session.Query.All<PartTwo.TestEntityTwo87>().ToArray();
      Assert.That(result871.Length, Is.EqualTo(1));
      Assert.That(result871[0].BooleanField, Is.True);
      Assert.That(result871[0].Int16Field, Is.EqualTo(87));
      Assert.That(result871[0].Int32Field, Is.EqualTo(87));
      Assert.That(result871[0].Int64Field, Is.EqualTo(87));
      Assert.That(result871[0].FloatField, Is.EqualTo((float)87));
      Assert.That(result871[0].DoubleField, Is.EqualTo((double)87));
      Assert.That(result871[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result871[0].StringField, Is.EqualTo("TestEntityTwo87"));
      Assert.That(result871[0].Text, Is.EqualTo("This is an instance of TestEntityTwo87"));
      var result880 = session.Query.All<PartOne.TestEntityOne88>().ToArray();
      Assert.That(result880.Length, Is.EqualTo(1));
      Assert.That(result880[0].BooleanField, Is.True);
      Assert.That(result880[0].Int16Field, Is.EqualTo(88));
      Assert.That(result880[0].Int32Field, Is.EqualTo(88));
      Assert.That(result880[0].Int64Field, Is.EqualTo(88));
      Assert.That(result880[0].FloatField, Is.EqualTo((float)88));
      Assert.That(result880[0].DoubleField, Is.EqualTo((double)88));
      Assert.That(result880[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result880[0].StringField, Is.EqualTo("TestEntityOne88"));
      Assert.That(result880[0].Text, Is.EqualTo("This is an instance of TestEntityOne88"));

      var result881 = session.Query.All<PartTwo.TestEntityTwo88>().ToArray();
      Assert.That(result881.Length, Is.EqualTo(1));
      Assert.That(result881[0].BooleanField, Is.True);
      Assert.That(result881[0].Int16Field, Is.EqualTo(88));
      Assert.That(result881[0].Int32Field, Is.EqualTo(88));
      Assert.That(result881[0].Int64Field, Is.EqualTo(88));
      Assert.That(result881[0].FloatField, Is.EqualTo((float)88));
      Assert.That(result881[0].DoubleField, Is.EqualTo((double)88));
      Assert.That(result881[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result881[0].StringField, Is.EqualTo("TestEntityTwo88"));
      Assert.That(result881[0].Text, Is.EqualTo("This is an instance of TestEntityTwo88"));
      var result890 = session.Query.All<PartOne.TestEntityOne89>().ToArray();
      Assert.That(result890.Length, Is.EqualTo(1));
      Assert.That(result890[0].BooleanField, Is.True);
      Assert.That(result890[0].Int16Field, Is.EqualTo(89));
      Assert.That(result890[0].Int32Field, Is.EqualTo(89));
      Assert.That(result890[0].Int64Field, Is.EqualTo(89));
      Assert.That(result890[0].FloatField, Is.EqualTo((float)89));
      Assert.That(result890[0].DoubleField, Is.EqualTo((double)89));
      Assert.That(result890[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result890[0].StringField, Is.EqualTo("TestEntityOne89"));
      Assert.That(result890[0].Text, Is.EqualTo("This is an instance of TestEntityOne89"));

      var result891 = session.Query.All<PartTwo.TestEntityTwo89>().ToArray();
      Assert.That(result891.Length, Is.EqualTo(1));
      Assert.That(result891[0].BooleanField, Is.True);
      Assert.That(result891[0].Int16Field, Is.EqualTo(89));
      Assert.That(result891[0].Int32Field, Is.EqualTo(89));
      Assert.That(result891[0].Int64Field, Is.EqualTo(89));
      Assert.That(result891[0].FloatField, Is.EqualTo((float)89));
      Assert.That(result891[0].DoubleField, Is.EqualTo((double)89));
      Assert.That(result891[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result891[0].StringField, Is.EqualTo("TestEntityTwo89"));
      Assert.That(result891[0].Text, Is.EqualTo("This is an instance of TestEntityTwo89"));
      var result900 = session.Query.All<PartOne.TestEntityOne90>().ToArray();
      Assert.That(result900.Length, Is.EqualTo(1));
      Assert.That(result900[0].BooleanField, Is.True);
      Assert.That(result900[0].Int16Field, Is.EqualTo(90));
      Assert.That(result900[0].Int32Field, Is.EqualTo(90));
      Assert.That(result900[0].Int64Field, Is.EqualTo(90));
      Assert.That(result900[0].FloatField, Is.EqualTo((float)90));
      Assert.That(result900[0].DoubleField, Is.EqualTo((double)90));
      Assert.That(result900[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result900[0].StringField, Is.EqualTo("TestEntityOne90"));
      Assert.That(result900[0].Text, Is.EqualTo("This is an instance of TestEntityOne90"));

      var result901 = session.Query.All<PartTwo.TestEntityTwo90>().ToArray();
      Assert.That(result901.Length, Is.EqualTo(1));
      Assert.That(result901[0].BooleanField, Is.True);
      Assert.That(result901[0].Int16Field, Is.EqualTo(90));
      Assert.That(result901[0].Int32Field, Is.EqualTo(90));
      Assert.That(result901[0].Int64Field, Is.EqualTo(90));
      Assert.That(result901[0].FloatField, Is.EqualTo((float)90));
      Assert.That(result901[0].DoubleField, Is.EqualTo((double)90));
      Assert.That(result901[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result901[0].StringField, Is.EqualTo("TestEntityTwo90"));
      Assert.That(result901[0].Text, Is.EqualTo("This is an instance of TestEntityTwo90"));
      var result910 = session.Query.All<PartOne.TestEntityOne91>().ToArray();
      Assert.That(result910.Length, Is.EqualTo(1));
      Assert.That(result910[0].BooleanField, Is.True);
      Assert.That(result910[0].Int16Field, Is.EqualTo(91));
      Assert.That(result910[0].Int32Field, Is.EqualTo(91));
      Assert.That(result910[0].Int64Field, Is.EqualTo(91));
      Assert.That(result910[0].FloatField, Is.EqualTo((float)91));
      Assert.That(result910[0].DoubleField, Is.EqualTo((double)91));
      Assert.That(result910[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result910[0].StringField, Is.EqualTo("TestEntityOne91"));
      Assert.That(result910[0].Text, Is.EqualTo("This is an instance of TestEntityOne91"));

      var result911 = session.Query.All<PartTwo.TestEntityTwo91>().ToArray();
      Assert.That(result911.Length, Is.EqualTo(1));
      Assert.That(result911[0].BooleanField, Is.True);
      Assert.That(result911[0].Int16Field, Is.EqualTo(91));
      Assert.That(result911[0].Int32Field, Is.EqualTo(91));
      Assert.That(result911[0].Int64Field, Is.EqualTo(91));
      Assert.That(result911[0].FloatField, Is.EqualTo((float)91));
      Assert.That(result911[0].DoubleField, Is.EqualTo((double)91));
      Assert.That(result911[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result911[0].StringField, Is.EqualTo("TestEntityTwo91"));
      Assert.That(result911[0].Text, Is.EqualTo("This is an instance of TestEntityTwo91"));
      var result920 = session.Query.All<PartOne.TestEntityOne92>().ToArray();
      Assert.That(result920.Length, Is.EqualTo(1));
      Assert.That(result920[0].BooleanField, Is.True);
      Assert.That(result920[0].Int16Field, Is.EqualTo(92));
      Assert.That(result920[0].Int32Field, Is.EqualTo(92));
      Assert.That(result920[0].Int64Field, Is.EqualTo(92));
      Assert.That(result920[0].FloatField, Is.EqualTo((float)92));
      Assert.That(result920[0].DoubleField, Is.EqualTo((double)92));
      Assert.That(result920[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result920[0].StringField, Is.EqualTo("TestEntityOne92"));
      Assert.That(result920[0].Text, Is.EqualTo("This is an instance of TestEntityOne92"));

      var result921 = session.Query.All<PartTwo.TestEntityTwo92>().ToArray();
      Assert.That(result921.Length, Is.EqualTo(1));
      Assert.That(result921[0].BooleanField, Is.True);
      Assert.That(result921[0].Int16Field, Is.EqualTo(92));
      Assert.That(result921[0].Int32Field, Is.EqualTo(92));
      Assert.That(result921[0].Int64Field, Is.EqualTo(92));
      Assert.That(result921[0].FloatField, Is.EqualTo((float)92));
      Assert.That(result921[0].DoubleField, Is.EqualTo((double)92));
      Assert.That(result921[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result921[0].StringField, Is.EqualTo("TestEntityTwo92"));
      Assert.That(result921[0].Text, Is.EqualTo("This is an instance of TestEntityTwo92"));
      var result930 = session.Query.All<PartOne.TestEntityOne93>().ToArray();
      Assert.That(result930.Length, Is.EqualTo(1));
      Assert.That(result930[0].BooleanField, Is.True);
      Assert.That(result930[0].Int16Field, Is.EqualTo(93));
      Assert.That(result930[0].Int32Field, Is.EqualTo(93));
      Assert.That(result930[0].Int64Field, Is.EqualTo(93));
      Assert.That(result930[0].FloatField, Is.EqualTo((float)93));
      Assert.That(result930[0].DoubleField, Is.EqualTo((double)93));
      Assert.That(result930[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result930[0].StringField, Is.EqualTo("TestEntityOne93"));
      Assert.That(result930[0].Text, Is.EqualTo("This is an instance of TestEntityOne93"));

      var result931 = session.Query.All<PartTwo.TestEntityTwo93>().ToArray();
      Assert.That(result931.Length, Is.EqualTo(1));
      Assert.That(result931[0].BooleanField, Is.True);
      Assert.That(result931[0].Int16Field, Is.EqualTo(93));
      Assert.That(result931[0].Int32Field, Is.EqualTo(93));
      Assert.That(result931[0].Int64Field, Is.EqualTo(93));
      Assert.That(result931[0].FloatField, Is.EqualTo((float)93));
      Assert.That(result931[0].DoubleField, Is.EqualTo((double)93));
      Assert.That(result931[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result931[0].StringField, Is.EqualTo("TestEntityTwo93"));
      Assert.That(result931[0].Text, Is.EqualTo("This is an instance of TestEntityTwo93"));
      var result940 = session.Query.All<PartOne.TestEntityOne94>().ToArray();
      Assert.That(result940.Length, Is.EqualTo(1));
      Assert.That(result940[0].BooleanField, Is.True);
      Assert.That(result940[0].Int16Field, Is.EqualTo(94));
      Assert.That(result940[0].Int32Field, Is.EqualTo(94));
      Assert.That(result940[0].Int64Field, Is.EqualTo(94));
      Assert.That(result940[0].FloatField, Is.EqualTo((float)94));
      Assert.That(result940[0].DoubleField, Is.EqualTo((double)94));
      Assert.That(result940[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result940[0].StringField, Is.EqualTo("TestEntityOne94"));
      Assert.That(result940[0].Text, Is.EqualTo("This is an instance of TestEntityOne94"));

      var result941 = session.Query.All<PartTwo.TestEntityTwo94>().ToArray();
      Assert.That(result941.Length, Is.EqualTo(1));
      Assert.That(result941[0].BooleanField, Is.True);
      Assert.That(result941[0].Int16Field, Is.EqualTo(94));
      Assert.That(result941[0].Int32Field, Is.EqualTo(94));
      Assert.That(result941[0].Int64Field, Is.EqualTo(94));
      Assert.That(result941[0].FloatField, Is.EqualTo((float)94));
      Assert.That(result941[0].DoubleField, Is.EqualTo((double)94));
      Assert.That(result941[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result941[0].StringField, Is.EqualTo("TestEntityTwo94"));
      Assert.That(result941[0].Text, Is.EqualTo("This is an instance of TestEntityTwo94"));
      var result950 = session.Query.All<PartOne.TestEntityOne95>().ToArray();
      Assert.That(result950.Length, Is.EqualTo(1));
      Assert.That(result950[0].BooleanField, Is.True);
      Assert.That(result950[0].Int16Field, Is.EqualTo(95));
      Assert.That(result950[0].Int32Field, Is.EqualTo(95));
      Assert.That(result950[0].Int64Field, Is.EqualTo(95));
      Assert.That(result950[0].FloatField, Is.EqualTo((float)95));
      Assert.That(result950[0].DoubleField, Is.EqualTo((double)95));
      Assert.That(result950[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result950[0].StringField, Is.EqualTo("TestEntityOne95"));
      Assert.That(result950[0].Text, Is.EqualTo("This is an instance of TestEntityOne95"));

      var result951 = session.Query.All<PartTwo.TestEntityTwo95>().ToArray();
      Assert.That(result951.Length, Is.EqualTo(1));
      Assert.That(result951[0].BooleanField, Is.True);
      Assert.That(result951[0].Int16Field, Is.EqualTo(95));
      Assert.That(result951[0].Int32Field, Is.EqualTo(95));
      Assert.That(result951[0].Int64Field, Is.EqualTo(95));
      Assert.That(result951[0].FloatField, Is.EqualTo((float)95));
      Assert.That(result951[0].DoubleField, Is.EqualTo((double)95));
      Assert.That(result951[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result951[0].StringField, Is.EqualTo("TestEntityTwo95"));
      Assert.That(result951[0].Text, Is.EqualTo("This is an instance of TestEntityTwo95"));
      var result960 = session.Query.All<PartOne.TestEntityOne96>().ToArray();
      Assert.That(result960.Length, Is.EqualTo(1));
      Assert.That(result960[0].BooleanField, Is.True);
      Assert.That(result960[0].Int16Field, Is.EqualTo(96));
      Assert.That(result960[0].Int32Field, Is.EqualTo(96));
      Assert.That(result960[0].Int64Field, Is.EqualTo(96));
      Assert.That(result960[0].FloatField, Is.EqualTo((float)96));
      Assert.That(result960[0].DoubleField, Is.EqualTo((double)96));
      Assert.That(result960[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result960[0].StringField, Is.EqualTo("TestEntityOne96"));
      Assert.That(result960[0].Text, Is.EqualTo("This is an instance of TestEntityOne96"));

      var result961 = session.Query.All<PartTwo.TestEntityTwo96>().ToArray();
      Assert.That(result961.Length, Is.EqualTo(1));
      Assert.That(result961[0].BooleanField, Is.True);
      Assert.That(result961[0].Int16Field, Is.EqualTo(96));
      Assert.That(result961[0].Int32Field, Is.EqualTo(96));
      Assert.That(result961[0].Int64Field, Is.EqualTo(96));
      Assert.That(result961[0].FloatField, Is.EqualTo((float)96));
      Assert.That(result961[0].DoubleField, Is.EqualTo((double)96));
      Assert.That(result961[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result961[0].StringField, Is.EqualTo("TestEntityTwo96"));
      Assert.That(result961[0].Text, Is.EqualTo("This is an instance of TestEntityTwo96"));
      var result970 = session.Query.All<PartOne.TestEntityOne97>().ToArray();
      Assert.That(result970.Length, Is.EqualTo(1));
      Assert.That(result970[0].BooleanField, Is.True);
      Assert.That(result970[0].Int16Field, Is.EqualTo(97));
      Assert.That(result970[0].Int32Field, Is.EqualTo(97));
      Assert.That(result970[0].Int64Field, Is.EqualTo(97));
      Assert.That(result970[0].FloatField, Is.EqualTo((float)97));
      Assert.That(result970[0].DoubleField, Is.EqualTo((double)97));
      Assert.That(result970[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result970[0].StringField, Is.EqualTo("TestEntityOne97"));
      Assert.That(result970[0].Text, Is.EqualTo("This is an instance of TestEntityOne97"));

      var result971 = session.Query.All<PartTwo.TestEntityTwo97>().ToArray();
      Assert.That(result971.Length, Is.EqualTo(1));
      Assert.That(result971[0].BooleanField, Is.True);
      Assert.That(result971[0].Int16Field, Is.EqualTo(97));
      Assert.That(result971[0].Int32Field, Is.EqualTo(97));
      Assert.That(result971[0].Int64Field, Is.EqualTo(97));
      Assert.That(result971[0].FloatField, Is.EqualTo((float)97));
      Assert.That(result971[0].DoubleField, Is.EqualTo((double)97));
      Assert.That(result971[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result971[0].StringField, Is.EqualTo("TestEntityTwo97"));
      Assert.That(result971[0].Text, Is.EqualTo("This is an instance of TestEntityTwo97"));
      var result980 = session.Query.All<PartOne.TestEntityOne98>().ToArray();
      Assert.That(result980.Length, Is.EqualTo(1));
      Assert.That(result980[0].BooleanField, Is.True);
      Assert.That(result980[0].Int16Field, Is.EqualTo(98));
      Assert.That(result980[0].Int32Field, Is.EqualTo(98));
      Assert.That(result980[0].Int64Field, Is.EqualTo(98));
      Assert.That(result980[0].FloatField, Is.EqualTo((float)98));
      Assert.That(result980[0].DoubleField, Is.EqualTo((double)98));
      Assert.That(result980[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result980[0].StringField, Is.EqualTo("TestEntityOne98"));
      Assert.That(result980[0].Text, Is.EqualTo("This is an instance of TestEntityOne98"));

      var result981 = session.Query.All<PartTwo.TestEntityTwo98>().ToArray();
      Assert.That(result981.Length, Is.EqualTo(1));
      Assert.That(result981[0].BooleanField, Is.True);
      Assert.That(result981[0].Int16Field, Is.EqualTo(98));
      Assert.That(result981[0].Int32Field, Is.EqualTo(98));
      Assert.That(result981[0].Int64Field, Is.EqualTo(98));
      Assert.That(result981[0].FloatField, Is.EqualTo((float)98));
      Assert.That(result981[0].DoubleField, Is.EqualTo((double)98));
      Assert.That(result981[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result981[0].StringField, Is.EqualTo("TestEntityTwo98"));
      Assert.That(result981[0].Text, Is.EqualTo("This is an instance of TestEntityTwo98"));
      var result990 = session.Query.All<PartOne.TestEntityOne99>().ToArray();
      Assert.That(result990.Length, Is.EqualTo(1));
      Assert.That(result990[0].BooleanField, Is.True);
      Assert.That(result990[0].Int16Field, Is.EqualTo(99));
      Assert.That(result990[0].Int32Field, Is.EqualTo(99));
      Assert.That(result990[0].Int64Field, Is.EqualTo(99));
      Assert.That(result990[0].FloatField, Is.EqualTo((float)99));
      Assert.That(result990[0].DoubleField, Is.EqualTo((double)99));
      Assert.That(result990[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result990[0].StringField, Is.EqualTo("TestEntityOne99"));
      Assert.That(result990[0].Text, Is.EqualTo("This is an instance of TestEntityOne99"));

      var result991 = session.Query.All<PartTwo.TestEntityTwo99>().ToArray();
      Assert.That(result991.Length, Is.EqualTo(1));
      Assert.That(result991[0].BooleanField, Is.True);
      Assert.That(result991[0].Int16Field, Is.EqualTo(99));
      Assert.That(result991[0].Int32Field, Is.EqualTo(99));
      Assert.That(result991[0].Int64Field, Is.EqualTo(99));
      Assert.That(result991[0].FloatField, Is.EqualTo((float)99));
      Assert.That(result991[0].DoubleField, Is.EqualTo((double)99));
      Assert.That(result991[0].DateTimeField, Is.EqualTo(DateTime.Now.Date));
      Assert.That(result991[0].StringField, Is.EqualTo("TestEntityTwo99"));
      Assert.That(result991[0].Text, Is.EqualTo("This is an instance of TestEntityTwo99"));
    }
  }
}