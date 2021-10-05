// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.03

using System.Collections.Generic;

using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels
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

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }

      [Field]
      public int? DescendantValueField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string BaseClassField { get; set; }

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }

      [Field]
      public int? DescendantValueField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string BaseClassField { get; set; }

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string DescendantClassField { get; set; }

      [Field]
      public int? DescendantValueField { get; set; }
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

    public class AnotherSingleTableDescentant : SingleTableHierarchyBase
    {
      [Field]
      public int SomeValue { get; set; }
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

    public class AnotherClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public int SomeValue { get; set; }
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

    public class AnotherConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
      public int SomeValue { get; set; }
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
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SingleTableHBField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeDescendantField { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.SingleTableHierarchyBase", "SingleTableRemovableField"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.ConcreteTableHierarchyBase", "ConcreteTableRemovableField"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.ClassTableHierarchyBase", "ClassTableRemovableField"));

          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.SingleTableDescendant", "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.ConcreteTableDescendant", "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveFieldModel.ClassTableDescendant", "SomeDescendantRemovableField"));
        }
      }
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

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.SingleTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.SingleTableDescendant22Removed"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.SingleTableDescendant12Removed", "SingleTableD12Field"));
          hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.SingleTableDescendant22Removed", "SingleTableD22Field"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.ClassTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.ClassTableDescendant22Removed"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.ConcreteTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RemoveTypeModel.ConcreteTableDescendant22Removed"));
        }
      }
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
      public int FieldWithRightName { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithRightName { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }

      [Field]
      public int FieldWithRightName { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithRightName { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ConcreteTableHBField { get; set; }

      [Field]
      public int FieldWithRightName { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
      public double AnotherFieldWithRightName { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameFieldHint(typeof (SingleTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (SingleTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));

          hints.Add(new RenameFieldHint(typeof (ConcreteTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (ConcreteTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));

          hints.Add(new RenameFieldHint(typeof (ClassTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (ClassTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));
        }
      }
    }
  }

  namespace RenameTypeModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
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

    public class ClassTableDescendant : ClassTableHierarchyBase
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

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field(Length = 50)]
      public string SomeStringField { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameTypeModel.SingleTableDescendant1", typeof(SingleTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameTypeModel.ClassTableDescendant1", typeof(ClassTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.RenameTypeModel.ConcreteTableDescendant1", typeof(ConcreteTableDescendant)));
        }
      }
    }
  }

  namespace MoveFieldToLastDescendantModel
  {
    [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }
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

      [Field(Length = 50)]
      public string MovableField { get; set; }
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
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }
    }

    public class ConcreteTableDescendant2 : ConcreteTableDescendant1
    {
      [Field]
      public float SomeField { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string SingleTableHBField { get; set; }
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
      [Field]
      public float SomeField { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.ClassTableHierarchyBase", "MovableField", typeof(ClassTableDescendant2)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.ConcreteTableHierarchyBase", "MovableField", typeof(ConcreteTableDescendant2)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.SingleTableHierarchyBase", "MovableField", typeof(SingleTableDescendant2)));
        }
      }
    }
  }

  namespace MoveFieldToFirstDescendantModel
  {
    [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Length = 50)]
      public string ClassTableHBField { get; set; }
    }

    public class ClassTableDescendant1 : ClassTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
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
    }

    public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
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
    }

    public class SingleTableDescendant1 : SingleTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field(Length = 50)]
      public string Comment { get; set; }

      [Field(Length = 50)]
      public string MovableField { get; set; }
    }

    public class SingleTableDescendant2 : SingleTableDescendant1
    {
      [Field]
      public float SomeField { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.ClassTableHierarchyBase", "MovableField", typeof(ClassTableDescendant1)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.ConcreteTableHierarchyBase", "MovableField", typeof(ConcreteTableDescendant1)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels.MoveFieldModel.SingleTableHierarchyBase", "MovableField", typeof(SingleTableDescendant1)));
        }
      }
    }
  }
}
