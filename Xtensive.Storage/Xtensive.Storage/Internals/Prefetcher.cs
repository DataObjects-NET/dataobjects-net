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
using Xtensive.Storage.Resources;

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
    private static readonly object descriptorArraysCachingRegion = new object();

    private readonly Func<TElement, Key> keyExtractor;
    private readonly IEnumerable<TElement> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors =
      new Dictionary<FieldInfo, PrefetchFieldDescriptor>();
    private readonly List<Func<TElement, SessionHandler, IEnumerable>> prefetchManyDelegates =
      new List<Func<TElement, SessionHandler, IEnumerable>>();
    private readonly Dictionary<TypeInfo, PrefetchFieldDescriptor[]> userDescriptorsCache =
      new Dictionary<TypeInfo, PrefetchFieldDescriptor[]>();

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
      var processedElements = new Queue<TElement>();
      var waitingElements = new Queue<TElement>();
      var referenceContainer = new StrongReferenceContainer(null);
      var elementsHavingKeyWithUnknownType = new List<TElement>();
      try {
        var sessionHandler = Session.Demand().Handler;
        var prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
        foreach (var element in source) {
          var elementKey = RegisterPrefetch(element, sessionHandler, ref referenceContainer);
          if (prefetchTaskExecutionCount != sessionHandler.PrefetchTaskExecutionCount) {
            ProcessFetchedElements(processedElements, waitingElements, elementsHavingKeyWithUnknownType,
              sessionHandler, referenceContainer, false);
          }
          waitingElements.Enqueue(element);
          if (!elementKey.HasExactType)
            elementsHavingKeyWithUnknownType.Add(element);
          prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
          if (processedElements.Count > 0)
            yield return processedElements.Dequeue();
        }
        while (processedElements.Count > 0)
          yield return processedElements.Dequeue();
        referenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
        ProcessFetchedElements(processedElements, waitingElements, elementsHavingKeyWithUnknownType,
          sessionHandler, referenceContainer, true);
        while (processedElements.Count > 0)
          yield return processedElements.Dequeue();
        while (waitingElements.Count > 0)
          yield return waitingElements.Dequeue();
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

    private void Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression,
      int? entitySetItemCountLimit)
    {
      var body = expression.Body;
      if (body.NodeType != ExpressionType.MemberAccess)
        throw new ArgumentException(Strings.ExSpecifiedExpressionIsNotMemberExpression, "expression");
      var memberAccess = (MemberExpression) body;
      if (memberAccess.Expression != expression.Parameters[0])
        throw new ArgumentException(Strings.ExAccessToTypeMemberCanNotBeExtractedFromSpecifiedExpression,
          "expression");
      var property = memberAccess.Member as PropertyInfo;
      if (property == null)
        throw new ArgumentException(Strings.ExAccessedMemberIsNotProperty, "expression");
      var modelField = modelType.Fields.Where(field => field.UnderlyingProperty.Equals(property))
        .SingleOrDefault();
      if (modelField == null)
        throw new ArgumentException(String.Format(Strings.ExSpecifiedPropertyXIsNotPersistent,
          property.Name), "expression");
      fieldDescriptors[modelField] = new PrefetchFieldDescriptor(modelField, entitySetItemCountLimit, true);
    }

    private void FetchRemainingFieldsOfDelayedElements(ICollection<TElement> elementsHavingKeyWithUnknownType,
      StrongReferenceContainer referenceContainer, SessionHandler sessionHandler)
    {
      foreach (var elementToBeFetched in elementsHavingKeyWithUnknownType) {
        var key = keyExtractor.Invoke(elementToBeFetched);
        var type = key.TypeRef.Type;
        var cachedKey = sessionHandler.Session.EntityStateCache[key, false].Key;
        var descriptorsArray = (PrefetchFieldDescriptor[]) sessionHandler.Session.Domain
          .GetCachedItem(new Pair<object, TypeInfo>(descriptorArraysCachingRegion, cachedKey.Type),
            pair => ((Pair<object, TypeInfo>) pair).Second.Fields
              .Where(field => field.DeclaringType!=type
                && field.Parent==null && PrefetchTask.IsFieldToBeLoadedByDefault(field))
              .Select(field => new PrefetchFieldDescriptor(field, false)).ToArray());
        var prefetchTaskCount = sessionHandler.PrefetchTaskExecutionCount;
        referenceContainer.JoinIfPossible(sessionHandler.Prefetch(cachedKey, cachedKey.Type, descriptorsArray));
      }
      elementsHavingKeyWithUnknownType.Clear();
    }

    private Key RegisterPrefetch(TElement element, SessionHandler sessionHandler,
      ref StrongReferenceContainer strongReferenceContainer)
    {
      var key = keyExtractor.Invoke(element);
      TypeInfo type;
      if (key.HasExactType)
        type = key.Type;
      else if (modelType==null)
        type = key.TypeRef.Type;
      else
        type = modelType;
      var descriptorArray = GetUserDescriptorArray(type);
      strongReferenceContainer.JoinIfPossible(sessionHandler
        .Prefetch(keyExtractor.Invoke(element), type, descriptorArray));
      return key;
    }

    private PrefetchFieldDescriptor[] GetUserDescriptorArray(TypeInfo type)
    {
      PrefetchFieldDescriptor[] result;
      if (!userDescriptorsCache.TryGetValue(type, out result)) {
        result = type.Fields
          .Where(field => field.Parent==null && PrefetchTask.IsFieldToBeLoadedByDefault(field)
            && !fieldDescriptors.ContainsKey(field))
          .Select(field => new PrefetchFieldDescriptor(field, false))
          .Concat(fieldDescriptors.Values).ToArray();
        userDescriptorsCache[type] = result;
      }
      return result;
    }

    private void ProcessFetchedElements(Queue<TElement> processedElements, Queue<TElement> waitingElements,
      List<TElement> delayedElements, SessionHandler sessionHandler,
      StrongReferenceContainer referenceContainer, bool forceTasksExecution)
    {
      while (waitingElements.Count > 1 || waitingElements.Count==1 && forceTasksExecution) {
        var waitingElement = waitingElements.Peek();
        if (delayedElements.Count > 0 && ReferenceEquals(waitingElement, delayedElements[0]))
          FetchRemainingFieldsOfDelayedElements(delayedElements, referenceContainer, sessionHandler);
        if (waitingElements.Count==1 && forceTasksExecution)
          referenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
        if (prefetchManyDelegates.Count > 0) {
          var prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
          ExecutePrefetchMany(waitingElement, sessionHandler);
        }
        processedElements.Enqueue(waitingElements.Dequeue());
      }
    }

    private void ExecutePrefetchMany(TElement element, SessionHandler sessionHandler)
    {
      foreach (var prefetchManyDelegate in prefetchManyDelegates) {
        var enumerator = prefetchManyDelegate.Invoke(element, sessionHandler).GetEnumerator();
        using ((IDisposable) enumerator)
          while (enumerator.MoveNext()) {
          }
      }
    }

    #endregion


    // Constructors

    internal Prefetcher(IEnumerable<TElement> source, Func<TElement, Key> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.source = source;
      this.keyExtractor = keyExtractor;
      modelType = typeof (T) != typeof (Entity) ? typeof (T).GetTypeInfo() : null;
    }
  }
}