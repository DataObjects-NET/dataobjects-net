// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Node"/> comparison result.
  /// </summary>
  public class NodeDifference : Difference,
    IDifference<Node>
  {
    /// <inheritdoc/>
    public new Node Source {
      get { return (Node) base.Source; }
    }

    /// <inheritdoc/>
    public new Node Target {
      get { return (Node) base.Target; }
    }

    /// <summary>
    /// Gets or sets the movement info.
    /// </summary>
    public MovementInfo MovementInfo { get; set; }

    /// <summary>
    /// Gets list of property changes.
    /// </summary>
    public Dictionary<string, Difference> PropertyChanges { get; private set; }

    /// <inheritdoc/>
    public override void Build(ActionSequence sequence)
    {
      // Processing movement
      using (var scope = sequence.LogAction()) {
        if (MovementInfo.IsRemoved) {
          var ra = new RemoveNodeAction() {Path = Source.Path};
          scope.Action = ra;
          scope.Commit();
          return;
        }

        if (MovementInfo.IsCreated) {
          var ca = new CreateNodeAction()
            {
              Path = Target.Parent==null ? string.Empty : Target.Parent.Path,
              Type = Target.GetType(),
              Name = Target.Name,
              Index = Target.Index
            };
          scope.Action = ca;
          scope.Commit();
        }
        else if (!MovementInfo.IsUnchanged) {
          var ca = new MoveNodeAction()
            {
              Path = Source.Path,
              Parent = Target.Parent==null ? string.Empty : Target.Parent.Path,
              Name = Target.Name,
              Index = Target.Index
            };
          scope.Action = ca;
          scope.Commit();
        }
      }

      // And property changes
      foreach (var pair in PropertyChanges)
        pair.Value.Build(sequence);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      if (MovementInfo!=null)
        sb.Append(MovementInfo);
      foreach (var pair in PropertyChanges)
        sb.AppendLine().AppendFormat(Strings.PropertyChangeFormat, pair.Key, pair.Value);
      return sb.ToString().Indent(ToString_IndentSize, false);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    public NodeDifference(Node source, Node target)
      : base(source, target)
    {
      PropertyChanges = new Dictionary<string, Difference>();
    }
  }
}