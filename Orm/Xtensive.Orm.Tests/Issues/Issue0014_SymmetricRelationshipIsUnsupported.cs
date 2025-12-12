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
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(Person).Namespace);
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
          Assert.That(first.Friends.Count, Is.EqualTo(2));
          Assert.That(second.Friends.Count, Is.EqualTo(1));
          Assert.That(third.Friends.Count, Is.EqualTo(1));
          Assert.That(first.Friends.First().Friends.First(), Is.SameAs(first));
          Assert.That(first.Friends.Skip(1).First().Friends.First(), Is.SameAs(first));

          first.Friends.Add(first);
          Assert.That(first.Friends.Count, Is.EqualTo(3));

          first.Friends.Remove(first);
          Assert.That(first.Friends.Count, Is.EqualTo(2));

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
          Assert.That(second, Is.SameAs(first.BestFriend));
          Assert.That(second.BestFriend, Is.SameAs(first));

          first.BestFriend = third;
          Assert.That(third, Is.SameAs(first.BestFriend));
          Assert.That(third.BestFriend, Is.SameAs(first));
          Assert.That(second.BestFriend, Is.SameAs(null));

          first.BestFriend = null;
          Assert.That(first.BestFriend, Is.SameAs(null));
          Assert.That(third.BestFriend, Is.SameAs(null));

          first.BestFriend = first;
          Assert.That(first, Is.SameAs(first.BestFriend));

          first.BestFriend = null;
          Assert.That(first.BestFriend, Is.SameAs(null));

          t.Complete();
        }
      }
    }
  }
}