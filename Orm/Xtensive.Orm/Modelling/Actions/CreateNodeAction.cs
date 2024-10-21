// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Collections;

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

    /// <summary>
    /// Gets or sets the node type.
    /// </summary>
    public Type Type {
      get { return type; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        EnsureNotLocked();
        type = value;
      }
    }

    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    public string Name {
      get { return name; }
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        EnsureNotLocked();
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the node index.
    /// </summary>
    /// <value>The index.</value>
    public int? Index {
      get { return index; }
      set {
        EnsureNotLocked();
        index = value;
      }
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    public object[] Parameters {
      get {
        if (!IsLocked)
          return parameters;
        return parameters==null ? null : (object[]) parameters.Clone();
      }
      set {
        EnsureNotLocked();
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

    /// <summary>
    /// Tries to invoke node constructor with the specified set of arguments.
    /// </summary>
    /// <param name="model">The model to pass as the first argument.</param>
    /// <param name="arguments">The other arguments.</param>
    /// <returns>Created node, if the constructor was successfully bound;
    /// otherwise, <see langword="null" />.</returns>
    protected Node TryConstructor(IModel model, params object[] arguments)
    {
      if (parameters!=null)
        arguments = arguments.Concat(parameters.Select(p => PathNodeReference.Resolve(model, p))).ToArray(arguments.Length + parameters.Length);
      var argTypes = arguments.SelectToArray(a => a.GetType());
      var ci = type.GetConstructor(argTypes);
      if (ci==null)
        return null;
      return (Node) ci.Invoke(arguments);
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