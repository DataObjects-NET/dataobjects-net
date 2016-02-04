using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

      [Field]
      public string BaseClassField { get; set; }

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
      public string DescendantClassField { get; set; }

      [Field]
      public int? DescendantValueField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string BaseClassField { get; set; }

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public string DescendantClassField { get; set; }

      [Field]
      public int? DescendantValueField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string BaseClassField { get; set; }

      [Field]
      public int? BaseClassValueField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
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
      [Field]
      public int Id { get; set; }

      [Field]
      public string SomeKindOfText { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
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
      [Field]
      public int Id { get; set; }

      [Field]
      public string SomeKindOfText { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
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
      [Field]
      public int Id { get; set; }

      [Field]
      public string SomeKindOfText { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
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

      [Field]
      public string ClassTableHBField { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public double SomeDescendantField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableDescendant : ConcreteTableHierarchyBase
    {
      [Field]
      public double SomeDescendantField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field]
      public int Id { get; set; }

      [Field]
      public string SingleTableHBField { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
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

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveFieldHint(typeof (SingleTableHierarchyBase), "SingleTableRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ConcreteTableHierarchyBase), "ConcreteTableRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ClassTableHierarchyBase), "ClassTableRemovableField"));

          hints.Add(new RemoveFieldHint(typeof (SingleTableDescendant), "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ConcreteTableDescendant), "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ClassTableDescendant), "SomeDescendantRemovableField"));
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

      [Field]
      public string SinlgeTableHBField { get; set; }
    }

    public class SingleTableDescendant1 : SingleTableHierarchyBase
    {
      [Field]
      public string SingleTableD1Field { get; set; }
    }

    public class SingleTableDescendant11 : SingleTableDescendant1
    {
      [Field]
      public string SingleTableD11Field { get; set; }
    }

    public class SingleTableDescendant2 : SingleTableHierarchyBase
    {
      [Field]
      public string SingleTableD2Field { get; set; }
    }

    public class SingleTableDescendant21 : SingleTableDescendant2
    {
      [Field]
      public string SingleTableD21Field { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ClassTableHRField { get; set; }
    }

    public class ClassTableDescendant1 : ClassTableHierarchyBase
    {
      [Field]
      public string ClassTableD1Field { get; set; }
    }

    public class ClassTableDescendant11 : ClassTableDescendant1
    {
      [Field]
      public string ClassTableD11Field { get; set; }
    }

    public class ClassTableDescendant2 : ClassTableHierarchyBase
    {
      [Field]
      public string ClassTableD2Field { get; set; }
    }

    public class ClassTableDescendant21 : ClassTableDescendant2
    {
      [Field]
      public string ClassTableD21Field { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field]
      public int Id { get; set; }

      [Field]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
    {
      [Field]
      public string ConcreteTableD1Field { get; set; }
    }

    public class ConcreteTableDescendant11 : ConcreteTableDescendant1
    {
      [Field]
      public string ConcreteTableD11Field { get; set; }
    }

    public class ConcreteTableDescendant2 : ConcreteTableHierarchyBase
    {
      [Field]
      public string ConcreteTableD2Field { get; set; }
    }

    public class ConcreteTableDescendant21 : ConcreteTableDescendant2
    {
      [Field]
      public string ConcreteTableD21 { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.SingleTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.SingleTableDescendant22Removed"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.ClassTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.ClassTableDescendant22Removed"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.ConcreteTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RemoveTypeModel.ConcreteTableDescendant22Removed"));
        }
      }
    }
  }

  namespace RenameFieldModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field]
      public int Id { get; set; }

      [Field]
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
      [Field]
      public int Id { get; set; }

      [Field]
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
      [Field]
      public int Id { get; set; }

      [Field]
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

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
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
      [Field]
      public int Id { get; set; }
    }

    public class SingleTableDescendant : SingleTableHierarchyBase
    {
      [Field]
      public string SomeStringField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class ClassTableHierarchyBase : Entity
    {
      [Field]
      public int Id { get; set; }
    }

    public class ClassTableDescendant : ClassTableHierarchyBase
    {
      [Field]
      public string SomeStringField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field]
      public int Id { get; set; }
    }

    public class ConcreteTableDescendant : Entity
    {
      [Field]
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

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RenameTypeModel.SingleTableDescendant1", typeof (SingleTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RenameTypeModel.ClassTableDescendant1", typeof (ClassTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.RenameTypeModel.ConcreteTableDescendant1", typeof (ConcreteTableDescendant)));
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

      [Field]
      public string ClassTableHBField { get; set; }
    }

    public class ClassTableAncestor1 : ClassTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }
    }

    public class ClassTableAncestor2 : ClassTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }

      [Field]
      public string MovableField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableAncestor1 : ConcreteTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }
    }

    public class ConcreteTableAncestor2 : ConcreteTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }

      [Field]
      public string MovableField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string SingleTableHBField { get; set; }
    }

    public class SingleTableAncestor1 : SingleTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }
    }

    public class SingleTableAncestor2 : SingleTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }

      [Field]
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

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.ClassTableHierarchyBase", "MovableField", typeof(ClassTableAncestor2)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.ConcreteTableHierarchyBase", "MovableField", typeof(ConcreteTableAncestor2)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.SingleTableHierarchyBase", "MovableField", typeof(SingleTableAncestor2)));
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

      [Field]
      public string ClassTableHBField { get; set; }
    }

    public class ClassTableAncestor1 : ClassTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }

      [Field]
      public string MovableField { get; set; }
    }

    public class ClassTableAncestor2 : ClassTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }

    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class ConcreteTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string ConcreteTableHBField { get; set; }
    }

    public class ConcreteTableAncestor1 : ConcreteTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }

      [Field]
      public string MovableField { get; set; }
    }

    public class ConcreteTableAncestor2 : ConcreteTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class SingleTableHierarchyBase : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string SingleTableHBField { get; set; }
    }

    public class SingleTableAncestor1 : SingleTableHierarchyBase
    {
      [Field]
      public int Value { get; set; }

      [Field]
      public string Comment { get; set; }

      [Field]
      public string MovableField { get; set; }
    }

    public class SingleTableAncestor2 : SingleTableAncestor1
    {
      [Field]
      public float SomeField { get; set; }
    }

    namespace UpgradeHandlers
    {
      public class FirstCustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.ClassTableHierarchyBase", "MovableField", typeof(ClassTableAncestor1)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.ConcreteTableHierarchyBase", "MovableField", typeof(ConcreteTableAncestor1)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.UpgradeAndNamingRulesAdvanced.Sources.MoveFieldModel.SingleTableHierarchyBase", "MovableField", typeof(SingleTableAncestor1)));
        }
      }
    }
  }
}
