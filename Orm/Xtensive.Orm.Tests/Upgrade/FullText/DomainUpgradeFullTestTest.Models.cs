// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version1
{
  [HierarchyRoot]
  public class Article : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version2
{
  [HierarchyRoot]
  public class Article : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [FullText("English")]
    [Field]
    public string Title { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version3
{
  [HierarchyRoot]
  public class Article : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [FullText("German")]
    [Field]
    public string Title { get; set; }

    [FullText("German")]
    [Field]
    public string Content { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version4
{
  [HierarchyRoot]
  public class Article : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [FullText("German")]
    [Field]
    public string Title { get; set; }

    [FullText("German")]
    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version5
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [FullText("German")]
    [Field]
    public string Title { get; set; }

    [FullText("German")]
    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version6
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model
{
  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static string runningVersion;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(string version)
    {
      if (isEnabled) {
        throw new InvalidOperationException();
      }
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled => isEnabled;

    protected override string DetectAssemblyVersion() => runningVersion;

    public override bool CanUpgradeFrom(string oldVersion) => true;


    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      var context = UpgradeContext;
      switch (runningVersion) {
        case "Version1":
          break;
        case "Version2":
          _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version1.Article", typeof(Model.Version2.Article)));
          break;
        case "Version3":
          _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version2.Article", typeof(Model.Version3.Article)));
          break;
        case "Version4":
          _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version3.Article", typeof(Model.Version4.Article)));
          _ = hints.Add(new RenameFieldHint(typeof(Model.Version4.Article), "Content", "Text"));
          break;
        case "Version5":
          _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version4.Article", typeof(Model.Version5.Book)));
          break;
        case "Version6":
          _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version5.Book", typeof(Model.Version6.Book)));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}