// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents a model of <see cref="Storage"/>.
  /// </summary>
  [Serializable]
  public sealed class DomainModel: Node
  {
    internal readonly object unlockKey = new object();

    /// <summary>
    /// Gets the services contained in this instance.
    /// </summary>
    public ServiceInfoCollection Services { get; private set; }

    /// <summary>
    /// Gets the <see cref="TypeInfo"/> instances contained in this instance.
    /// </summary>
    public TypeInfoCollection Types { get; private set; }

    /// <summary>
    /// Gets real indexes contained in this instance.
    /// </summary>
    public IndexInfoCollection RealIndexes { get; private set; }

    /// <summary>
    /// Gets the hierarchies.
    /// </summary>
    public HierarchyInfoCollection Hierarchies { get; private set; }

    /// <summary>
    /// Gets or sets the associations.
    /// </summary>
    public AssociationInfoCollection Associations { get; private set;}

    /// <summary>
    /// Gets or sets the generators.
    /// </summary>
    public GeneratorInfoCollection Generators { get; private set;}

    /// <summary>
    /// Gets the field from the current <see cref="DomainModel"/> by <paramref name="fieldExtractor"/> expression.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="fieldExtractor">The field expression.</param>
    public FieldInfo GetField<T>(Expression<Func<T,object>> fieldExtractor)
    {
      var e = fieldExtractor.Body;
      var me = e as MemberExpression;
      if (me == null)
        throw new ArgumentException();

      string fieldName = null;
      while (e.NodeType == ExpressionType.MemberAccess) {
        TypeInfo type;
        me = (MemberExpression)e;
        e = me.Expression;
        if (Types.TryGetValue(me.Type, out type)) {
          if (type.IsEntity) {
            if (fieldName == null) {
              fieldName = me.Member.Name;
              continue;
            }
            return type.Fields[fieldName];
          }
        }
        if (fieldName == null)
          fieldName = me.Member.Name;
        else
          fieldName = me.Member.Name + "." + fieldName;
      }

      FieldInfo result = Types[typeof(T)].Fields[fieldName];
      return result;
    }
    
    public object GetUnlockKey()
    {
      this.EnsureNotLocked();
      return unlockKey;
    }
 
    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Services.Lock(true);
      Hierarchies.Lock(true);
      Generators.Lock(true);
      Types.Lock(true);
      RealIndexes.Lock(true);
      Associations.Lock(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel"/> class.
    /// </summary>
    public DomainModel()
    {
      Services = new ServiceInfoCollection();
      Types = new TypeInfoCollection();
      RealIndexes = new IndexInfoCollection();
      Hierarchies = new HierarchyInfoCollection();
      Associations = new AssociationInfoCollection();
      Generators = new GeneratorInfoCollection();
    }
  }
}