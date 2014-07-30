// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.07.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0542_OfTypeTranslationBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0542_OfTypeTranslationBugModel
{
  [HierarchyRoot]
  public class Test : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public StructType Struct { get; set; }
  }

  [HierarchyRoot]
  public class Test2 : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    [Association(PairTo = "Struct.Item")]
    public EntitySet<Test> StructSet { get; set; }
  }

  public class StructType : Structure
  {
    [Field]
    public Test2 Item { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0542_OfTypeTranslationBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();

      configuration.Types.Register(typeof(Test));
      configuration.Types.Register(typeof(Test2));

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      var sessionConfiguration = new SessionConfiguration(SessionOptions.AutoActivation);

      using (var domain = Domain.Build(configuration)) {
        using (var session = domain.OpenSession(sessionConfiguration))
        using (var t = session.OpenTransaction()) {
          var test2 = new Test2();
          var test = new Test {
            Name = "",
            Struct = new StructType { Item = test2 }
          };
          t.Complete();
        }

        using (var session = domain.OpenSession(sessionConfiguration))
        using (var t = session.OpenTransaction()) {
          var test2 = Session.Current.Query.All<Test2>().SingleOrDefault();
          var result2 = test2.StructSet.OfType<Test>().ToList();
        }
      }
    }

  }
}
