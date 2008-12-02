// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26


using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Bug0014_Model;

namespace Xtensive.Storage.Tests.Bug0014_Model
{
  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(PairTo = "Friends")]
    public EntitySet<Person> Friends { get; set; }
  }
}
namespace Xtensive.Storage.Tests.BugReports
{
  public class Bug0014_SymmetricRelationshipIsUnsupported : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        Person person1, friend1, friend2;
        using (var t = Transaction.Open()) {
          person1 = new Person {Name = "Person1"};
          friend1 = new Person {Name = "Friend1"};
          friend2 = new Person {Name = "Friend2"};

          person1.Friends.Add(friend1);
          person1.Friends.Add(friend2);
          
          //Session.Current.Persist();
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          Assert.AreEqual(2,person1.Friends.Count);
        }
      }
    }

    private void Fill()
    {
      
    }
  }
}