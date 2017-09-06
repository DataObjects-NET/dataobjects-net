// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.09.06

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0708_StackOverflowOnDifferetialTupleMergeModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0708_StackOverflowOnDifferetialTupleMergeModel
{
  [HierarchyRoot]
  public class Ent : Entity
  {
    [Field, Key]
    public Guid Id { get; set; }

    [Field]
    public int Num { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture, Explicit("The test is really time-consuming")]
  public class IssueJira0708_StackOverflowOnDifferetialTupleMerge : AutoBuildTest
  {
    private int IterationsCount = 100000;

    [Test]
    public void MainTest()
    {
      var sw = new Stopwatch();
      using (var s = Domain.OpenSession(SessionConfiguration.Default))
      using (s.Activate())
      using (var t1 = s.OpenTransaction(TransactionOpenMode.New)) {
        sw.Start();

        var ent = new Ent();

        for (var i = 0; i < IterationsCount; i++) {
          Session.Current.Query.All<Ent>().Where(z => z.Id==ent.Id).ToArray();
          ent.Num = i;
          if (i % 100==0) {
            Console.WriteLine("{0}    {1}", i, sw.Elapsed.TotalSeconds);
            sw.Restart();
          }
        }
        sw.Stop();
        t1.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Ent).Assembly, typeof (Ent).Namespace);
      return configuration;
    }
  }
}
