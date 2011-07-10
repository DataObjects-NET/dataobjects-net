// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Linq;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// Defines a permission for a persistent type that is represented by <see cref="Permission.Type"/> property.
  /// </summary>
  public abstract class Permission
  {
    /// <summary>
    /// Gets or sets the type of entity.
    /// </summary>
    /// <value>The type of entity.</value>
    public Type Type { get; private set; }

    /// <summary>
    /// Specifies whether a permission is given to read entities of <see cref="Permission.Type"/>.
    /// </summary>
    public bool CanRead { get; private set; }

    /// <summary>
    /// Specifies whether a permission is given to write entities of <see cref="Permission.Type"/>.
    /// </summary>
    public bool CanWrite { get; private set; }

    /// <summary>
    /// Gets or sets the query that filters entities of <see cref="Permission.Type"/>.
    /// </summary>
    public Func<ImpersonationContext, QueryEndpoint, IQueryable<IEntity>> Query { get; protected set; }

    #region GetHashcode & Equals members

    /// <inheritdoc/>
    public bool Equals(Permission other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(other.Type, Type) && other.CanRead.Equals(CanRead) && other.CanWrite.Equals(CanWrite) && Equals(other.Query, Query);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (Permission)) return false;
      return Equals((Permission) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Type != null ? Type.GetHashCode() : 0);
        result = (result*397) ^ CanRead.GetHashCode();
        result = (result*397) ^ CanWrite.GetHashCode();
        result = (result*397) ^ (Query != null ? Query.GetHashCode() : 0);
        return result;
      }
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class.
    /// </summary>
    /// <param name="type">The type of entity.</param>
    /// <param name="canWrite">Specifies whether permission is given or not to write entities of <see cref="Permission.Type"/> type.</param>
    protected Permission(Type type, bool canWrite)
    {
      Type = type;
      CanRead = true;
      CanWrite = canWrite;
    }
  }

  /// <summary>
  /// Defines a permission for a persistent type that is represented by <see cref="Permission.Type"/> property.
  /// </summary>
  /// <typeparam name="T">Type of entity</typeparam>
  public class Permission<T> : Permission where T : class, IEntity
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Permission&lt;T&gt;"/> class.
    /// </summary>
    public Permission()
      : this(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="canWrite">Specifies whether permission is given or not to write entities of <typeparamref name="T"/> type.</param>
    public Permission(bool canWrite)
      : base(typeof(T), canWrite)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="query">The query that filters entities of <typeparamref name="T"/> type.</param>
    public Permission(Func<ImpersonationContext, QueryEndpoint, IQueryable<T>> query)
      : this(false, query)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="canWrite">Specifies whether permission is given or not to write entities of <typeparamref name="T"/> type.</param>
    /// <param name="query">The query that filters entities of <typeparamref name="T"/> type.</param>
    public Permission(bool canWrite, Func<ImpersonationContext, QueryEndpoint, IQueryable<T>> query)
      : base(typeof(T), canWrite)
    {
      Query = query;
    }
  }
}
