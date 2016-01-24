using System.Text;
using Xtensive.Collections;

namespace Xtensive.Orm.Upgrade.Internals.Interfaces
{
  internal interface IUpgradeHintValidator
  {
    void Validate(NativeTypeClassifier<UpgradeHint> hintsToValidate);
  }
}
