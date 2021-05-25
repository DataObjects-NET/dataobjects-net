// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.12.23

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0559_EntitySetQueryError_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0559_EntitySetQueryError_Model
  {
    [Serializable]
    [HierarchyRoot]
    [Index("Name")]
    public class Topic : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field(Length = 25)]
      public string Name { get; set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Subscription> Subscriptions { get; private set; }
    }

    [Serializable]
    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    [Index("ApplicationName")]
    public class Subscription : Entity
    {
      [Field, Key(0)]
      public long Id { get; private set; }

      [Field, Key(1)]
      public long LastEventId { get; private set; }

      [Field(Length = 25)]
      public string ApplicationName { get; set; }

      [Field]
      public bool Persistent { get; set; }

      public Subscription(long id, long lastEventId)
        : base(id, lastEventId)
      {
      }
    }
  }

  [TestFixture]
  public class Issue0559_EntitySetQueryError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Subscription).Assembly, typeof(Subscription).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var topicName = "Name";
        var ApplicationName = "Name";
        var defaultTopic = new Topic() { Name = topicName };
        var subscription = new Subscription(1, 1) {ApplicationName = ApplicationName};
        _ = defaultTopic.Subscriptions.Add(subscription);

        var subTopic = (from topic in session.Query.All<Topic>() where topic.Name == topicName select topic).SingleOrDefault<Topic>();
        var subscriptions = subTopic.Subscriptions;
        var result = from sub in subscriptions where sub.ApplicationName == ApplicationName select sub;
        var list = result.ToList();

        t.Complete();
      }
    }
  }
}