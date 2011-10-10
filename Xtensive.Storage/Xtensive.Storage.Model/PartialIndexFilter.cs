// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Partial index filter definition.
  /// </summary>
  public class PartialIndexFilter : Node
  {
    private LambdaExpression expression;

    /// <summary>
    /// Expression that defines partial index.
    /// </summary>
    public LambdaExpression Expression
    {
      get { return expression; }
      set
      {
        this.EnsureNotLocked();
        expression = value;
      }
    }

    /// <summary>
    /// Fields used in <see cref="Expression"/>.
    /// </summary>
    public FieldInfoCollection Fields { get; private set; }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      if (Expression==null)
        throw Exceptions.NotInitialized("Expression");
      base.Lock(recursive);
      if (recursive)
        Fields.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PartialIndexFilter()
    {
      Fields = new FieldInfoCollection(this, "Fields");
    }
  }
}