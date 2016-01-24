using System.Text;
using Xtensive.Collections;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal interface IUpgradeHintValidator
  {
    void Validate(NativeTypeClassifier<UpgradeHint> hintsToValidate);
  }
}
