// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.22

using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0551_IncorrectTypeIdFieldBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0551_IncorrectTypeIdFieldBugModel
{
  [HierarchyRoot(IncludeTypeId = true, Clustered = true, InheritanceSchema = InheritanceSchema.ClassTable)]
  public abstract class BaseEntity : Entity
  {
    [Field]
    [Key(Position = 0)]
    public int Id { get; private set; }
  }

  public class Student : BaseEntity
  {
    [Field()]
    public EntitySet<Course> Courses { get; private set; }
  }

  public class Course : BaseEntity
  {
    [Field]
    [Association(PairTo = "Courses", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Student> Students { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0551_IncorrectTypeIdFieldBug : AutoBuildTest
  {
    [Test]
    public void Test()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof (BaseEntity).Assembly, typeof (BaseEntity).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      using (var domain = Domain.Build(domainConfiguration))
      using (var session = domain.OpenSession()) {
      using (session.Activate())
      using (var trans = session.OpenTransaction()) {
          Student student1 = new Student();
          Course course1 = new Course();
          student1.Courses.Add(course1);
          trans.Complete();
        }
      }
    }
  }
}
