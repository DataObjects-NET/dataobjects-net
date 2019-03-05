// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System.Collections.Generic;
using System.Linq;
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

    public Dictionary<StoredFieldInfo, StoredFieldInfo> ReverseFieldMapping { get; private set; }

    public Dictionary<string, StoredTypeInfo> CurrentModelTypes { get; private set; }

    public List<StoredTypeInfo> SuspiciousTypes { get; set; }

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
      List<StoredTypeInfo> suspiciousTypes)
    {
      Hints = hints;
      TypeMapping = typeMapping;
      ReverseTypeMapping = reverseTypeMapping;
      FieldMapping = fieldMapping;
      ReverseFieldMapping = reverseFieldMapping;
      CurrentModelTypes = currentModelTypes;
      SuspiciousTypes = suspiciousTypes;
    }
  }
}