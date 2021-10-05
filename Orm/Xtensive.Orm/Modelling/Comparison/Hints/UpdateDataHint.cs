// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public IReadOnlyList<Pair<string, object>> UpdateParameter { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      var targets = base.GetTargets();
      foreach (var pair in UpdateParameter) {
        targets = targets.Append(new HintTarget(ModelType.Source, pair.First));
      }

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
    /// Initializes new instance of this type.
    /// </summary>
    public UpdateDataHint(
      string sourceTablePath,  
      IList<IdentityPair> identities,
      IList<Pair<string, object>> updateParameters)
      : base(sourceTablePath, identities)
    {
      ArgumentValidator.EnsureArgumentNotNull(updateParameters, "updateParameters");
      UpdateParameter = new ReadOnlyCollection<Pair<string, object>>(updateParameters);
    }

  }
}
