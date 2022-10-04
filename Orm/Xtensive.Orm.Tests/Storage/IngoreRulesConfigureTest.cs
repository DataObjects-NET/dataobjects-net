// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IngoreRulesConfigureTest
  {
    [Test]
    public void NoColumnOrTableIgnoredTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.IgnoreRules.Add(new IgnoreRule());

      _ = Assert.Throws<InvalidOperationException>(() => configuration.Lock());
    }

    [Test]
    public void CheckColumnOrIndexDeclaredTest()
    {
      const string SomeDB = "SomeDB";
      const string SomeSchema = "SomeSchema";
      const string SomeTable = "SomeTable";
      const string SomeColumn = "SomeColumn";
      const string SomeIndex = "SomeIndex";

      var invalidFactories = new TestDelegate[] {
        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule("", SomeSchema, SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, "", SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", "", SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, "", "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", SomeSchema, "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", "", "", SomeColumn, SomeIndex)
      };

      foreach (var func in invalidFactories) {
        _ = Assert.Throws<InvalidOperationException>(func);
      }

      _ = Assert.Throws<InvalidOperationException>(
        () => new IgnoreRule() { Table = SomeTable, Column = SomeColumn, Index = SomeIndex, });
      _ = Assert.Throws<InvalidOperationException>(
        () => new IgnoreRule() { Table = SomeTable, Index = SomeIndex, Column = SomeColumn, });
    }
  }
}
