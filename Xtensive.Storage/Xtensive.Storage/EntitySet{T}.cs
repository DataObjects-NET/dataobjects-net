// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Linq;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  /// <summary>
  /// Non-ordered persistent entity-bound set (with no duplicate items).
  /// </summary>
  /// <typeparam name="TItem">The type of the entities in this set.</typeparam>
  /// <remarks>
  /// <para>
  /// Use <see cref="EntitySet{TItem}"/> when you need to declare persistent property of entity set type.
  /// </para>
  /// <para>
  /// <c>EntitySets</c> can be used as a <see cref="AssociationAttribute.PairTo">paired property</see> with reference 
  /// (One-To-Many) or EntitySet (Many-To-Many) properties. In such case DataObjects.Net automatically
  /// modifies collection or it's paired property. If paired property is not specified, auxiliary table
  /// will be automatically created in database.
  /// </para>
  /// <para>EntitySet class implements <see cref="IQueryable{T}"/> interface and fully supported by 
  /// DataObjects.Net LINQ translator.</para>
  /// </remarks>
  /// <example>In following example User entity has three EntitySet properties with different association kinds.
  /// <code>
  /// public class User : Entity
  /// {
  ///   ...
  ///   
  ///   // persistent collection with auxiliary table
  ///   [Field]
  ///   public EntitySet&lt;Photo&gt; Photos { get; private set; }
  ///   
  ///   // One-to-many association
  ///   [Field, Association(PairTo = "Author")]
  ///   public EntitySet&lt;BlogItem&gt; BlogItems { get; private set; }
  ///   
  ///   // Many-to-many association
  ///   [Field, Association(PairTo = "Friends")]
  ///   public EntitySet&lt;User&gt; Friends { get; private set; }
  /// }
  /// </code>
  /// </example>
  /// <seealso cref="Entity">Entity class</seealso>
  /// <seealso cref="AssociationAttribute.PairTo">Using EntitySets with paired associations</seealso>
  public class EntitySet<TItem> : EntitySetBase,
    ICollection<TItem>, 
    ICountable<TItem>, 
    IQueryable<TItem>
    where TItem : IEntity
  {
    private static readonly MethodInfo GetItemCountQueryMethod = typeof(EntitySet<TItem>)
      .GetMethod("GetItemCountQuery", BindingFlags.Static | BindingFlags.NonPublic);
    
    private Expression expression;
    private bool isCountCalculated;

    /// <summary>
    /// Determines whether this collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for containment.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure] // Proxy
    public bool Contains(TItem item)
    {
      return base.Contains(item);
    }
    
    /// <summary>
    /// Adds the specified item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is added to the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure] // Proxy
    public bool Add(TItem item)
    {
      return base.Add(item);
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is removed from the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure] // Proxy
    public bool Remove(TItem item)
    {
      return base.Remove(item);
    }

    /// <summary>
    /// Adds the <paramref name="items"/> to this <see cref="EntitySet{TItem}"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="items">The items to add.</param>
    public new void AddRange<TElement>(IEnumerable<TElement> items)
      where TElement: TItem
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      base.AddRange(items);
    }

    /// <summary>
    /// Modifies the current <see cref="EntitySet{TItem}"/> object
    /// to contain only elements that are present in that object and in the specified collection.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="other">The collection to compare to the current <see cref="EntitySet{TItem}"/> object.</param>
    public new void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : TItem
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      base.IntersectWith(other);
    }

    /// <summary>
    /// Modifies the current <see cref="EntitySet{TItem}"/> object
    /// to contain all elements that are present in both itself and in the specified collection.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="other">The collection to compare to the current <see cref="EntitySet{TItem}"/> object.</param>
    public new void UnionWith<TElement>(IEnumerable<TElement> other)
      where TElement : TItem
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      base.UnionWith(other);
    }

    /// <summary>
    /// Removes all elements in the specified collection from the current <see cref="EntitySet{TItem}"/> object.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="other">The collection to compare to the current <see cref="EntitySet{TItem}"/> object.</param>
    public new void ExceptWith<TElement>(IEnumerable<TElement> other)
      where TElement : TItem
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      base.ExceptWith(other);
    }

    #region ICollection<T> members

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    int ICollection<TItem>.Count
    {
      [DebuggerStepThrough]
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    void ICollection<TItem>.Add(TItem item)
    {
      base.Add(item);
    }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (var item in Entities)
        yield return (TItem) item;
    }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      foreach (TItem item in this)
        array[arrayIndex++] = item;
    }

    #endregion

    #region IQueryable<T> members

    /// <summary>
    /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
    /// </summary>
    /// <value></value>
    /// <inheritdoc/>
    [Infrastructure]
    public Expression Expression
    {
      get {
        EnsureOwnerIsNotRemoved();
        if (expression==null) {
          // A hack making expression to look like regular parameter 
          // (ParameterExtractor.IsParameter => true)
          var owner = Expression.Property(Expression.Constant(new {Owner}), "Owner");
          expression = QueryHelper.CreateEntitySetQueryExpression(owner, Field);
        }
        return expression;
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public Type ElementType { get { return typeof (TItem); } }

    /// <inheritdoc/>
    [Infrastructure]
    public IQueryProvider Provider { get { return QueryProvider.Instance; } }

    #endregion
    
    /// <inheritdoc/>
    protected sealed override Func<long> GetItemCountQueryDelegate(FieldInfo field)
    {
      return (Func<long>) Delegate.CreateDelegate(typeof (Func<long>), field, GetItemCountQueryMethod);
    }
    
    private static IQueryable<TItem> GetItemsQuery(FieldInfo field)
    {
      var owner = Expression.Property(Expression.Constant(ownerParameter), ownerParameter.GetType()
        .GetProperty("Value", typeof(Entity)));
      var queryExpression = QueryHelper.CreateEntitySetQueryExpression(owner, field);
      return new Queryable<TItem>(queryExpression);
    }

    private static long GetItemCountQuery(FieldInfo field)
    {
      return GetItemsQuery(field).LongCount();
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySet(Entity owner, FieldInfo field)
      : base(owner, field)
    {
      Initialize(typeof (EntitySet<TItem>));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected EntitySet(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      throw new NotImplementedException();
    }
  }
}