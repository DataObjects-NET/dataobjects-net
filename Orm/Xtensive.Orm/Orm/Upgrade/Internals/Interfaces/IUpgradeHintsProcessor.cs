// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System.Collections.Generic;

namespace Xtensive.Orm.Upgrade.Internals.Interfaces
{
  internal interface IUpgradeHintsProcessor
  {
    UpgradeHintsProcessingResult Process(IEnumerable<UpgradeHint> inputHints);
  }
}