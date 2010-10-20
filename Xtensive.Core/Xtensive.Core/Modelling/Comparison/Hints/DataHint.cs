// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// An abstract base class for all data hints.
  /// </summary>
  [Serializable]
  public abstract class DataHint : Hint
  {
    /// <summary>
    /// Gets the source table path.
    /// </summary>
    public string SourceTablePath { get; private set; }

    /// <summary>
    /// Gets the identities for data operation.
    /// </summary>
    public ReadOnlyList<IdentityPair> Identities { get; private set; }
    
    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      var targets = new List<HintTarget>();
      targets.Add(new HintTarget(ModelType.Source, SourceTablePath));
      Identities.ForEach(pair => {
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
    protected DataHint(string sourceTablePath,  IList<IdentityPair> identities)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceTablePath, "sourceTablePath");
      ArgumentValidator.EnsureArgumentNotNull(identities, "pairs");
      
      SourceTablePath = sourceTablePath;
      Identities = new ReadOnlyList<IdentityPair>(identities);
    }
  }
}