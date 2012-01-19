// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.22

using System.Linq.Expressions;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class MarkerExpression : ExtendedExpression
  {
    public Expression Target { get; private set; }
    public MarkerType MarkerType { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MarkerExpression(Expression target, MarkerType markerType)
      : base(ExtendedExpressionType.Marker, target.Type)
    {
      Target = target;
      MarkerType = markerType;
    }
  }
}