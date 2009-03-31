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
    private int index;
    private object[] parameters;
    private string afterPath;
    private string newPath;

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

    public int Index {
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

    public string AfterPath {
      get { return afterPath; }
      set {
        this.EnsureNotLocked();
        afterPath = value;
      }
    }

    public string NewPath {
      get { return newPath; }
      set {
        this.EnsureNotLocked();
        newPath = value;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    protected override void PerformApply(IModel model, IPathNode item)
    {
      var node = (Node) item;
      var tmpIndex = index;
      if (AfterPath!=null) {
        var pathNode = model.Resolve(AfterPath);
        tmpIndex = (pathNode is NodeCollection) ? 0 : ((Node) pathNode).Index + 1;
      }
      if (TryConstructor(model, node, name, tmpIndex))
        return;
      if (TryConstructor(model, node, name))
        return;
      if (TryConstructor(model, node, tmpIndex))
        return;
      throw new InvalidOperationException(string.Format(
        Strings.ExCannotFindConstructorToExecuteX, this));
    }

    protected bool TryConstructor(IModel model, params object[] args)
    {
      if (parameters!=null)
        args = args.Concat(parameters.Select(p => PathNodeReference.Resolve(model, p))).ToArray();
      var argTypes = args.Select(a => a.GetType()).ToArray();
      var ci = type.GetConstructor(argTypes);
      if (ci==null)
        return false;
      ci.Invoke(args);
      return true;
    }

    /// <inheritdoc/>
    protected override void GetParameters(List<Pair<string>> parameters)
    {
      base.GetParameters(parameters);
      parameters.Add(new Pair<string>("Type", type.GetShortName()));
      parameters.Add(new Pair<string>("Name", name));
      if (AfterPath==null)
        parameters.Add(new Pair<string>("Index", index.ToString()));
      else
        parameters.Add(new Pair<string>("AfterPath", AfterPath));
      if (this.parameters!=null)
        parameters.Add(new Pair<string>("Parameters", this.parameters.ToCommaDelimitedString()));
    }

    /// <inheritdoc/>
    public override string[] GetDependencies()
    {
      return new[] {NewPath};
    }

    /// <inheritdoc/>
    public override string[] GetRequiredDependencies()
    {
      var dependencies = base.GetRequiredDependencies();
      dependencies = dependencies
        .Concat(new[] {NewPath + ".Remove()"})
        .ToArray();
      if (AfterPath!=null)
        dependencies = dependencies
          .Concat(new[] {AfterPath})
          .ToArray();
      return dependencies;
    }
  }
}