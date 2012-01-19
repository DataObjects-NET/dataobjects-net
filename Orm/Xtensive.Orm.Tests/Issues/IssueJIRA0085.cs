// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.04.28

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJIRA0085_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJIRA0085_Model
{
  [Serializable]
  [HierarchyRoot]
  public abstract class SomeEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Text { get; set; }

    protected SomeEntity(Session session) : base(session)
    {}
  }

  [Serializable]
  public class FirstEntity : SomeEntity
  {
    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<FirstChild> entSet { get; set; }

    public FirstEntity(Session session) : base(session)
    {}
  }

  [Serializable]
  public class SecondEntity : SomeEntity
  {
    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<SecondChild> entSet { get; set; }

    public SecondEntity(Session session) : base(session)
    {}
  }

  [Serializable]
  public class MyEntity<TEntity> : Entity where TEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Text { get; set; }

    [Field]
    public TEntity Owner { get; set; }

    public MyEntity(Session session, TEntity entity)
      : base(session)
    {
      Owner = entity;
    }

    public MyEntity(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class NextEntity : MyEntity<SomeEntity>
  {
    public NextEntity(Session session) : base(session)
    {}

    protected NextEntity(Session session, SomeEntity entity) : base(session, entity)
    {}
  }

  [Serializable]
  public class FirstChild : NextEntity
  {
    public FirstChild(Session session, SomeEntity entity) : base(session, entity)
    {}

    public FirstChild(Session session) : base(session)
    {}
  }

  [Serializable]
  public class SecondChild : NextEntity
  {
    public SecondChild(Session session)
      : base(session)
    {}

    public SecondChild(Session session, SomeEntity entity) : base(session, entity)
    {}
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJIRA0085 : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      //config.ForeignKeyMode = ForeignKeyMode.None;
      config.Types.Register(typeof (SecondChild).Assembly, typeof (SecondChild).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope transactionScope = session.OpenTransaction()) {
          // Creating new persistent object
          var first = new FirstEntity(session)
                        {
                          Text = "first!"
                        };

          // Creating new persistent object
          var second = new SecondEntity(session)
                         {
                           Text = "second!"
                         };

          new FirstChild(session, first) {Text = "First"};
          new SecondChild(session, second) {Text = "Second"};

          // Committing transaction
          transactionScope.Complete();
        }
      }

      using (Session session = Domain.OpenSession()) {
        using (TransactionScope transactionScope = session.OpenTransaction()) {
          Console.WriteLine(session.Query.All<SecondChild>().First().Text);
          Console.WriteLine(session.Query.All<FirstChild>().First().Text);
        }
      }
    }
  }
}