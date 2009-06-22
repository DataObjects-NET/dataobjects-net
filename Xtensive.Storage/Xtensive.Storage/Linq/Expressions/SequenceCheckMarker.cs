// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.22

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class SequenceCheckMarker : ExtendedExpression
  {
    public Expression Target { get; set; }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SequenceCheckMarker(Expression target)
      : base(ExtendedExpressionType.SequenceCheckMarker, target.Type)
    {
      Target = target;
    }
  }
}