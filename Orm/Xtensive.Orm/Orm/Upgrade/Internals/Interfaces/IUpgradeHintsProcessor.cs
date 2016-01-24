using System.Collections.Generic;

namespace Xtensive.Orm.Upgrade.Internals.Interfaces
{
  internal interface IUpgradeHintsProcessor
  {
    UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints);
  }
}