// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that property is persistent field,
  /// and defines its percistence-related properies.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class FieldAttribute : MappingAttribute
  {
    internal bool? isCollatable;
    internal bool? isTranslatable;
    internal int? length;
    internal bool? lazyLoad;
    internal int? scale;
    internal int? precision;
    internal ReferentialAction? referentialAction;

    /// <summary>
    /// Gets or sets the length of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="string"/> or array of <see cref="byte"/> fields.
    /// </remarks>
    public int Length
    {
      get { return length.HasValue ? length.Value : 0; }
      set { length = value; }
    }

    /// <summary>
    /// Gets or sets the scale of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Scale
    {
      get { return scale.HasValue ? scale.Value : 0; }
      set { scale = value; }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Precision
    {
      get { return precision.HasValue ? precision.Value : 0; }
      set { precision = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field should be stored as translatable.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="string"/> fields.
    /// </remarks>
    public bool Translatable
    {
      get { return isTranslatable.HasValue ? isTranslatable.Value : false; }
      set { isTranslatable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field should be stored as collatable.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="string"/> fields.
    /// </remarks>
    public bool Collatable
    {
      get { return isCollatable.HasValue ? isCollatable.Value : false; }
      set { isCollatable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether value of this field should be loaded on demand.
    /// </summary>
    /// <remarks>
    /// Usually lazy loading is used for byte-arrays, large string fields or <see cref="Structure">structures</see>.
    /// <see cref="Entity"/> and <see cref="EntitySet{TItem}"/> fields are always loaded on demand.
    /// </remarks>
    public bool LazyLoad
    {
      get { return lazyLoad.HasValue ? lazyLoad.Value : false; }
      set { lazyLoad = value; }
    }

    /// <summary>
    /// Gets or sets the referential action that will be executed on referenced Entity removal.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="Entity"/> and <see cref="EntitySet{TItem}"/>.
    /// </remarks>
    /// <seealso cref="ReferentialAction">Referential actions</seealso>
    public ReferentialAction OnRemove
    {
      get { return referentialAction.HasValue ? referentialAction.Value : ReferentialAction.Default; }
      set { referentialAction = value; }
    }

    /// <summary>
    /// Indicates that persistent collection or persistent field
    /// is a paired proprty with another collection or reference field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute can be applied to persistent properties of <see cref="Entity"/> 
    /// or <see cref="EntitySet{T}"/> types.
    /// </para>
    /// <para>
    /// When reference field is paired to another reference field, their value is automatically synchronized.
    /// </para>
    /// <para>
    /// When collection is paired to reference field (One-to-Many association), 
    /// it does not allocate any space in the database and all operations on this EntitySet are 
    /// automatically synchronized to paired reference field.
    /// </para>
    /// <para>
    /// When collection is paired to another collection (Many-to-Many) association, auxiliary table
    /// will be automatically created to support this association.
    /// </para>
    /// </remarks>
    /// <example>In following example User entity has three associated fields of diffirent association type.
    /// <code>
    /// public class User : Entity
    /// {
    ///   ...
    ///   
    ///   // One-to-one association with "User" propery of "Account" class.
    ///   [Field(PairTo = "User")]
    ///   public Account Account { get; private set; }
    ///   
    ///   // One-to-many association
    ///   [Field(PairTo = "Author")]
    ///   public EntitySet&lt;BlogItem&gt; BlogItems { get; private set; }
    ///   
    ///   // Many-to-many association
    ///   [Field(PairTo = "Friends")]
    ///   public EntitySet&lt;User&gt; Friends { get; private set; }
    /// }
    /// </code>
    /// </example>
    public string PairTo { get; set; }


    // Constructors

    /// <inheritdoc/>
    public FieldAttribute()
    {
    }

    /// <inheritdoc/>
    public FieldAttribute(string mappingName)
      : base(mappingName)
    {
    }
  }
}