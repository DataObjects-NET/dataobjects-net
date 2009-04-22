// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Resources;
using System.Linq;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Describes node creation.
  /// </summary>
  [Serializable]
  public class CreateNodeAction : NodeAction
  {
    private Type type;
    private string name;
    private int? index;
    private object[] parameters;

    public Type Type {
      get { return type; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        type = value;
      }
    }

    public string Name {
      get { return name; }
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.EnsureNotLocked();
        name = value;
      }
    }

    public int? Index {
      get { return index; }
      set {
        this.EnsureNotLocked();
        index = value;
      }
    }

    public object[] Parameters {
      get {
        if (!IsLocked)
          return parameters;
        return parameters==null ? null : (object[]) parameters.Clone();
      }
      set {
        this.EnsureNotLocked();
        parameters = value;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var parent = (Node) item;
      var node = TryConstructor(model, parent, name); // Regular node
      if (node==null)
        node = TryConstructor(model, parent); // Unnamed node
      if (node==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCannotFindConstructorToExecuteX, this));
      if (index.HasValue)
        node.Index = index.Value;
    }

    protected Node TryConstructor(IModel model, params object[] args)
    {
      if (parameters!=null)
        args = args.Concat(parameters.Select(p => PathNodeReference.Resolve(model, p))).ToArray();
      var argTypes = args.Select(a => a.GetType()).ToArray();
      var ci = type.GetConstructor(argTypes);
      if (ci==null)
        return null;
      return (Node) ci.Invoke(args);
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      parameters.Add(new Pair<string>("Type", type.GetShortName()));
      parameters.Add(new Pair<string>("Name", name));
      if (index.HasValue)
        parameters.Add(new Pair<string>("Index", index.ToString()));
      if (this.parameters!=null)
        parameters.Add(new Pair<string>("Parameters", this.parameters.ToCommaDelimitedString()));
    }
  }
}