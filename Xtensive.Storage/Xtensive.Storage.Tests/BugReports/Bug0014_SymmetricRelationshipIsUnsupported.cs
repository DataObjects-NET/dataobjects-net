// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26


using System.Linq;
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

    [Field(PairTo = "BestFriend")]
    public Person BestFriend { get; set;}
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
    public void ManyToManyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Person first = new Person {Name = "First"};
          Person second = new Person {Name = "Second"};
          Person third = new Person {Name = "Third"};

          first.Friends.Add(second);
          first.Friends.Add(third);
          Assert.AreEqual(2,first.Friends.Count);
          Assert.AreSame(first, first.Friends.First().Friends.First());
          Assert.AreSame(first, first.Friends.Skip(1).First().Friends.First());

          first.Friends.Add(first);
          Assert.AreEqual(3,first.Friends.Count);

          first.Friends.Remove(first);
          Assert.AreEqual(2,first.Friends.Count);

          t.Complete();
        }
      }
    }


    [Test]
    public void OneToOneTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Person first = new Person {Name = "First"};
          Person second = new Person {Name = "Second"};
          Person third = new Person {Name = "Third"};

          first.BestFriend = second;
          Assert.AreSame(first.BestFriend, second);
          Assert.AreSame(first, second.BestFriend);

          first.BestFriend = third;
          Assert.AreSame(first.BestFriend, third);
          Assert.AreSame(first, third.BestFriend);
          Assert.AreSame(null, second.BestFriend);

          first.BestFriend = null;
          Assert.AreSame(null, first.BestFriend);
          Assert.AreSame(null, third.BestFriend);

          first.BestFriend = first;
          Assert.AreSame(first.BestFriend, first);

          first.BestFriend = null;
          Assert.AreSame(null, first.BestFriend);

          t.Complete();
        }
      }
    }
  }
}