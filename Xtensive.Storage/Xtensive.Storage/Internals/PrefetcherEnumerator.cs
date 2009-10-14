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
  internal sealed class PrefetcherEnumerator<T> : IEnumerator<T>
  {
    private static readonly object descriptorArraysCachingRegion = new object();

    private readonly Func<T, Key> keyExtractor;
    private readonly IEnumerator<T> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors;
    private readonly List<Func<T, SessionHandler, IEnumerable>> prefetchManyDelegates;
    private readonly Dictionary<TypeInfo, PrefetchFieldDescriptor[]> userDescriptorsCache =
      new Dictionary<TypeInfo, PrefetchFieldDescriptor[]>();
    private readonly Queue<T> processedElements = new Queue<T>();
    private readonly Queue<T> waitingElements = new Queue<T>();
    private StrongReferenceContainer strongReferenceContainer;
    private readonly Queue<T> delayedElements = new Queue<T>();
    private object blockingDelayedElement;
    private readonly SessionHandler sessionHandler;
    private int prefetchTaskExecutionCount;
    private PrefetcherEnumeratorState state;
    private bool isDisposed;

    public void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      source.Dispose();
    }

    public bool MoveNext()
    {
      EnsureNotDisposed();
      switch (state) {
        case PrefetcherEnumeratorState.EnumerationOfSource:
        if (EnumerateSource())
          return true;
        state = PrefetcherEnumeratorState.FlushingOfElementsProcessedDuringEnumeration;
        goto case PrefetcherEnumeratorState.FlushingOfElementsProcessedDuringEnumeration;

        case PrefetcherEnumeratorState.FlushingOfElementsProcessedDuringEnumeration:
        if (FlushElementsProcessedDuringEnumeration())
          return true;
        state = PrefetcherEnumeratorState.FlushingOfElementsFromTail;
        goto case PrefetcherEnumeratorState.FlushingOfElementsFromTail;

        case PrefetcherEnumeratorState.FlushingOfElementsFromTail:
        if (processedElements.Count > 0) {
          Current = processedElements.Dequeue();
          return true;
        }
        state = PrefetcherEnumeratorState.Finished;
        return false;

        default:
        return false;
      }
    }

    private bool FlushElementsProcessedDuringEnumeration()
    {
      if (processedElements.Count > 0) {
        Current = processedElements.Dequeue();
        return true;
      }
      strongReferenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
      ProcessFetchedElements(true);
      return false;
    }

    private bool EnumerateSource()
    {
      while (source.MoveNext()) {
        var element = source.Current;
        var elementKey = RegisterPrefetch(element);
        if (prefetchTaskExecutionCount!=sessionHandler.PrefetchTaskExecutionCount) {
          blockingDelayedElement = null;
          ProcessFetchedElements(false);
        }
        waitingElements.Enqueue(element);
        if (!elementKey.HasExactType)
          delayedElements.Enqueue(element);
        prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
        if (processedElements.Count > 0) {
          Current = processedElements.Dequeue();
          return true;
        }
      }
      return false;
    }

    public void Reset()
    {
      processedElements.Clear();
      waitingElements.Clear();
      delayedElements.Clear();
      blockingDelayedElement = null;
      prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
      strongReferenceContainer = new StrongReferenceContainer(null);
      state = PrefetcherEnumeratorState.EnumerationOfSource;
    }

    public T Current { get; private set; }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    private void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(GetType().Name);
    }

    private Key RegisterPrefetch(T element)
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

    private void FetchRemainingFieldsOfDelayedElements()
    {
      while (delayedElements.Count > 0) {
        var elementToBeFetched = delayedElements.Dequeue();
        if (blockingDelayedElement == null)
          blockingDelayedElement = elementToBeFetched;
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
        if (prefetchManyDelegates.Count > 0)
          ExecutePrefetchMany(waitingElement);
        processedElements.Enqueue(waitingElements.Dequeue());
      }
    }

    private void ExecutePrefetchTasks()
    {
      strongReferenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
      blockingDelayedElement = null;
    }

    private void ExecutePrefetchMany(T element)
    {
      foreach (var prefetchManyDelegate in prefetchManyDelegates) {
        var enumerator = prefetchManyDelegate.Invoke(element, sessionHandler).GetEnumerator();
        using ((IDisposable) enumerator)
          while (enumerator.MoveNext()) {
          }
      }
    }


    // Constructors

    public PrefetcherEnumerator(IEnumerable<T> source, Func<T, Key> keyExtractor, TypeInfo modelType,
      Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors,
      List<Func<T, SessionHandler, IEnumerable>> prefetchManyDelegates)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      ArgumentValidator.EnsureArgumentNotNull(fieldDescriptors, "fieldDescriptors");
      ArgumentValidator.EnsureArgumentNotNull(prefetchManyDelegates, "prefetchManyDelegates");
      this.source = source.GetEnumerator();
      this.keyExtractor = keyExtractor;
      this.modelType = modelType;
      this.fieldDescriptors = fieldDescriptors;
      this.prefetchManyDelegates = prefetchManyDelegates;
      sessionHandler = Session.Demand().Handler;
      Reset();
    }
  }
}