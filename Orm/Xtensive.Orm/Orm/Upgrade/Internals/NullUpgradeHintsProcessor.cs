using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Upgrade.Internals.Interfaces;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class NullUpgradeHintsProcessor : IUpgradeHintsProcessor
  {
    public UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints)
    {
      var hints = new NativeTypeClassifier<UpgradeHint>(true);
      var typeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      var reverseTypeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      var fieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      return new UpgradeHintsProcessingResult(hints, typeMapping, reverseTypeMapping, fieldMapping);
    }
  }
}