// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Manages of prefetch of <see cref="Entity"/>'s fields.
  /// </summary>
  /// <typeparam name="T">The type containing fields which can be registered for prefetch.</typeparam>
  /// <typeparam name="TElement">The type of the element.</typeparam>
  [Serializable]
  public sealed class Prefetcher<T, TElement> : IEnumerable<TElement>
    where T : Entity
  {
    private readonly Func<TElement, Key> keyExtractor;
    private readonly IEnumerable<TElement> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors =
      new Dictionary<FieldInfo, PrefetchFieldDescriptor>();
    private readonly List<Func<TElement, SessionHandler, IEnumerable>> prefetchManyDelegates =
      new List<Func<TElement, SessionHandler, IEnumerable>>();
    private readonly Queue<TElement> processedElements = new Queue<TElement>();
    private readonly List<TElement> waitingElements = new List<TElement>();

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression)
    {
      Prefetch(expression, null);
      return this;
    }

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{TItem}"/>.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression,
      int entitySetItemCountLimit)
    {
      Prefetch(expression, (int?) entitySetItemCountLimit);
      return this;
    }

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> PrefetchMany<TFieldValue, TSelectorResult>(
      Expression<Func<T, TFieldValue>> expression,
      Func<TFieldValue,IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
    {
      PrefetchMany(expression, null, selector, prefetchManyFunc);
      return this;
    }

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{TItem}"/>.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> PrefetchMany<TFieldValue, TSelectorResult>(
      Expression<Func<T, TFieldValue>> expression, int entitySetItemCountLimit,
      Func<TFieldValue, IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
    {
      PrefetchMany(expression, (int?) entitySetItemCountLimit, selector, prefetchManyFunc);
      return this;
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source, 
    /// registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    /// <remarks>This methods allows to select value of type which does not implement 
    /// <see cref="IEnumerable{T}"/>. The result of the <paramref name="selector"/> is transformed 
    /// to <see cref="IEnumerable{T}"/> by <see cref="EnumerableUtils.One{TItem}"/>.</remarks>
    public Prefetcher<T, TElement> PrefetchSingle<TFieldValue, TSelectorResult>(
      Expression<Func<T, TFieldValue>> expression, Func<TFieldValue,TSelectorResult> selector,
      Func<IEnumerable<TSelectorResult>,IEnumerable<TSelectorResult>> prefetchManyFunc)
      where TSelectorResult : Entity
    {
      Prefetch(expression, null);
      var compiledPropertyGetter = expression.CachingCompile();
      Func<TElement, SessionHandler, IEnumerable> prefetchManyDelegate =
        (element, sessionHandler) => {
          EntityState state;
          sessionHandler.TryGetEntityState(keyExtractor.Invoke(element), out state);
          return prefetchManyFunc.Invoke(EnumerableUtils.One(selector
            .Invoke(compiledPropertyGetter.Invoke((T) state.Entity))));
        };
      prefetchManyDelegates.Add(prefetchManyDelegate);
      return this;
    }
    
    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
      try {
        var cachedFieldDescriptors = fieldDescriptors.Select(pair => pair.Value).ToArray();
        var sessionHandler = Session.Demand().Handler;
        foreach (var element in source) {
          var prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
          sessionHandler.Prefetch(keyExtractor.Invoke(element), modelType, cachedFieldDescriptors);
          if (prefetchTaskExecutionCount != sessionHandler.PrefetchTaskExecutionCount)
            ProcessFetchedElements(element, sessionHandler);
          else
            waitingElements.Add(element);
          if (processedElements.Count > 0)
            yield return processedElements.Dequeue();
        }
        while (processedElements.Count > 0)
          yield return processedElements.Dequeue();
        sessionHandler.ExecutePrefetchTasks();
        foreach (var element in waitingElements)
          yield return element;
      }
      finally {
        processedElements.Clear();
        waitingElements.Clear();
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private \ internal methods

    private void PrefetchMany<TFieldValue, TSelectorResult>(
      Expression<Func<T, TFieldValue>> expression, int? entitySetItemCountLimit,
      Func<TFieldValue, IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
    {
      Prefetch(expression, entitySetItemCountLimit);
      var compiledPropertyGetter = expression.CachingCompile();
      Func<TElement, SessionHandler, IEnumerable> prefetchManyDelegate =
        (element, sessionHandler) => {
          EntityState state;
          sessionHandler.TryGetEntityState(keyExtractor.Invoke(element), out state);
          return prefetchManyFunc.Invoke(selector
            .Invoke(compiledPropertyGetter.Invoke((T) state.Entity)));
        };
      prefetchManyDelegates.Add(prefetchManyDelegate);
    }

    private void Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression, int? entitySetItemCountLimit)
    {
      var body = expression.Body;
      if (body.NodeType != ExpressionType.MemberAccess)
        throw new ArgumentException("The specified expression is not MemberExpression", "expression");
      var memberAccess = (MemberExpression) body;
      if (memberAccess.Expression != expression.Parameters[0])
        throw new ArgumentException("The access to a type's member can not be extracted from the specified expression.", "expression");
      var property = memberAccess.Member as PropertyInfo;
      if (property == null)
        throw new ArgumentException("The accessed type's member is not a property.", "expression");
      var modelField = modelType.Fields.Where(field => field.UnderlyingProperty.Equals(property))
        .SingleOrDefault();
      if (modelField == null)
        throw new ArgumentException(String.Format("The specified property {0} is not persistent.",
          property.Name), "expression");
      if (fieldDescriptors.ContainsKey(modelField))
        throw new InvalidOperationException(String
          .Format("The field {0} has been already registered for the prefetch.", modelField));
      fieldDescriptors.Add(modelField, new PrefetchFieldDescriptor(modelField, entitySetItemCountLimit));
    }

    private void ProcessFetchedElements(TElement currentElement, SessionHandler sessionHandler)
    {
      var prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
      if (prefetchManyDelegates.Count > 0)
        foreach (var waitingElement in waitingElements)
          prefetchTaskExecutionCount = ExecutePrefetchMany(waitingElement, sessionHandler);
      foreach (var waitingElement in waitingElements)
        processedElements.Enqueue(waitingElement);
      waitingElements.Clear();
      if (prefetchTaskExecutionCount != sessionHandler.PrefetchTaskExecutionCount) {
        if (prefetchManyDelegates.Count > 0)
          ExecutePrefetchMany(currentElement, sessionHandler);
        processedElements.Enqueue(currentElement);
      }
      else
        waitingElements.Add(currentElement);
    }

    private int ExecutePrefetchMany(TElement element, SessionHandler sessionHandler)
    {
      var result = sessionHandler.PrefetchTaskExecutionCount;
      foreach (var prefetchManyDelegate in prefetchManyDelegates) {
        var enumerator = prefetchManyDelegate.Invoke(element, sessionHandler).GetEnumerator();
        using ((IDisposable) enumerator)
          while (enumerator.MoveNext()) {
          }
      }
      return result;
    }

    #endregion


    // Constructors

    internal Prefetcher(IEnumerable<TElement> source, Func<TElement, Key> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.source = source;
      this.keyExtractor = keyExtractor;
      modelType = typeof (T).GetTypeInfo();
    }
  }
}