// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint for update data operation.
  /// </summary>
  [Serializable]
  public class UpdateDataHint : DataHint
  {
    /// <summary>
    /// Gets the update parameter. The first is updated column path, 
    /// the second is new value or null (default value).
    /// </summary>
    public ReadOnlyList<Pair<string, object>> UpdateParameter { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      var targets = base.GetTargets();
      foreach (var pair in UpdateParameter)
        targets.AddOne(new HintTarget(ModelType.Source, pair.First));
      return targets;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Update '{0}' set ({1}) where ({2})",
        SourceTablePath,
        string.Join(", ", UpdateParameter.Select(pair =>
          string.Format("{0}={1}", pair.First, pair.Second ?? "Default"))
          .ToArray()),
        string.Join(" and ", Identities.Select(pair => pair.ToString()).ToArray()));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public UpdateDataHint(
      string sourceTablePath,  
      IList<IdentityPair> identities,
      IList<Pair<string, object>> updateParameters)
      : base(sourceTablePath, identities)
    {
      ArgumentValidator.EnsureArgumentNotNull(updateParameters, "updateParameters");
      UpdateParameter = new ReadOnlyList<Pair<string, object>>(updateParameters);
    }

  }
}