// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.08.21

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue_OfTypeMissesImplementors_Model;

namespace Xtensive.Orm.Tests.Issues.Issue_OfTypeMissesImplementors_Model
{
  public interface IHasImplementors : IEntity
  {
    
  }

  [HierarchyRoot]
  public abstract class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Branch1 : Root, IHasImplementors
  {
    [Field]
    public string Name1 { get; set; }
  }

  public abstract class Branch2 : Root, IHasImplementors
  {
    [Field]
    public string Name1 { get; set; }
    
  }

  public class Node1 : Branch2
  {
    [Field]
    public string Name2 { get; set; }
   
  }

  public class Node2 : Branch2
  {
    [Field]
    public string Name2 { get; set; }
  }
  
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public IHasImplementors Reference { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue_OfTypeMissesImplementors : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Root).Assembly, typeof (Root).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          
          var c = new Container();
          new Branch1();
          new Node1();
          c.Reference = new Node2();

          Session.Current.SaveChanges();

          var items = Query.All<Root>().Where(e => e is IHasImplementors).ToList();
          Assert.AreEqual(3, items.Count);
          
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          
          var c = Query.All<Container>().First();
          Assert.IsNotNull(c.Reference);
          // Rollback
        }
      }
    }
  }
}