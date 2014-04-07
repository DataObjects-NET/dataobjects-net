// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.14

using System.Collections.Generic;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class HintGenerationResult
  {
    public List<UpgradeHint> ModelHints { get; private set; }

    public List<Hint> SchemaHints { get; private set; }

    public Dictionary<string, string> TypeMapAfterHintGeneration { get; private set; }

    public HintGenerationResult(List<UpgradeHint> modelHints, List<Hint> schemaHints, Dictionary<string, string> typeMapAfterHintGeneration)
    {
      ModelHints = modelHints;
      SchemaHints = schemaHints;
      TypeMapAfterHintGeneration = typeMapAfterHintGeneration ?? new Dictionary<string, string>();
    }
  }
}