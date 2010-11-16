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
      hints.Add(new MoveFieldHint("B", "Reference", typeof (ModelVersion2.A)));
      base.AddUpgradeHints(hints);
    }
  }
}
