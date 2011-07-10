// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.07.10

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.LocalCollectionsTest_Model;

namespace Xtensive.Orm.Tests.Issues.LocalCollectionsTest_Model
{
  [HierarchyRoot]
  [Index("Text", Unique = true)]
  public class TextEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class LocalCollectionsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TextEntity).Assembly, typeof (TextEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {

          var strings = new HashSet<string>();
          for (int i = 0; i < 800; i++) {
            strings.Add(i.ToString());
          }

          var existing = s.Query.All<TextEntity>()
            .Where(i => i.Text.In(strings))
            .Select(i => i.Text);
          foreach (var text in strings.Except(existing)) {
            new TextEntity {Text = text};
          }
          t.Complete();
          // Rollback
        }
      }
    }
  }
}