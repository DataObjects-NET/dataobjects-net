// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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

    [Field(Length = 250)]
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
      config.Types.Register(typeof(TextEntity).Assembly, typeof(TextEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {

          var itemCount = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)
            ? 64000 / 250 //64K limit of Firebird / 250 (which is lenght of field) :)
            : 800;

          var strings = new HashSet<string>();
          for (var i = 0; i < itemCount; i++) {
            _ = strings.Add(i.ToString());
          }

          var existing = s.Query.All<TextEntity>()
            .Where(i => i.Text.In(strings))
            .Select(i => i.Text);
          foreach (var text in strings.Except(existing)) {
            _ = new TextEntity { Text = text };
          }
          t.Complete();
          // Rollback
        }
      }
    }
  }
}