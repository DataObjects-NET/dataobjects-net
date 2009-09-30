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
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class Prefetcher<TItem, TElement> : IEnumerable<TElement>
  {
    private readonly Func<TElement, Key> keyExtractor;
    private readonly IEnumerable<TElement> source;
    private readonly TypeInfo modelType;
    private readonly List<PrefetchFieldDescriptor> fieldDescriptors = new List<PrefetchFieldDescriptor>();
    private readonly Queue<TElement> processedElements = new Queue<TElement>();

    public Prefetcher<TItem, TElement> Prefetch<TResult>(Expression<Func<TItem, TResult>> expression)
    {
      Prefetch(expression, null);
      return this;
    }

    public Prefetcher<TItem, TElement> Prefetch<TResult>(Expression<Func<TItem, TResult>> expression,
      int entitySetItemCountLimit)
    {
      Prefetch(expression, (int?) entitySetItemCountLimit);
      return this;
    }

    private void Prefetch<TResult>(Expression<Func<TItem, TResult>> expression, int? entitySetItemCountLimit)
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
      var modelField = modelType.Fields.Where(field => field.UnderlyingProperty.Equals(property)).Single();
      fieldDescriptors.Add(new PrefetchFieldDescriptor(modelField, entitySetItemCountLimit));
    }

    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
      var cachedFieldDescriptors = fieldDescriptors.ToArray();
      var sessionHandler = Session.Demand().Handler;
      var waitingElements = new List<TElement>();
      foreach (var element in source) {
        sessionHandler.Prefetch(keyExtractor.Invoke(element), modelType, cachedFieldDescriptors);
        if (sessionHandler.IsPrefetchAutoExecutionOccured) {
          foreach (var waitingElement in waitingElements)
            processedElements.Enqueue(waitingElement);
          waitingElements.Clear();
        }
        if (processedElements.Count > 0)
          yield return processedElements.Dequeue();
      }
      while (processedElements.Count > 0)
        yield return processedElements.Dequeue();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    internal Prefetcher(IEnumerable<TElement> source, Func<TElement, Key> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.source = source;
      this.keyExtractor = keyExtractor;
      modelType = typeof (TElement).GetTypeInfo();
    }
  }
}