// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hints comparer that the table is recreated.
  /// How to treat it is up to comparer.
  /// </summary>
  [Serializable]
  [Obsolete("Hint is no longer in use and will be removed in future releases.")]
  public sealed class RecreateTableHint : Hint
  {
    /// <summary>
    /// Path to the table.
    /// </summary>
    public string Path { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      yield return new HintTarget(ModelType.Source, Path);
      yield return new HintTarget(ModelType.Target, Path);
    }

    public override string ToString() => $"Recreate '{Path}'";

    public RecreateTableHint(string path)
    {
      Path = path;
    }
  }
}
