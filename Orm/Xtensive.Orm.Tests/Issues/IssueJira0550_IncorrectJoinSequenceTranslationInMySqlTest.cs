// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.01

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0550_IncorrectJoinSequenceTranslationInMySqlTestModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0550_IncorrectJoinSequenceTranslationInMySqlTestModel
{
  public interface IHuman : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field(Length = 50)]
    string Name { get; set; }

    [Field(Length = 50)]
    string Surname { get; set; }

    [Field]
    DateTime Birthday { get; set; }
  }

  [HierarchyRoot]
  [Index("Surname", Unique = true)]
  public abstract class Human : Entity, IHuman
  {
    public int Id { get; private set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public DateTime Birthday { get; set; }
  }

  public class Teacher : Worker
  {
    [Field]
    public EntitySet<Discepline> Disceplines { get; set; }
  }

  public class Worker : Human
  {
    [Field]
    public string Field { get; set; }

    [Field]
    public WorkerContract Contract { get; set; }
  }

  [HierarchyRoot]
  public abstract class Contract : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    public int Number { get; set; }

    [Field(Nullable = false)]
    public DateTime StartsWith { get; set; }

    [Field]
    public DateTime EndsWith { get; set; }
  }
  
  public class WorkerContract : Contract
  {
    [Field]
    [Association(PairTo = "Contract")]
    public EntitySet<Teacher> Workers { get; set; }
  }

  [HierarchyRoot]
  public class Discepline : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Disceplines")]
    public EntitySet<Teacher> Teachers { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0550_IncorrectJoinSequenceTranslationInMySqlTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var juliaTeacher = session.Query.All<Teacher>().Single(el => el.Name=="Julia");
        var discipline = session.Query.All<Discepline>().Single(el => el.Name=="German");
        Assert.DoesNotThrow(() => juliaTeacher.Disceplines.Remove(discipline));
        transaction.Complete();
      }
    }

    protected override DomainConfiguration  BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Worker).Assembly, typeof (Worker).Namespace);
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.MySql);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var englishTeacher = new Teacher {
          Name = "Julia",
          Surname = "Ololo",
          Birthday = new DateTime(1987, 06, 18),
          Contract = new WorkerContract { Number = 2, StartsWith = new DateTime(2012, 10, 20) },
        };
        var english = new Discepline { Name = "English" };
        var german = new Discepline { Name = "German" };
        englishTeacher.Disceplines.AddRange(new[] { english, german });
        transaction.Complete();
      }
    }
  }
}
