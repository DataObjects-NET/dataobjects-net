// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

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

    /// <inheritdoc/>
    public string Path {
      get { return path; }
      set {
        this.EnsureNotLocked();
        path = value;
      }
    }

    #region Execute method

    /// <inheritdoc/>
    public virtual void Execute(IModel model)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      var item = model.Resolve(path);
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

    protected string EscapeName(string name)
    {
      return new[] {name}.RevertibleJoin(Node.PathEscape, Node.PathDelimiter);
    }

    #endregion

    #region ToString method

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(GetActionName());
      var parameters = new List<Pair<string>>();
      GetParameters(parameters);
      foreach (var kvp in parameters)
        sb.AppendFormat(", {0}={1}", kvp.First, kvp.Second);
      return sb.ToString();
    }

    /// <summary>
    /// Gets the parameters for <see cref="ToString"/> formatting.
    /// </summary>
    /// <returns>The sequence of parameters.</returns>
    protected virtual void GetParameters(List<Pair<string>> parameters)
    {
      parameters.Add(new Pair<string>("Path", path));
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

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected NodeAction()
    {
    }
  }
}