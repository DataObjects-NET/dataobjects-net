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
  /// Default <see cref="INodeComparer{T}"/> provider. 
  /// Provides default comparer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class NodeComparerProvider : AssociateProvider,
    INodeComparerProvider
  {
    private static readonly NodeComparerProvider @default = new NodeComparerProvider();
    private ThreadSafeDictionary<Type, INodeComparerBase> comparers = 
      ThreadSafeDictionary<Type, INodeComparerBase>.Create(new object());
    private ThreadSafeCached<INodeComparerBase> objectNodeComparer = ThreadSafeCached<INodeComparerBase>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static INodeComparerProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region INodeComparerProvider members

    /// <summary>
    /// Gets <see cref="INodeComparer{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the SQL comparer for.</typeparam>
    /// <returns><see cref="INodeComparer{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    public virtual NodeComparer<T> GetNodeComparer<T>()
    {
      return GetAssociate<T, INodeComparer<T>, NodeComparer<T>>();
    }
   
    /// <inheritdoc/>
    public INodeComparerBase GetNodeComparerByInstance(object value)
    {
      if (value == null)
        return objectNodeComparer.GetValue(
          @this => @this.GetNodeComparer<object>().Implementation, 
          this);
      else
        return GetNodeComparerByType(value.GetType());
    }

    /// <inheritdoc/>
    public INodeComparerBase GetNodeComparerByType(Type type)
    {
      return comparers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetNodeComparerBase", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as INodeComparerBase,
        this);
    }

    #endregion

    #region Protected method overrides

    // ReSharper disable UnusedPrivateMember
    private INodeComparerBase InnerGetNodeComparerBase<T>() 
      where T : SchemaNode
    {
      return GetAssociate<T, INodeComparer<T>, NodeComparer<T>>().Implementation;
    }
    // ReSharper restore UnusedPrivateMember

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult) (object) new NodeComparer<TKey>((INodeComparer<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected NodeComparerProvider()
    {
      TypeSuffixes = new[] {"Comparer"};
      Type t = typeof (NodeComparerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}