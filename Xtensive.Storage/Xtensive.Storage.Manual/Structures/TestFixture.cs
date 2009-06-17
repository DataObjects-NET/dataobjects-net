// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.16

using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.Structures
{
  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }
  }

  [HierarchyRoot]
  public class Range : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Point Left { get; set; }

    [Field]
    public Point Right { get; set; }
  }
}

namespace Xtensive.Storage.Manual.Structures
{
  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("mssql2005://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Range).Assembly, typeof (Range).Namespace);
      var domain = Domain.Build(config);

      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          // Rollback
        }
      }
    }
  }
}