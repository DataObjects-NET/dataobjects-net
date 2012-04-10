// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Partial index filter definition.
  /// </summary>
  public sealed class PartialIndexFilterInfo : Node
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

    private IList<FieldInfo> fields;

    /// <summary>
    /// Fields used in <see cref="Expression"/>.
    /// </summary>
    public IList<FieldInfo> Fields
    {
      get { return fields; }
      set
      {
        this.EnsureNotLocked();
        fields = value;
      }
    }

    
    /// <summary>
    /// Locks the instance and (possibly) all dependent objects.
    /// 
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      if (IsLocked)
        return;
      if (Expression==null)
        throw Exceptions.NotInitialized("Expression");
      if (Fields==null)
        throw Exceptions.NotInitialized("Fields");
      fields = fields.ToList().AsReadOnly();
      base.Lock(recursive);
    }
  }
}