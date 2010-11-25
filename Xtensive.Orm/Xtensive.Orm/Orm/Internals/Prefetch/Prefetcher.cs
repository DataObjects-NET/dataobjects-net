// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Internals.Prefetch
{
  /// <summary>
  /// Manages of prefetch of <see cref="Entity"/>'s fields.
  /// </summary>
  /// <typeparam name="T">The type containing fields which can be registered for prefetch.</typeparam>
  /// <typeparam name="TElement">The type of the element.</typeparam>
  [Serializable]
  public sealed class Prefetcher<T, TElement> : IEnumerable<TElement>
    where T : IEntity
  {
    private static readonly object descriptorArraysCachingRegion = new object();

    private readonly Func<TElement, Key> keyExtractor;
    private readonly IEnumerable<TElement> source;
    private readonly TypeInfo modelType;
    private readonly Dictionary<FieldInfo, PrefetchFieldDescriptor> fieldDescriptors =
      new Dictionary<FieldInfo, PrefetchFieldDescriptor>();
    private readonly List<Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>>>
      prefetchManyProcessorCreators =
        new List<Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>>>();
    private object blockingDelayedElement;
    private readonly Session session;

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
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
    /// <typeparam name="TItem">The type of a sequence item.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{TItem}"/>.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> Prefetch<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression,
      int entitySetItemCountLimit)
    {
      Prefetch(expression, (int?)entitySetItemCountLimit);
      return this;
    }

    /// <summary>
    /// Registers the prefetch of the field specified by <paramref name="expression"/> and
    /// registers the delegate prefetching fields of an object referenced by element of
    /// the sequence.
    /// </summary>
    /// <typeparam name="TItem">The type of a sequence item.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> Prefetch<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression,
      Func<IEnumerable<TItem>, IEnumerable<TItem>> nestedPrefetcher)
    {
      Prefetch(expression, (int?) null);
      RegisterPrefetchMany(expression.Compile(), nestedPrefetcher);
      return this;
    }

    /// <summary>
    /// Registers the delegate prefetching fields of an object referenced by element of
    /// the sequence specified by <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="selector">The selector of a sequence.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    public Prefetcher<T, TElement> PrefetchMany<TSelectorResult>(
      Func<T,IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> nestedPrefetcher)
    {
      RegisterPrefetchMany(selector, nestedPrefetcher);
      return this;
    }

    /// <summary>
    /// Registers the delegate prefetching fields of an object referenced by the value
    /// specified by <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="selector">The selector of a field value.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    /// <remarks>This methods allows to select value of type which does not implement 
    /// <see cref="IEnumerable{T}"/>. The result of the <paramref name="selector"/> is transformed 
    /// to <see cref="IEnumerable{T}"/> by <see cref="EnumerableUtils.One{TItem}"/>.</remarks>
    public Prefetcher<T, TElement> PrefetchSingle<TSelectorResult>(
      Func<T,TSelectorResult> selector,
      Func<IEnumerable<TSelectorResult>,IEnumerable<TSelectorResult>> nestedPrefetcher)
      where TSelectorResult : IEntity
    {
      Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>> prefetchManyDelegate =
        (rootEnumerator, sessionHandler) => {
          Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector =
            (element, sh) => SelectChildElements(element,
              fieldValue => EnumerableUtils.One(selector.Invoke(fieldValue)), sh);
          return new PrefetchManyProcessor<TElement, TSelectorResult>(rootEnumerator, childElementSelector,
            nestedPrefetcher, sessionHandler);
        };
      prefetchManyProcessorCreators.Add(prefetchManyDelegate);
      return this;
    }
    
    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
      var sessionHandler = session.Handler;
      IEnumerable<TElement> result = new RootElementsPrefetcher<TElement>(source, keyExtractor, modelType,
        fieldDescriptors, sessionHandler);
      foreach (var prefetchManyDelegate in prefetchManyProcessorCreators)
        result = prefetchManyDelegate.Invoke(result, sessionHandler);
      return result.ToTransactional(session).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private \ internal methods

    private void RegisterPrefetchMany<TSelectorResult>(
      Func<T, IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> nestedPrefetcher)
    {
      ArgumentValidator.EnsureArgumentNotNull(nestedPrefetcher, "nestedPrefetcher");
      Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>> prefetchManyDelegate =
        (rootEnumerator, sessionHandler) => {
          Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector =
            (element, sh) => SelectChildElements(element, selector, sh);
          return new PrefetchManyProcessor<TElement, TSelectorResult>(rootEnumerator, childElementSelector,
            nestedPrefetcher, sessionHandler);
        };
      prefetchManyProcessorCreators.Add(prefetchManyDelegate);
    }

    private IEnumerable<TSelectorResult> SelectChildElements<TSelectorResult>(
      TElement element, Func<T, IEnumerable<TSelectorResult>> selector, SessionHandler sessionHandler)
    {
      EntityState state;
      sessionHandler.TryGetEntityState(keyExtractor.Invoke(element), out state);
      return selector.Invoke((T) (object) state.Entity);
    }

    private void Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression,
      int? entitySetItemCountLimit)
    {
      var body = expression.Body;
      if (body.NodeType != ExpressionType.MemberAccess)
        throw new ArgumentException(Strings.ExSpecifiedExpressionIsNotMemberExpression, "expression");
      var modelField = GetModelField(expression, (MemberExpression) body);
      fieldDescriptors[modelField] = new PrefetchFieldDescriptor(modelField, entitySetItemCountLimit,
        true, true, null);
    }

    private FieldInfo GetModelField(LambdaExpression expression, MemberExpression memberAccess)
    {
      var root = memberAccess;
      FieldInfo modelField = null;
      TypeInfo referencingType;
      if (root.Expression.Type.IsSubclassOf(typeof (Structure))) {
        while (root.Expression.NodeType==ExpressionType.MemberAccess) {
          if (modelField==null)
            modelField = FindFieldByProperty(root.Member as PropertyInfo, root.Expression.Type,
              out referencingType);
          else
            modelField = GetMappedFieldOfStructure(root, modelField);
          root = (MemberExpression) root.Expression;
        }
        modelField = GetMappedFieldOfStructure(root, modelField);
      }
      else
        modelField = FindFieldByProperty(root.Member as PropertyInfo, root.Expression.Type,
          out referencingType);
      if (root.Expression!=expression.Parameters[0])
        throw new ArgumentException(Strings.ExAccessToTypeMemberCanNotBeExtractedFromSpecifiedExpression,
          "expression");
      return modelField;
    }

    private FieldInfo GetMappedFieldOfStructure(MemberExpression propertyAccess, FieldInfo modelField)
    {
      if (!modelField.DeclaringType.IsStructure)
        throw new ArgumentException(Strings.ExAccessToTypeMemberCanNotBeExtractedFromSpecifiedExpression,
          "expression");
      TypeInfo referencingType;
      var fieldToStructure = FindFieldByProperty(propertyAccess.Member as PropertyInfo,
        propertyAccess.Expression.Type, out referencingType);
      modelField = referencingType
        .StructureFieldMapping[new Pair<FieldInfo>(fieldToStructure, modelField)];
      return modelField;
    }

    private FieldInfo FindFieldByProperty(PropertyInfo property, Type type, out TypeInfo typeInfo)
    {
      if (property==null)
        throw new ArgumentException(Strings.ExAccessedMemberIsNotProperty, "expression");
      typeInfo = type.GetTypeInfo(session.Domain);
      var result = typeInfo.Fields
        .Where(field => field.UnderlyingProperty!=null && field.UnderlyingProperty.Equals(property))
        .SingleOrDefault();
      if (result==null)
        throw new ArgumentException(String.Format(Strings.ExSpecifiedPropertyXIsNotPersistent,
          property.Name), "expression");
      return result;
    }

    #endregion


    // Constructors

    internal Prefetcher(Session session, IEnumerable<TElement> source, Func<TElement, Key> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.source = source;
      this.keyExtractor = keyExtractor;
      this.session = session;
      modelType = typeof (T) != typeof (Entity) ? typeof (T).GetTypeInfo(session.Domain) : null;
    }
  }
}