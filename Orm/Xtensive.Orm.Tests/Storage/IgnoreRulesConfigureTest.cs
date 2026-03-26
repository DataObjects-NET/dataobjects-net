// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesConfigureTest
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

      var invalidCases = new TestDelegate[] {
        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule("", SomeSchema, SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, "", SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", "", SomeTable, SomeColumn, SomeIndex),
        () => new IgnoreRule(SomeDB, "", "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", SomeSchema, "", SomeColumn, SomeIndex),
        () => new IgnoreRule("", "", "", SomeColumn, SomeIndex),

        () => new IgnoreRule() { Table = SomeTable, Column = SomeColumn, Index = SomeIndex },
        () => new IgnoreRule() { Table = SomeTable, Index = SomeIndex, Column = SomeColumn }
      };

      foreach (var func in invalidCases) {
        _ = Assert.Throws<InvalidOperationException>(func);
      }
    }

    [Test]
    public void ValidRules()
    {
      const string SomeDB = "SomeDB";
      const string SomeSchema = "SomeSchema";
      const string SomeTable = "SomeTable";
      const string SomeColumn = "SomeColumn";
      const string SomeIndex = "SomeIndex";

      var validCases = new TestDelegate[] {
        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, null, SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, SomeColumn, null),

        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, "", SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, SomeTable, SomeColumn, ""),

        () => new IgnoreRule("", SomeSchema, SomeTable, "", SomeIndex),
        () => new IgnoreRule(SomeDB, "", SomeTable, "", SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, "", "", SomeIndex),
        () => new IgnoreRule("", "", SomeTable, "", SomeIndex),
        () => new IgnoreRule(SomeDB, "", "", "", SomeIndex),
        () => new IgnoreRule("", SomeSchema, "", "", SomeIndex),
        () => new IgnoreRule("", "", "", "", SomeIndex),

        () => new IgnoreRule(null, SomeSchema, SomeTable, null, SomeIndex),
        () => new IgnoreRule(SomeDB, null, SomeTable, null, SomeIndex),
        () => new IgnoreRule(SomeDB, SomeSchema, null, null, SomeIndex),
        () => new IgnoreRule(null, null, SomeTable, null, SomeIndex),
        () => new IgnoreRule(SomeDB, null, null, null, SomeIndex),
        () => new IgnoreRule(null, SomeSchema, null, null, SomeIndex),
        () => new IgnoreRule(null, null, null, null, SomeIndex),

        () => new IgnoreRule("", SomeSchema, SomeTable, SomeTable, ""),
        () => new IgnoreRule(SomeDB, "", SomeTable, SomeTable, ""),
        () => new IgnoreRule(SomeDB, SomeSchema, "", SomeTable, ""),
        () => new IgnoreRule("", "", SomeTable, SomeTable, ""),
        () => new IgnoreRule(SomeDB, "", "", SomeTable, ""),
        () => new IgnoreRule("", SomeSchema, "", SomeTable, ""),
        () => new IgnoreRule("", "", "", SomeTable, ""),

        () => new IgnoreRule(null, SomeSchema, SomeTable, SomeTable, null),
        () => new IgnoreRule(SomeDB, null, SomeTable, SomeTable, null),
        () => new IgnoreRule(SomeDB, SomeSchema, null, SomeTable, null),
        () => new IgnoreRule(null, null, SomeTable, SomeTable, null),
        () => new IgnoreRule(SomeDB, null, null, SomeTable, null),
        () => new IgnoreRule(null, SomeSchema, null, SomeTable, null),
        () => new IgnoreRule(null, null, null, SomeTable, null),

        () => new IgnoreRule("", SomeSchema, SomeTable, "", ""),
        () => new IgnoreRule(SomeDB, "", SomeTable, "", ""),
        () => new IgnoreRule("", "", SomeTable, "", ""),

        () => new IgnoreRule(null, SomeSchema, SomeTable, null, null),
        () => new IgnoreRule(SomeDB, null, SomeTable, null, null),
        () => new IgnoreRule(null, null, SomeTable, null, null),
      };

      foreach (var func in validCases) {
         Assert.DoesNotThrow(func);
      }
    }
  }
}
