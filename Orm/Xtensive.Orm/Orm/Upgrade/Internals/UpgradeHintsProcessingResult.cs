using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Orm.Model.Stored;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class UpgradeHintsProcessingResult
  {
    public NativeTypeClassifier<UpgradeHint> Hints { get; private set; }

    public Dictionary<StoredTypeInfo, StoredTypeInfo> TypeMapping { get; private set; }

    public Dictionary<StoredTypeInfo, StoredTypeInfo> ReverseTypeMapping { get; private set; }

    public Dictionary<StoredFieldInfo, StoredFieldInfo> FieldMapping { get; private set; }

    public UpgradeHintsProcessingResult(NativeTypeClassifier<UpgradeHint> hints,
      Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping,
      Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping)
    {
      Hints = hints;
      TypeMapping = typeMapping;
      ReverseTypeMapping = reverseTypeMapping;
      FieldMapping = fieldMapping;
    }
  }
}