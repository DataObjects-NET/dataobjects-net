// Copyright (C) 2015-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class UpgradeHintsProcessingResult
  {
    public NativeTypeClassifier<UpgradeHint> Hints { get; }

    public Dictionary<StoredTypeInfo, StoredTypeInfo> TypeMapping { get; }

    public Dictionary<StoredTypeInfo, StoredTypeInfo> ReverseTypeMapping { get; }

    public Dictionary<StoredFieldInfo, StoredFieldInfo> FieldMapping { get; }

    public Dictionary<StoredFieldInfo, StoredFieldInfo> ReverseFieldMapping { get; }

    public Dictionary<string, StoredTypeInfo> CurrentModelTypes { get; }

    public HashSet<StoredTypeInfo> SuspiciousTypes { get; }

    public IReadOnlyList<StoredTypeInfo> CurrentNonConnectorTypes { get; }

    public IReadOnlyList<StoredTypeInfo> ExtractedNonConnectorTypes { get; }

    public bool AreAllTypesMapped()
    {
      return CurrentModelTypes.Count == TypeMapping.Count && CurrentModelTypes.Count==ReverseTypeMapping.Count;
    }

    public Dictionary<string, string> GetUpgradedTypeMap()
    {
      return ReverseTypeMapping
        .ToDictionary(item => item.Key.UnderlyingType, item => item.Value.UnderlyingType);
    }

    public UpgradeHintsProcessingResult(NativeTypeClassifier<UpgradeHint> hints,
      Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping,
      Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> reverseFieldMapping,
      Dictionary<string, StoredTypeInfo> currentModelTypes,
      HashSet<StoredTypeInfo> suspiciousTypes,
      IReadOnlyList<StoredTypeInfo> currentNonConnectorTypes,
      IReadOnlyList<StoredTypeInfo> extractedNonConnectorTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(hints, nameof(hints));
      ArgumentValidator.EnsureArgumentNotNull(typeMapping, nameof(typeMapping));
      ArgumentValidator.EnsureArgumentNotNull(reverseTypeMapping, nameof(reverseTypeMapping));
      ArgumentValidator.EnsureArgumentNotNull(fieldMapping, nameof(fieldMapping));
      ArgumentValidator.EnsureArgumentNotNull(reverseFieldMapping, nameof(reverseFieldMapping));
      ArgumentValidator.EnsureArgumentNotNull(currentModelTypes, nameof(currentModelTypes));
      ArgumentValidator.EnsureArgumentNotNull(suspiciousTypes, nameof(suspiciousTypes));
      ArgumentValidator.EnsureArgumentNotNull(currentNonConnectorTypes, nameof(suspiciousTypes));
      ArgumentValidator.EnsureArgumentNotNull(extractedNonConnectorTypes, nameof(suspiciousTypes));

      Hints = hints;
      TypeMapping = typeMapping;
      ReverseTypeMapping = reverseTypeMapping;
      FieldMapping = fieldMapping;
      ReverseFieldMapping = reverseFieldMapping;
      CurrentModelTypes = currentModelTypes;
      SuspiciousTypes = suspiciousTypes;
      CurrentNonConnectorTypes = currentNonConnectorTypes;
      ExtractedNonConnectorTypes = extractedNonConnectorTypes;
    }
  }
}