// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2010.11.17

using System.Collections.Generic;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint
{
  public class Upgrader: UpgradeHandler
  {
    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.A).FullName, typeof (ModelVersion2.A)));
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.B).FullName, typeof (ModelVersion2.B)));
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.X).FullName, typeof (ModelVersion2.X)));
      hints.Add(new MoveFieldHint(typeof (ModelVersion1.B).FullName, "Reference", typeof (ModelVersion2.A)));
      base.AddUpgradeHints(hints);
    }
  }
}
