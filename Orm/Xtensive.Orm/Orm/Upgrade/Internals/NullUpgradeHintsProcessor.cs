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
      var suspiciousTypes = new HashSet<StoredTypeInfo>();

      return new UpgradeHintsProcessingResult(
        hints, typeMapping, reverseTypeMapping, fieldMapping, reverseFieldMapping, currentModelTypes,
        suspiciousTypes, Array.Empty<StoredTypeInfo>(), Array.Empty<StoredTypeInfo>());
    }

    public NullUpgradeHintsProcessor(StoredDomainModel currentDomainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(currentDomainModel, nameof(currentDomainModel));

      this.currentDomainModel = currentDomainModel;
    }
  }
}