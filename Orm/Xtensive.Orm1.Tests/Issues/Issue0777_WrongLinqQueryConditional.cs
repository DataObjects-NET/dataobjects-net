// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.13

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0777_WrongLinqQueryConditional_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0777_WrongLinqQueryConditional_Model
  {
    [HierarchyRoot]
    public class BooleanHell : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTime ApocalypseDate { get; set; }

      [Field]
      public DateTime DefaultApocalypseDate { get; set; } 
    }
  }

  [Serializable]
  public class Issue0777_WrongLinqQueryConditional : AutoBuildTest
  {
    DateTime? apocalypseNow = DateTime.Now;
    DateTime? apocalypseNever = null;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BooleanHell));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new BooleanHell { ApocalypseDate = new DateTime(2012, 12, 12), DefaultApocalypseDate = new DateTime(2012, 12, 12)};
        session.SaveChanges();

        var firstHell = session.Query.All<BooleanHell>()
          .Where(hell => hell.ApocalypseDate >= (apocalypseNow.HasValue ? apocalypseNow.Value : hell.DefaultApocalypseDate))
          .ToList();
        var defaultHell = session.Query.All<BooleanHell>()
          .Where(hell => hell.ApocalypseDate >= (apocalypseNever.HasValue ? apocalypseNever.Value : hell.DefaultApocalypseDate))
          .ToList();

        t.Complete();
      }
    }
  }
}