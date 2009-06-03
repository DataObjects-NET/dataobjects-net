// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using Xtensive.Core;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint for copy data operation.
  /// </summary>
  [Serializable]
  public sealed class CopyDataHint : DataHint
  {
    /// <summary>
    /// Gets or sets the copied columns. The first value is source column path, 
    /// the second value is destination column path.
    /// </summary>
    public List<Pair<string>> CopiedColumns { get; private set; }
    
    /// <summary>
    /// Creates new <see cref="CopyDataHint"/> according to temporary renames.
    /// </summary>
    /// <param name="temporaryRenames">The temporary renames set.</param>
    /// <returns>Updates hint.</returns>
    public CopyDataHint Update(Dictionary<string, Node> temporaryRenames)
    {
      var copyDataHint = new CopyDataHint();
      {
        Node newNode;
        if (temporaryRenames.TryGetValue(SourceTablePath, out newNode))
          copyDataHint.SourceTablePath = newNode.Path;
        else
          copyDataHint.SourceTablePath = SourceTablePath;
      }
      foreach (var pair in CopiedColumns) {
        Node newNode;
        if (temporaryRenames.TryGetValue(pair.First, out newNode))
          copyDataHint.CopiedColumns.Add(new Pair<string>(newNode.Path, pair.Second));
        else
          copyDataHint.CopiedColumns.Add(pair);
      }
      foreach (var pair in Identities) {
        Node newNode;
        if (temporaryRenames.TryGetValue(pair.Source, out newNode))
          copyDataHint.Identities.Add(
            new IdentityPair(newNode.Path, pair.Target, pair.IsIdentifiedByConstant));
        else
          copyDataHint.Identities.Add(pair);
      }
      return copyDataHint;
    }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      var targets = new List<HintTarget>();
      targets.Add(new HintTarget(ModelType.Source, SourceTablePath));
      foreach (var pair in CopiedColumns) {
        targets.Add(new HintTarget(ModelType.Source, pair.First));
        targets.Add(new HintTarget(ModelType.Target, pair.Second));
      }
      foreach (var pair in Identities) {
        targets.Add(new HintTarget(ModelType.Source, pair.Source));
        if (!pair.IsIdentifiedByConstant)
          targets.Add(new HintTarget(ModelType.Target, pair.Target));
      }
      return targets;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Copy from ({0}) to ({1}) where ({2})",
        string.Join(", ", CopiedColumns.Select(pair => pair.First)
          .ToArray()),
        string.Join(", ", CopiedColumns.Select(pair => pair.Second)
          .ToArray()),
        string.Join(" and ", Identities.Select(pair => pair.ToString())
          .ToArray()));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CopyDataHint()
    {
      CopiedColumns = new List<Pair<string>>();
    }

    
  }
}