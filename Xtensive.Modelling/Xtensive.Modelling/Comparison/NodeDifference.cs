// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Core.Helpers;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Node"/> comparison result.
  /// </summary>
  public class NodeDifference : Difference,
    IDifference<Node>,
    IHasPropertyChanges
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

    /// <inheritdoc/>
    public Dictionary<string, Difference> PropertyChanges { get; private set; }

    /// <inheritdoc/>
    public override void AppendActions(IList<NodeAction> sequence)
    {
      // Processing movement
      if (MovementInfo.IsRemoved) {
        sequence.Add(
          new RemoveNodeAction() {Path = (Source ?? Target).Path});
        if (Source!=null)
          return;
      }

      if (MovementInfo.IsCreated) {
        var cna = new CreateNodeAction()
          {
            Path = Target.Parent==null ? string.Empty : Target.Parent.Path,
            Type = Target.GetType(),
            Name = Target.Name,
          };
        if (Target.Nesting.IsNestedToCollection) {
          var collection = (NodeCollection) Target.Nesting.PropertyValue;
          cna.AfterPath = Target.Index==0 ? collection.Path : collection[Target.Index - 1].Path;
        }
        sequence.Add(cna);
      }
      else if (!MovementInfo.IsUnchanged) {
        sequence.Add(new MoveNodeAction()
          {
            Path = Source.Path,
            Parent = Target.Parent==null ? string.Empty : Target.Parent.Path,
            Name = Target.Name,
            Index = Target.Index,
            NewPath = Target.Path
          });
      }

      // And property changes
      foreach (var pair in PropertyChanges)
        pair.Value.AppendActions(sequence);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      var sb = new StringBuilder();
      if (MovementInfo!=null)
        sb.Append(MovementInfo);
      foreach (var pair in PropertyChanges)
        sb.AppendLine().AppendFormat(Strings.PropertyChangeFormat,
          pair.Key, pair.Value);
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