// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.22

using System.Linq.Expressions;


namespace Xtensive.Orm.Linq.Expressions
{
  internal class MarkerExpression : ExtendedExpression
  {
    public Expression Target { get; private set; }
    public MarkerType MarkerType { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public MarkerExpression(Expression target, MarkerType markerType)
      : base(ExtendedExpressionType.Marker, target.Type)
    {
      Target = target;
      MarkerType = markerType;
    }
  }
}