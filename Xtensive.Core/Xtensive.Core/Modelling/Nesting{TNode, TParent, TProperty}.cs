// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using Xtensive.Resources;
using Xtensive.Helpers;

namespace Xtensive.Modelling
{
  /// <summary>
  /// <see cref="INesting"/> implementation.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  /// <typeparam name="TProperty">The type of the property.</typeparam>
  [Serializable]
  public sealed class Nesting<TNode, TParent, TProperty> : Nesting
    where TNode: Node
    where TParent: Node
    where TProperty: IPathNode
  {
    [NonSerialized]
    private PropertyInfo propertyInfo;
    [NonSerialized]
    private bool isNestedToCollection;
    [NonSerialized]
    private Func<Node, IPathNode> propertyGetter;
    [NonSerialized]
    private Action<Node, IPathNode> propertySetter;

    /// <inheritdoc/>
    public override PropertyInfo PropertyInfo {
      [DebuggerStepThrough]
      get { return propertyInfo; }
    }

    /// <inheritdoc/>
    public override bool IsNestedToCollection {
      [DebuggerStepThrough]
      get { return isNestedToCollection; }
    }

    /// <inheritdoc/>
    public override Func<Node, IPathNode> PropertyGetter {
      [DebuggerStepThrough]
      get { return propertyGetter; }
    }

    /// <inheritdoc/>
    internal override Action<Node, IPathNode> PropertySetter {
      [DebuggerStepThrough]
      get { return propertySetter; }
    }

    /// <exception cref="InvalidOperationException">Invalid property type.</exception>
    internal  override void Initialize()
    {
      base.Initialize();
      if (PropertyName.IsNullOrEmpty())
        return;

      var tNode = typeof (TNode);
      var tParent = typeof (TParent);
      var tProperty = typeof (TProperty);
      
      propertyInfo = tParent.GetProperty(PropertyName);
      if (propertyInfo==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExBindingFailedForX, tParent.GetShortName()+"."+PropertyName));
      if (propertyInfo.PropertyType!=tProperty)
        throw new InvalidOperationException(String.Format(
          Strings.ExTypeOfXPropertyMustBeY, 
          propertyInfo.GetShortName(true), tProperty.GetShortName()));
      isNestedToCollection = typeof (NodeCollection).IsAssignableFrom(tProperty);

      // Getter
      var typedGetter = DelegateHelper.CreateGetMemberDelegate<TParent, TProperty>(PropertyName);
      if (typedGetter==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExBindingFailedForX, propertyInfo.GetShortName(true)));
      propertyGetter = 
        n => typedGetter.Invoke((TParent) n);
      
      // Setter
      var typedSetter = DelegateHelper.CreateSetMemberDelegate<TParent, TProperty>(PropertyName);
      if (typedSetter==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExBindingFailedForX, propertyInfo.GetShortName(true)));
      propertySetter = 
        (n,v) => typedSetter.Invoke((TParent) n, (TProperty) v);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="node"><see cref="Node"/> property value.</param>
    /// <param name="propertyName"><see cref="Nesting.PropertyName"/> property value.</param>
    public Nesting(TNode node, string propertyName)
      : base(node, propertyName)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="node"><see cref="Node"/> property value.</param>
    public Nesting(TNode node)
      : base(node)
    {
    }
  }
}