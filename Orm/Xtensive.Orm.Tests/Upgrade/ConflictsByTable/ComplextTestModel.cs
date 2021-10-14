// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel
{
  namespace Modifiers
  {
    public class MakeClassTable : IModule
    {
      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        var hierarchy = model.Hierarchies[3];
        hierarchy.Schema = InheritanceSchema.ClassTable;
      }

      public void OnBuilt(Domain domain) { }
    }

    public class MakeConcreteTable : IModule
    {
      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        var hierarchy = model.Hierarchies[3];
        hierarchy.Schema = InheritanceSchema.ConcreteTable;
      }

      public void OnBuilt(Domain domain) { }
    }
  }

  namespace Original
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf2 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf3 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf3(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf4 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf4(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf2 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf2> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf3 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf3> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf4 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf4> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case1
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf2 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf3 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf3(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf2 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf2> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf3 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf3> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case2
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf2 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf2(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf3")]
    public class Leaf31 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf31(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf2 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf2> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf31 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf31> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case3
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf2")]
    public class Leaf21 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf3")]
    public class Leaf31 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf31(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf21 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf21> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Leaf31 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf31> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case4
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf1")]
    public class Leaf11 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf11(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf2")]
    public class Leaf21 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf3")]
    public class Leaf31 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf31(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf11 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf11> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf21 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf21> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf31 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf31> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case5
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Middle2")]
    public class Middle21 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf1")]
    public class Leaf11 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf11(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf2")]
    public class Leaf21 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf3")]
    public class Leaf31 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf31(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle21 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle21> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf11 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf11> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf21 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf21> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf31 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf31> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case6
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    [TableMapping("Middle1")]
    public class Middle11 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle11(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Middle2")]
    public class Middle21 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf1")]
    public class Leaf11 : Middle11
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf11(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf2")]
    public class Leaf21 : Middle11
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf21(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf3")]
    public class Leaf31 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf31(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Leaf4")]
    public class Leaf41 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf41(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle11 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle11> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle21 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle21> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf11 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf11> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf21 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf21> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf31 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf31> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf41 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf41> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case7
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    public class Middle1 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle1(Session session, string name)
        : base(session, name)
      {
      }
    }

    [TableMapping("Middle2")]
    public class Middle21 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle21(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf2 : Middle1
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf3 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf3(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf4 : Middle21
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf4(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle1 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle1> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle21 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle21> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf2 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf2> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf3 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf3> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf4 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf4> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Case8
  {
    // Will be rewritten
    [HierarchyRoot(InheritanceSchema.Default)]
    public class Root : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 12, Nullable = false)]
      public string Name { get; private set; }

      public Root(Session session, string name)
        : base(session)
      {
        Name = name;
      }
    }

    [TableMapping("Middle1")]
    public class Middle11 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle11(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Middle2 : Root
    {
      [Field]
      public string MiddleValue { get; set; }

      public Middle2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf1 : Middle11
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf1(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf2 : Middle11
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf2(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf3 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf3(Session session, string name)
        : base(session, name)
      {
      }
    }

    public class Leaf4 : Middle2
    {
      [Field]
      public string LeafValue { get; set; }

      public Leaf4(Session session, string name)
        : base(session, name)
      {
      }
    }

    [HierarchyRoot]
    public class RootRef : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Root RefRoot { get; set; }

      [Field]
      public EntitySet<Root> RootItems { get; private set; }

      public RootRef(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle11 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle11> MiddleItems { get; private set; }

      public MiddleRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class MiddleRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Middle2 RefMiddle { get; set; }

      [Field]
      public EntitySet<Middle2> MiddleItems { get; private set; }

      public MiddleRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf1 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf1> LeafItems { get; private set; }

      public LeafRef1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf2 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf2> LeafItems { get; private set; }

      public LeafRef2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf3 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf3> LeafItems { get; private set; }

      public LeafRef3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LeafRef4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Leaf4 RefLeaf { get; set; }

      [Field]
      public EntitySet<Leaf4> LeafItems { get; private set; }

      public LeafRef4(Session session)
        : base(session)
      {
      }
    }
  }
}