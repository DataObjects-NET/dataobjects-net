using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.InterfaceEntitySetTest_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace InterfaceEntitySetTest_Model
  {
    public interface IHasTraceFields : IEntity 
    {
      [Key]
      [Field]
      long ID { get; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear, PairTo = "Owner")]
      EntitySet<TraceField> TraceFields { get; }
    }
  
    [HierarchyRoot]
    public class TraceField: Entity 
    {
      [Key]
      [Field]
      public long ID { get; private set;  }

      [Field]
      public IHasTraceFields Owner { get; set; }
    }

    [HierarchyRoot]
    public class Pallet: Entity, IHasTraceFields {
        
      public long ID { get; private set;  }

      [Field]
      public EntitySet<TraceField> TraceFields { get; private set; }
    }

    public class WestfroPallet: Pallet {
      public WestfroPallet()
      {
        Console.WriteLine(TraceFields.Count);
      }
    } 
  }

  public class InterfaceEntitySetTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Pallet).Assembly, typeof (Pallet).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var pallet = new WestfroPallet();
        t.Complete();
      }
    }
  }
}