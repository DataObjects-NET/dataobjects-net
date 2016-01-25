// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

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

    public Dictionary<StoredFieldInfo, StoredFieldInfo> ReverseFieldMapping { get; private set; }

    public Dictionary<string, StoredTypeInfo> CurrentModelTypes { get; private set; }

    public UpgradeHintsProcessingResult(NativeTypeClassifier<UpgradeHint> hints,
      Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping,
      Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> reverseFieldMapping,
      Dictionary<string, StoredTypeInfo> currentModelTypes)
    {
      Hints = hints;
      TypeMapping = typeMapping;
      ReverseTypeMapping = reverseTypeMapping;
      FieldMapping = fieldMapping;
      ReverseFieldMapping = reverseFieldMapping;
      CurrentModelTypes = currentModelTypes;
    }
  }
}