// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;


namespace Xtensive.Modelling
{
  /// <summary>
  /// Abstract base <see cref="INesting"/> implementation.
  /// </summary>
  public abstract class Nesting : INesting
  {
    private string escapedPropertyName;

    /// <inheritdoc/>
    public string PropertyName { get; private set; }

    /// <inheritdoc/>
    public string EscapedPropertyName {
      [DebuggerStepThrough]
      get { return escapedPropertyName; }
    }

    /// <inheritdoc/>
    public Node Node { get; private set; }

    /// <inheritdoc/>
    public abstract bool IsNestedToCollection { get; }

    /// <inheritdoc/>
    public abstract PropertyInfo PropertyInfo { get; }

    /// <inheritdoc/>
    public abstract Func<Node, IPathNode> PropertyGetter { get; }

    /// <inheritdoc/>
    internal abstract Action<Node, IPathNode> PropertySetter { get; }

    /// <inheritdoc/>
    public IPathNode PropertyValue {
      [DebuggerStepThrough]
      get {
        if (PropertyGetter==null)
          return null;
        return PropertyGetter(Node.Parent);
      }
      [DebuggerStepThrough]
      internal set { PropertySetter(Node.Parent, value); }
    }

    /// <exception cref="InvalidOperationException">Invalid property type.</exception>
    internal virtual void Initialize()
    {
      if (PropertyName.IsNullOrEmpty())
        return;
      escapedPropertyName = new[] {PropertyName}.RevertibleJoin(
        Modelling.Node.PathEscape, Modelling.Node.PathDelimiter);
    }


    // Constructors

    internal Nesting(Node node, string propertyName)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(propertyName, "propertyName");
      Node = node;
      PropertyName = propertyName;
      Initialize();
    }

    internal Nesting(Node node)
    {
      ArgumentValidator.EnsureArgumentIs<IModel>(node, "node");
      Node = node;
      Initialize();
    }
  }
}