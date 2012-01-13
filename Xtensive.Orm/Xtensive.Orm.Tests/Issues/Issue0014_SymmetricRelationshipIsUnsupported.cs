// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26


using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.Issue0014_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0014_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field, Association(PairTo = "Friends")]
    public EntitySet<Person> Friends { get; set; }

    [Field, Association(PairTo = "BestFriend")]
    public Person BestFriend { get; set;}
  }
}
namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0014_SymmetricRelationshipIsUnsupported : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Person).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
    }

    [Test]
    public void ManyToManyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Person first = new Person {Name = "First"};
          Person second = new Person {Name = "Second"};
          Person third = new Person {Name = "Third"};

          first.Friends.Add(second);
          first.Friends.Add(third);
          Assert.AreEqual(2,first.Friends.Count);
          Assert.AreEqual(1,second.Friends.Count);
          Assert.AreEqual(1,third.Friends.Count);
          Assert.AreSame(first, first.Friends.First().Friends.First());
          Assert.AreSame(first, first.Friends.Skip(1).First().Friends.First());

          first.Friends.Add(first);
          Assert.AreEqual(3,first.Friends.Count);

          first.Friends.Remove(first);
          Assert.AreEqual(2,first.Friends.Count);

          first.Friends.Clear();
          second.Friends.Clear();
          third.Friends.Clear();
          Session.Current.SaveChanges();

          first.Friends.Add(first);

          t.Complete();
        }
      }
    }


    [Test]
    public void OneToOneTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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