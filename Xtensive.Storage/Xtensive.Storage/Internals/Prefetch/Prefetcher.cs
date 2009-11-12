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

namespace Xtensive.Storage.Internals.Prefetch
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
      where TSelectorResult : IEntity
    {
      Prefetch(expression, null);
      var compiledPropertyGetter = expression.CachingCompile();
      Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>> prefetchManyDelegate =
        (rootEnumerator, sessionHandler) => {
          Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector =
            (element, sh) => SelectChildElements(element, compiledPropertyGetter,
              fieldValue => EnumerableUtils.One(selector.Invoke(fieldValue)), sh);
          return new PrefetchManyProcessor<TElement, TSelectorResult>(rootEnumerator, childElementSelector,
            prefetchManyFunc, sessionHandler);
        };
      prefetchManyProcessorCreators.Add(prefetchManyDelegate);
      return this;
    }
    
    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
      var sessionHandler = Session.Demand().Handler;
      IEnumerable<TElement> result = new RootElementsPrefetcher<TElement>(source, keyExtractor, modelType,
        fieldDescriptors, sessionHandler);
      foreach (var prefetchManyDelegate in prefetchManyProcessorCreators)
        result = prefetchManyDelegate.Invoke(result, sessionHandler);
      return result.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Executes prefetech tasks.
    /// </summary>
    public void Execute()
    {
      foreach (var element in this) {
        // Doing nothing.
      }
    }

    #region Private \ internal methods

    private void PrefetchMany<TFieldValue, TSelectorResult>(
      Expression<Func<T, TFieldValue>> expression, int? entitySetItemCountLimit,
      Func<TFieldValue, IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
    {
      Prefetch(expression, entitySetItemCountLimit);
      var compiledPropertyGetter = expression.CachingCompile();
      Func<IEnumerable<TElement>, SessionHandler, IEnumerable<TElement>> prefetchManyDelegate =
        (rootEnumerator, sessionHandler) => {
          Func<TElement, SessionHandler, IEnumerable<TSelectorResult>> childElementSelector =
            (element, sh) => SelectChildElements(element, compiledPropertyGetter,
              selector, sh);
          return new PrefetchManyProcessor<TElement, TSelectorResult>(rootEnumerator, childElementSelector,
            prefetchManyFunc, sessionHandler);
        };
      prefetchManyProcessorCreators.Add(prefetchManyDelegate);
    }

    private IEnumerable<TSelectorResult> SelectChildElements<TFieldValue, TSelectorResult>(
      TElement element, Func<T, TFieldValue> propertyGetter,
      Func<TFieldValue, IEnumerable<TSelectorResult>> selector, SessionHandler sessionHandler)
    {
      EntityState state;
      sessionHandler.TryGetEntityState(keyExtractor.Invoke(element), out state);
      return selector.Invoke(propertyGetter.Invoke((T) (object) state.Entity));
    }

    private void Prefetch<TFieldValue>(Expression<Func<T, TFieldValue>> expression,
      int? entitySetItemCountLimit)
    {
      var body = expression.Body;
      if (body.NodeType != ExpressionType.MemberAccess)
        throw new ArgumentException(Strings.ExSpecifiedExpressionIsNotMemberExpression, "expression");
      var modelField = GetModelField(expression, (MemberExpression) body);
      fieldDescriptors[modelField] = new PrefetchFieldDescriptor(modelField, entitySetItemCountLimit,
        true, null);
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

    internal Prefetcher(IEnumerable<TElement> source, Func<TElement, Key> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.source = source;
      this.keyExtractor = keyExtractor;
      session = Session.Demand();
      modelType = typeof (T) != typeof (Entity) ? typeof (T).GetTypeInfo(session.Domain) : null;
    }
  }
}