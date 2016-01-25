// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using Xtensive.Collections;

namespace Xtensive.Orm.Upgrade.Internals.Interfaces
{
  internal interface IUpgradeHintValidator
  {
    void Validate(NativeTypeClassifier<UpgradeHint> hintsToValidate);
  }
}
