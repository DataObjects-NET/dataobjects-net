using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint
{
  public class Upgrader: UpgradeHandler
  {
    protected override void AddUpgradeHints(Core.Collections.ISet<UpgradeHint> hints)
    {
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.A).FullName, typeof (ModelVersion2.A)));
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.B).FullName, typeof (ModelVersion2.B)));
      hints.Add(new RenameTypeHint(typeof (ModelVersion1.X).FullName, typeof (ModelVersion2.X)));
      hints.Add(new MoveFieldHint(typeof (ModelVersion1.B).FullName, "Reference", typeof (ModelVersion2.A)));
      base.AddUpgradeHints(hints);
    }
  }
}
