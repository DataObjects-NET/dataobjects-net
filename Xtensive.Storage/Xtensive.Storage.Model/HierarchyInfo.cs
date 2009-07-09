// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class HierarchyInfo : Node
  {
    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeInfo Root { get; private set; }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema { get; private set; }

    /// <summary>
    /// Gets the types of the current <see cref="HierarchyInfo"/>.
    /// </summary>
    public ReadOnlyList<TypeInfo> Types { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="KeyInfo"/> property for this instance.
    /// </summary>
    public KeyInfo KeyInfo { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="GeneratorInfo"/> property for this instance.
    /// </summary>
    public GeneratorInfo GeneratorInfo { get; private set; }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      KeyInfo.UpdateState(recursive);
      var list = new List<TypeInfo> {Root};
      list.AddRange(Root.GetDescendants(true));
      Types = new ReadOnlyList<TypeInfo>(list);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      KeyInfo.Lock(recursive);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="root">The hierarchy root.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="keyInfo">The key info.</param>
    /// <param name="generatorInfo">The generator info.</param>
    public HierarchyInfo(TypeInfo root, InheritanceSchema schema, KeyInfo keyInfo, GeneratorInfo generatorInfo)
    {
      Root = root;
      Schema = schema;
      KeyInfo = keyInfo;
      GeneratorInfo = generatorInfo;
    }
  }
}