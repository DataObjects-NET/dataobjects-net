// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// An abstract base class for all data hints.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{GetType().Name}, {SourceTablePath}")]
  public abstract class DataHint : Hint
  {
    /// <summary>
    /// Gets or sets the source table path.
    /// </summary>
    public string SourceTablePath { get; set; }

    /// <summary>
    /// Gets or sets the identities for data operation.
    /// </summary>
    public List<IdentityPair> Identities { get; private set; }
    
    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      var targets = new List<HintTarget>();
      targets.Add(new HintTarget(ModelType.Source, SourceTablePath));
      Identities.Apply(pair => {
        targets.Add(new HintTarget(ModelType.Source, pair.Source));
        if (!pair.IsIdentifiedByConstant)
          targets.Add(new HintTarget(ModelType.Source, pair.Target));
      });
      return targets;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected DataHint()
    {
      Identities = new List<IdentityPair>();
    }
  }
}