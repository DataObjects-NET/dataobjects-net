// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class RootElementsPrefetcher<T> : IEnumerable<T>
  {
    private static readonly object descriptorArraysCachingRegion = new object();

    private readonly Func<T, Key> keyExtractor;
    private readonly IEnumerable<T> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors;
    private readonly Dictionary<TypeInfo, PrefetchFieldDescriptor[]> userDescriptorsCache =
      new Dictionary<TypeInfo, PrefetchFieldDescriptor[]>();
    private readonly Queue<T> processedElements = new Queue<T>();
    private readonly Queue<T> waitingElements = new Queue<T>();
    private readonly StrongReferenceContainer strongReferenceContainer;
    private readonly Queue<T> delayedElements = new Queue<T>();
    private object blockingDelayedElement;
    private readonly SessionHandler sessionHandler;
    private int prefetchTaskExecutionCount;
    private bool isDisposed;

    public IEnumerator<T> GetEnumerator()
    {
      prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
      foreach (var element in source) {
        var elementKey = RegisterPrefetch(element);
        if (prefetchTaskExecutionCount!=sessionHandler.PrefetchTaskExecutionCount) {
          blockingDelayedElement = null;
          ProcessFetchedElements(false);
        }
        waitingElements.Enqueue(element);
        if (!elementKey.HasExactType)
          delayedElements.Enqueue(element);
        prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
        if (processedElements.Count > 0)
          yield return processedElements.Dequeue();
      }
      while (processedElements.Count > 0)
        yield return processedElements.Dequeue();
      strongReferenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
      ProcessFetchedElements(true);
      while (processedElements.Count > 0)
        yield return processedElements.Dequeue();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private \ internal methods

    private Key RegisterPrefetch(T element)
    {
      var key = keyExtractor.Invoke(element);
      var type = SelectType(key);
      var descriptorArray = GetUserDescriptorArray(type);
      strongReferenceContainer.JoinIfPossible(sessionHandler
        .Prefetch(keyExtractor.Invoke(element), type, descriptorArray));
      return key;
    }

    private TypeInfo SelectType(Key key)
    {
      TypeInfo type;
      if (key.HasExactType)
        type = key.Type;
      else if (modelType==null)
        type = key.TypeRef.Type;
      else
        type = modelType;
      return type;
    }

    private PrefetchFieldDescriptor[] GetUserDescriptorArray(TypeInfo type)
    {
      PrefetchFieldDescriptor[] result;
      if (!userDescriptorsCache.TryGetValue(type, out result)) {
        result = type.Fields
          .Where(field => field.Parent==null && PrefetchHelper.IsFieldToBeLoadedByDefault(field)
            && !fieldDescriptors.ContainsKey(field))
          .Select(field => new PrefetchFieldDescriptor(field, false))
          .Concat(fieldDescriptors.Values).ToArray();
        userDescriptorsCache[type] = result;
      }
      return result;
    }

    private void FetchRemainingFieldsOfDelayedElements()
    {
      while (delayedElements.Count > 0) {
        var elementToBeFetched = delayedElements.Dequeue();
        if (blockingDelayedElement == null)
          blockingDelayedElement = elementToBeFetched;
        var key = keyExtractor.Invoke(elementToBeFetched);
        var cachedKey = sessionHandler.Session.EntityStateCache[key, false].Key;
        var descriptorsArray = (PrefetchFieldDescriptor[]) sessionHandler.Session.Domain
          .GetCachedItem(new Pair<object, TypeInfo>(descriptorArraysCachingRegion, cachedKey.Type),
            pair => PrefetchHelper
              .CreateDescriptorsForFieldsLoadedByDefault(((Pair<object, TypeInfo>) pair).Second));
        var prefetchTaskCount = sessionHandler.PrefetchTaskExecutionCount;
        strongReferenceContainer.JoinIfPossible(sessionHandler.Prefetch(cachedKey, cachedKey.Type,
          descriptorsArray));
        if (prefetchTaskCount != sessionHandler.PrefetchTaskExecutionCount)
          blockingDelayedElement = elementToBeFetched;
      }
    }

    private void ProcessFetchedElements(bool forceTasksExecution)
    {
      while (waitingElements.Count > 0) {
        var waitingElement = waitingElements.Peek();
        if (ReferenceEquals(waitingElement, blockingDelayedElement))
          if (forceTasksExecution)
            ExecutePrefetchTasks();
          else
            return;
        if (delayedElements.Count > 0 && ReferenceEquals(waitingElement, delayedElements.Peek()))
          FetchRemainingFieldsOfDelayedElements();
        if (forceTasksExecution)
          ExecutePrefetchTasks();
        if (ReferenceEquals(waitingElement, blockingDelayedElement))
          return;
        processedElements.Enqueue(waitingElements.Dequeue());
      }
    }

    private void ExecutePrefetchTasks()
    {
      strongReferenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
      blockingDelayedElement = null;
    }

    #endregion


    // Constructors

    public RootElementsPrefetcher(IEnumerable<T> source, Func<T, Key> keyExtractor, TypeInfo modelType,
      Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors, SessionHandler sessionHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      ArgumentValidator.EnsureArgumentNotNull(fieldDescriptors, "fieldDescriptors");
      ArgumentValidator.EnsureArgumentNotNull(sessionHandler, "sessionHandler");
      this.source = source;
      this.keyExtractor = keyExtractor;
      this.modelType = modelType;
      this.fieldDescriptors = fieldDescriptors;
      this.sessionHandler = sessionHandler;
      strongReferenceContainer = new StrongReferenceContainer(null);
    }
  }
}