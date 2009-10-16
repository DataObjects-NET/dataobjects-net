// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.15

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class PrefetchManyProcessor<TElement, TSelectorResult> : IEnumerable<TElement>
  {
    private class RootElementContainer
    {
      public TElement RootElement { get; set; }

      public int RemainingChildElementsCount { get; set; }
    }

    private readonly LinkedList<RootElementContainer> remainingCountOfChildElements =
      new LinkedList<RootElementContainer>();
    private readonly Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector;
    private readonly Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc;
    private readonly IEnumerable<TElement> source;
    private readonly SessionHandler sessionHandler;

    public IEnumerator<TElement> GetEnumerator()
    {
      TElement result;
      var prefetchedChildElements = prefetchManyFunc.Invoke(ExtractChildElements());
      foreach (var childElement in prefetchedChildElements) {
        if (remainingCountOfChildElements.Last.Value.RemainingChildElementsCount > 0)
          remainingCountOfChildElements.Last.Value.RemainingChildElementsCount--;
        else
          yield return RemoveLastElement();
        if (remainingCountOfChildElements.Last.Value.RemainingChildElementsCount==0
          && remainingCountOfChildElements.First!=remainingCountOfChildElements.Last) {
          yield return RemoveLastElement();
        }
      }
      while (remainingCountOfChildElements.Count > 0)
        yield return RemoveLastElement();
    }

    private TElement RemoveLastElement()
    {
      var result = remainingCountOfChildElements.Last.Value.RootElement;
      remainingCountOfChildElements.RemoveLast();
      return result;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private IEnumerable<TSelectorResult> ExtractChildElements()
    {
      foreach (var rootElement in source) {
        var currentNode = new LinkedListNode<RootElementContainer>(
          new RootElementContainer {RootElement = rootElement});
        remainingCountOfChildElements.AddFirst(currentNode);
        var childElements = childElementSelector.Invoke(rootElement, sessionHandler);
        foreach (var childElement in childElements) {
          currentNode.Value.RemainingChildElementsCount++;
          yield return childElement;
        }
      }
    }


    // Constructors

    public PrefetchManyProcessor(IEnumerable<TElement> source,
      Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc,
      SessionHandler sessionHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(childElementSelector, "childElementSelector");
      ArgumentValidator.EnsureArgumentNotNull(prefetchManyFunc, "prefetchManyFunc");
      ArgumentValidator.EnsureArgumentNotNull(sessionHandler, "sessionHandler");

      this.source = source;
      this.childElementSelector = childElementSelector;
      this.prefetchManyFunc = prefetchManyFunc;
      this.sessionHandler = sessionHandler;
    }
  }
}