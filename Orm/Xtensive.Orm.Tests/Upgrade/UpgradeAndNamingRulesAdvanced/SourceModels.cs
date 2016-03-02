// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.03

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels
{
  namespace AddNewFieldModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string BaseClassField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string BaseClassField { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string BaseClassField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }
    }
  }

  namespace AddNewTypeModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SomeKindOfText { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SomeKindOfText { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    [HierarchyRoot]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SomeKindOfText { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string Comment { get; set; }
    }
  }

  namespace RemoveFieldModel
  {
    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }

      [Field(Length = 50)]
      public string ClassTableRemovableField { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }

      [Field(Length = 50)]
      public string SomeDescendantRemovableField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableHBField { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableRemovableField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }

      [Field(Length = 50)]
      public string SomeDescendantRemovableField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SingleTableHBField { get; set; }

      [Field(Length = 50)]
      public string SingleTableRemovableField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }

      [Field(Length = 50)]
      public string SomeDescendantRemovableField { get; set; }
    }
  }

  namespace RemoveTypeModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SinlgeTableHBField { get; set; }
    }

    public class SingleTableDescendant1 : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SingleTableD1Field { get; set; }
    }

    public class SingleTableDescendant11 : SingleTableDescendant1
    {
      [Field(Length = 50)]
      public string SingleTableD11Field { get; set; }
    }

    public class SingleTableDescendant12Removed : SingleTableDescendant1
    {
      [Field(Length = 50)]
      public string SingleTableD12Field { get; set; }
    }

    public class SingleTableDescendant2 : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SingleTableD2Field { get; set; }
    }

    public class SingleTableDescendant21 : SingleTableDescendant2
    {
      [Field(Length = 50)]
      public string SingleTableD21Field { get; set; }
    }

    public class SingleTableDescendant22Removed : SingleTableDescendant2
    {
      [Field(Length = 50)]
      public string SingleTableD22Field { get; set; }
    }


    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }
    }

    public class ClassTableDescendant1 : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string ClassTableD1Field { get; set; }
    }

    public class ClassTableDescendant11 : ClassTableDescendant1
    {
      [Field(Length = 50)]
      public string ClassTableD11Field { get; set; }
    }

    public class ClassTableDescendant12Removed : ClassTableDescendant1
    {
      [Field(Length = 50)]
      public string ClassTableD12Field { get; set; }
    }

    public class ClassTableDescendant2 : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string ClassTableD2Field { get; set; }
    }

    public class ClassTableDescendant21 : ClassTableDescendant2
    {
      [Field(Length = 50)]
      public string ClassTableD21Field { get; set; }
    }

    public class ClassTableDescendant22Removed : ClassTableDescendant2
    {
      [Field(Length = 50)]
      public string ClassTableD22Field { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string ConcreteTableD1Field { get; set; }
    }

    public class ConcreteTableDescendant11 : ConcreteTableDescendant1
    {
      [Field(Length = 50)]
      public string ConcreteTableD11Field { get; set; }
    }

    public class ConcreteTableDescendant12Removed : ConcreteTableDescendant1
    {
      [Field(Length = 50)]
      public string ConcreteTableD12Field { get; set; }
    }

    public class ConcreteTableDescendant2 : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string ConcreteTableD2Field { get; set; }
    }

    public class ConcreteTableDescendant21 : ConcreteTableDescendant2
    {
      [Field(Length = 50)]
      public string ConcreteTableD21Field { get; set; }
    }

    public class ConcreteTableDescendant22Removed : ConcreteTableDescendant2
    {
      [Field(Length = 50)]
      public string ConcreteTableD22Field { get; set; }
    }
  }

  namespace RenameFieldModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SingleTableHBField { get; set; }

      [Field]
      public int FieldWithWrongName { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithWrongName { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ClassTableHBField { get; set; }

      [Field]
      public int FieldWithWrongName { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithWrongName { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ConcreteTableHBField { get; set; }

      [Field]
      public int FieldWithWrongName { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithWrongName { get; set; }
    }
  }

  namespace RenameTypeModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field , Key]
      public int Id { get; set; }
    }

    public class SingleTableDescendant1 : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeStringField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }
    }

    public class ClassTableDescendant1 : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeStringField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }
    }

    public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeStringField { get; set; }
    }
  }

  namespace MoveFieldModel
  {
    [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    public class ClassTableDescendant1 : ClassTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    public class ClassTableDescendant2 : ClassTableDescendant1
    {
      [Field]
      public float SomeField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableHBField { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    public class ConcreteTableDescendant2 : ConcreteTableDescendant1
    {
      [Field]
      public float SomeField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SingleTableHBField { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    public class SingleTableDescendant1 : SingleTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    public class SingleTableDescendant2 : SingleTableDescendant1
    {
      [Field(Length = 50)]
      public float SomeField { get; set; }
    }
  }
}
