// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.05.21

using System;
using NUnit.Framework;
using model1 = Xtensive.Orm.Tests.Issues.IssueJira0529_BugWithRecycledNestedV1;
using model2 = Xtensive.Orm.Tests.Issues.IssueJira0529_BugWithRecycledNestedV2;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Issues.IssueJira0529_BugWithRecycledNestedV1
{
  public class Foo
  {
    public class Foo2
    {
      [HierarchyRoot]
      public class Bar : Entity
      {
        [Field]
        [Key]
        public Guid Id { get; set; }
      }
    }
  }

  namespace Recycled
  {
    public class Foo
    {
      public class Foo2
      {
        [HierarchyRoot]
        [Recycled]
        public class Bar : Entity
        {
          [Field]
          [Key]
          public Guid Id { get; set; }
        }
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0529_BugWithRecycledNestedV2
{
  public class Foo
  {
    public class Foo2
    {
      [HierarchyRoot]
      [Recycled]
      public class Bar : Entity
      {
        [Field]
        [Key]
        public Guid Id { get; set; }
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0531_IncorrectNameOfRecycledNestedType : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var dc = DomainConfigurationFactory.Create();
      dc.Types.Register(typeof(model1.Foo.Foo2.Bar));
      dc.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(dc)) { }

      dc = DomainConfigurationFactory.Create();
      dc.Types.Register(typeof (model1.Recycled.Foo.Foo2.Bar));
      dc.UpgradeMode = DomainUpgradeMode.PerformSafely;

      using (var domain = Domain.Build(dc)) { }
    }
  }
}


