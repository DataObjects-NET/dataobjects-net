// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Reflection;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Abstract base class for any node action.
  /// </summary>
  [Serializable]
  public abstract class NodeAction : LockableBase,
    INodeAction
  {
    private string path;
    private Difference difference;

    /// <inheritdoc/>
    public string Path {
      get { return path; }
      set {
        this.EnsureNotLocked();
        path = value;
      }
    }

    /// <inheritdoc/>
    public Difference Difference {
      get { return difference; }
      set {
        this.EnsureNotLocked();
        difference = value;
      }
    }

    #region Execute method

    /// <inheritdoc/>
    public virtual void Execute(IModel model)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      var item = model.Resolve(path, true);
      ActionHandler.Current.Execute(this);
      PerformExecute(model, item);
    }

    /// <summary>
    /// Actually executed <see cref="Execute"/> method call.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="item"><see cref="Path"/> resolution result.</param>
    protected abstract void PerformExecute(IModel model, IPathNode item);

    #endregion

    #region Protected \ internal methods

    /// <summary>
    /// Escapes the name in path.
    /// </summary>
    /// <param name="name">The name to escape.</param>
    /// <returns>Escaped name.</returns>
    protected static string EscapeName(string name)
    {
      return new[] {name}.RevertibleJoin(Node.PathEscape, Node.PathDelimiter);
    }

    #endregion

    #region ToString method

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      if (this is GroupingNodeAction)
        sb.Append("[");
      sb.Append(GetActionName());
      var parameters = new List<Pair<string>>();
      GetParameters(parameters);
      foreach (var kvp in parameters)
        sb.AppendFormat(", {0}={1}", kvp.First, kvp.Second);
      if (this is GroupingNodeAction)
        sb.Append("]");
      var nestedActions = GetNestedActions();
      foreach (var action in nestedActions)
        sb.AppendLine().Append(action.ToString().Indent(2));
      return sb.ToString();
    }

    /// <summary>
    /// Gets the name of the action for <see cref="ToString"/> formatting.
    /// </summary>
    /// <returns>The name of the action.</returns>
    protected virtual string GetActionName()
    {
      string sn = GetType().GetShortName();
      return sn.TryCutSuffix("Action");
    }

    /// <summary>
    /// Gets the parameters for <see cref="ToString"/> formatting.
    /// </summary>
    /// <returns>The sequence of parameters.</returns>
    protected virtual void GetParameters(List<Pair<string>> parameters)
    {
      if (path!=null)
        parameters.Add(new Pair<string>("Path", path));
    }

    /// <summary>
    /// Gets the sequence of nested actions for <see cref="ToString"/> formatting, if any.
    /// </summary>
    /// <returns>The sequence of nested actions.</returns>
    protected virtual IEnumerable<NodeAction> GetNestedActions()
    {
      return EnumerableUtils<NodeAction>.Empty;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    protected NodeAction()
    {
    }
  }
}