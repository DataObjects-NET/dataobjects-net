// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Sql.Dom.Resources;
using Log=Xtensive.Core.Log;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Default <see cref="ISqlComparer{T}"/> provider. 
  /// Provides default comparer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class SqlComparerProvider : AssociateProvider,
    ISqlComparerProvider
  {
    private static readonly SqlComparerProvider @default = new SqlComparerProvider();
    private ThreadSafeDictionary<Type, ISqlComparerBase> comparers = 
      ThreadSafeDictionary<Type, ISqlComparerBase>.Create(new object());
    private ThreadSafeCached<ISqlComparerBase> objectSqlComparer = ThreadSafeCached<ISqlComparerBase>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ISqlComparerProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region ISqlComparerProvider members

    /// <summary>
    /// Gets <see cref="ISqlComparer{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the SQL comparer for.</typeparam>
    /// <returns><see cref="ISqlComparer{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    public virtual SqlComparer<T> GetSqlComparer<T>()
    {
      return GetAssociate<T, ISqlComparer<T>, SqlComparer<T>>();
    }
   
    /// <inheritdoc/>
    public ISqlComparerBase GetSqlComparerByInstance(object value)
    {
      if (value == null)
        return objectSqlComparer.GetValue(
          @this => @this.GetSqlComparer<object>().Implementation, 
          this);
      else
        return GetSqlComparerByType(value.GetType());
    }

    /// <inheritdoc/>
    public ISqlComparerBase GetSqlComparerByType(Type type)
    {
      return comparers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetSqlComparerBase", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as ISqlComparerBase,
        this);
    }

    #endregion

    #region Protected method overrides

    // ReSharper disable UnusedPrivateMember
    private ISqlComparerBase InnerGetSqlComparerBase<T>() 
      where T : SchemaNode
    {
      return GetAssociate<T, ISqlComparer<T>, SqlComparer<T>>().Implementation;
    }
    // ReSharper restore UnusedPrivateMember

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult) (object) new SqlComparer<TKey>((ISqlComparer<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected SqlComparerProvider()
    {
      TypeSuffixes = new[] {"SqlComparer"};
      Type t = typeof (SqlComparerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}