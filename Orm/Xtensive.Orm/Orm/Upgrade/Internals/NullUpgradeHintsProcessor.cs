// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Upgrade.Internals.Interfaces;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class NullUpgradeHintsProcessor : IUpgradeHintsProcessor
  {
    private readonly StoredDomainModel currentDomainModel;

    public UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints)
    {
      var hints = new NativeTypeClassifier<UpgradeHint>(true);
      var typeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      var reverseTypeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      var fieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      var reverseFieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      var currentModelTypes = currentDomainModel.Types.ToDictionary(t => t.UnderlyingType);
      return new UpgradeHintsProcessingResult(hints, typeMapping, reverseTypeMapping, fieldMapping, reverseFieldMapping, currentModelTypes);
    }

    public NullUpgradeHintsProcessor(StoredDomainModel currentDomainModel)
    {
      this.currentDomainModel = currentDomainModel;
    }
  }
}