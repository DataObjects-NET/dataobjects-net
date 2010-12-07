// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal sealed class RootElementsPrefetcher<T> : IEnumerable<T>
  {
    private readonly Func<T, Key> keyExtractor;
    private readonly IEnumerable<T> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors;
    private readonly Dictionary<TypeInfo, FieldDescriptorCollection> userDescriptorsCache =
      new Dictionary<TypeInfo, FieldDescriptorCollection>();
    private readonly Queue<T> processedElements = new Queue<T>();
    private readonly Queue<Pair<Key, T>> waitingElements = new Queue<Pair<Key, T>>();
    private readonly StrongReferenceContainer strongReferenceContainer;
    private readonly Queue<Triplet<Key, T, TypeInfo>> delayedElements = new Queue<Triplet<Key, T, TypeInfo>>();
    private Dictionary<Key, LinkedList<Pair<Key, TypeInfo>>> referencedDelayedElementKeys;
    private readonly Dictionary<Key, LinkedList<Pair<Key, TypeInfo>>> referencedDelayedElementKeysFirst =
      new Dictionary<Key, LinkedList<Pair<Key, TypeInfo>>>();
    private readonly Dictionary<Key, LinkedList<Pair<Key, TypeInfo>>> referencedDelayedElementKeysSecond =
      new Dictionary<Key, LinkedList<Pair<Key, TypeInfo>>>();

    private Key blockingDelayedElement;
    private readonly SessionHandler sessionHandler;
    private int prefetchTaskExecutionCount;
    private bool isDisposed;

    private IEnumerable<T> GetItems()
    {
      if (source==null)
        yield break;
      prefetchTaskExecutionCount = sessionHandler.PrefetchTaskExecutionCount;
      foreach (var element in source) {
        if (element==null) {
          yield return element;
          continue;
        }
        var key = keyExtractor.Invoke(element);
        var type = RegisterPrefetch(element, key);
        if (prefetchTaskExecutionCount!=sessionHandler.PrefetchTaskExecutionCount) {
          blockingDelayedElement = null;
          ProcessFetchedElements(false);
        }
        var pair = new Pair<Key, T>(key, element);
        waitingElements.Enqueue(pair);
        if (!key.HasExactType)
          delayedElements.Enqueue(new Triplet<Key, T, TypeInfo>(key, element, type));
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

    public IEnumerator<T> GetEnumerator()
    {
      foreach (var pair in GetItems())
        yield return pair;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private \ internal methods

    private TypeInfo RegisterPrefetch(T element, Key key)
    {
      var type = SelectType(key);
      var descriptors = GetUserDescriptors(type);
      strongReferenceContainer.JoinIfPossible(sessionHandler
        .Prefetch(keyExtractor.Invoke(element), type, descriptors));
      return type;
    }

    private TypeInfo SelectType(Key key)
    {
      TypeInfo type;
      if (key.HasExactType)
        type = key.TypeReference.Type;
      else if (modelType==null)
        type = key.TypeReference.Type;
      else
        type = modelType;
      return type;
    }

    private FieldDescriptorCollection GetUserDescriptors(TypeInfo type)
    {
      FieldDescriptorCollection result;
      if (!userDescriptorsCache.TryGetValue(type, out result)) {
        result = new FieldDescriptorCollection(
          PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(sessionHandler.Session.Domain, type)
          .Concat(fieldDescriptors.Values));
        userDescriptorsCache[type] = result;
      }
      return result;
    }

    private void FetchRemainingFieldsOfDelayedElements()
    {
      while (delayedElements.Count > 0) {
        var elementToBeFetched = delayedElements.Dequeue();
        if (blockingDelayedElement == null)
          blockingDelayedElement = elementToBeFetched.First;
        var prefetchTaskCount = sessionHandler.PrefetchTaskExecutionCount;
        LinkedList<Pair<Key, TypeInfo>> referencedKeys;
        if (referencedDelayedElementKeys.TryGetValue(elementToBeFetched.First, out referencedKeys)) {
          RegisterPrefetchOfDelayedReferencedElements(referencedKeys);
          referencedDelayedElementKeys.Remove(elementToBeFetched.First);
        }
        RegisterPrefetchOfDefaultFields(elementToBeFetched.First, elementToBeFetched.Third);
        if (prefetchTaskCount != sessionHandler.PrefetchTaskExecutionCount)
          blockingDelayedElement = elementToBeFetched.First;
      }
    }

    private void RegisterPrefetchOfDelayedReferencedElements(LinkedList<Pair<Key, TypeInfo>> referencedKeys)
    {
      foreach (var referencedKeyPair in referencedKeys)
        RegisterPrefetchOfDefaultFields(referencedKeyPair.First, referencedKeyPair.Second);
    }

    private void FetchRemainingFieldsOfReferencedElements()
    {
      if (referencedDelayedElementKeys.Count == 0)
        return;
      var currentCollection = referencedDelayedElementKeys;
      referencedDelayedElementKeys = referencedDelayedElementKeys == referencedDelayedElementKeysFirst
        ? referencedDelayedElementKeysSecond
        : referencedDelayedElementKeysFirst;
      foreach (var pair in currentCollection) {
        if (blockingDelayedElement==null)
          blockingDelayedElement = pair.Key;
        var prefetchTaskCount = sessionHandler.PrefetchTaskExecutionCount;
        RegisterPrefetchOfDelayedReferencedElements(pair.Value);
        if (prefetchTaskCount!=sessionHandler.PrefetchTaskExecutionCount)
          blockingDelayedElement = pair.Key;
      }
      currentCollection.Clear();
    }

    private void RegisterPrefetchOfDefaultFields(Key key, TypeInfo queriedType)
    {
      var keyWithType = key;
      if (!key.HasExactType)
          keyWithType = sessionHandler.Session.EntityStateCache[keyWithType, false].Key;
      if (!keyWithType.HasExactType || keyWithType.TypeReference.Type == queriedType)
        return;
      var descriptorsArray = PrefetchHelper
        .GetCachedDescriptorsForFieldsLoadedByDefault(sessionHandler.Session.Domain, keyWithType.TypeReference.Type);
      strongReferenceContainer.JoinIfPossible(sessionHandler.Prefetch(keyWithType, keyWithType.TypeReference.Type,
        descriptorsArray));
    }

    private void ProcessFetchedElements(bool forceTasksExecution)
    {
      while (waitingElements.Count > 0) {
        var waitingElement = waitingElements.Peek();
        if (waitingElement.First.Equals(blockingDelayedElement))
          if (forceTasksExecution)
            ExecutePrefetchTasks();
          else
            return;
        if (delayedElements.Count > 0 && waitingElement.First.Equals(delayedElements.Peek().First))
          FetchRemainingFieldsOfDelayedElements();
        else if (referencedDelayedElementKeys.Count > 0
          && referencedDelayedElementKeys.ContainsKey(waitingElement.First))
          FetchRemainingFieldsOfReferencedElements();
        if (forceTasksExecution)
          ExecutePrefetchTasks();
        if (waitingElement.First.Equals(blockingDelayedElement))
          return;
        processedElements.Enqueue(waitingElements.Dequeue().Second);
      }
    }

    private void ExecutePrefetchTasks()
    {
      strongReferenceContainer.JoinIfPossible(sessionHandler.ExecutePrefetchTasks());
      blockingDelayedElement = null;
    }

    private void HandleReferencedKeyExtraction(Key ownerKey, FieldInfo referencingField, Key referencedKey)
    {
      TypeInfo type;
      if (referencedKey.HasExactType)
        type = referencedKey.TypeInfo;
      else
        type = referencingField.IsEntitySet
          ? sessionHandler.Session.Domain.Model.Types[referencingField.ItemType]
          : referencingField.Associations.Last().TargetType;
      GetReferencedKeysList(ownerKey).AddLast(new Pair<Key, TypeInfo>(referencedKey, type));
    }

    private LinkedList<Pair<Key, TypeInfo>> GetReferencedKeysList(Key ownerKey)
    {
      LinkedList<Pair<Key, TypeInfo>> result;
      if (!referencedDelayedElementKeys.TryGetValue(ownerKey, out result)) {
        result = new LinkedList<Pair<Key, TypeInfo>>();
        referencedDelayedElementKeys.Add(ownerKey, result);
      }
      return result;
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
      this.fieldDescriptors = new Dictionary<FieldInfo, PrefetchFieldDescriptor>(fieldDescriptors.Count);
      Action<Key, FieldInfo, Key> keyExtractionSubscriber = HandleReferencedKeyExtraction;
      foreach (var pair in fieldDescriptors)
        this.fieldDescriptors[pair.Key] = new PrefetchFieldDescriptor(pair.Value.Field,
          pair.Value.EntitySetItemCountLimit, pair.Value.FetchFieldsOfReferencedEntity,
          pair.Value.FetchLazyFields, keyExtractionSubscriber);
      this.sessionHandler = sessionHandler;
      strongReferenceContainer = new StrongReferenceContainer(null);
      referencedDelayedElementKeys = referencedDelayedElementKeysFirst;
    }
  }
}