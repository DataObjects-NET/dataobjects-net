// Copyright (C) 2015-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System;
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
      Hints                      = hints ?? throw new ArgumentNullException(nameof(hints));
      TypeMapping                = typeMapping ?? throw new ArgumentNullException(nameof(typeMapping));
      ReverseTypeMapping         = reverseTypeMapping ?? throw new ArgumentNullException(nameof(reverseTypeMapping));
      FieldMapping               = fieldMapping ?? throw new ArgumentNullException(nameof(fieldMapping));
      ReverseFieldMapping        = reverseFieldMapping ?? throw new ArgumentNullException(nameof(reverseFieldMapping));
      CurrentModelTypes          = currentModelTypes ?? throw new ArgumentNullException(nameof(currentModelTypes));
      SuspiciousTypes            = suspiciousTypes ?? throw new ArgumentNullException(nameof(suspiciousTypes));
      CurrentNonConnectorTypes   = currentNonConnectorTypes ?? throw new ArgumentNullException(nameof(currentNonConnectorTypes));
      ExtractedNonConnectorTypes = extractedNonConnectorTypes ?? throw new ArgumentNullException(nameof(extractedNonConnectorTypes));
    }
  }
}