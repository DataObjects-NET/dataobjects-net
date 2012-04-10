// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.07

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Provides additional properties to association. 
  /// This attribute can be applied on persistent properties of <see cref="Entity"/> or <see cref="EntitySet{TItem}"/> type.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class AssociationAttribute : StorageAttribute
  {
    internal OnRemoveAction? onTargetRemove;
    internal OnRemoveAction? onOwnerRemove;

    /// <summary>
    /// Gets or sets the <see cref="OnRemoveAction"/> action that will be executed in case that 
    /// target (referenced) Entity is about to be removed.
    /// </summary>
    public OnRemoveAction OnTargetRemove
    {
      get { return onTargetRemove.HasValue ? onTargetRemove.Value : OnRemoveAction.Default; }
      set { onTargetRemove = value; }
    }

    /// <summary>
    /// Gets or sets the <see cref="OnRemoveAction"/> action that will be executed in case that 
    /// owner Entity (the owner of the reference field) is about to be removed.
    /// </summary>
    public OnRemoveAction OnOwnerRemove
    {
      get { return onOwnerRemove.HasValue ? onOwnerRemove.Value : OnRemoveAction.Default; }
      set { onOwnerRemove = value; }
    }

    /// <summary>
    /// Indicates that association (persistent collection or persistent field)
    /// is inverse end of another another collection or reference field.
    /// </summary>
    /// <remarks>
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
    /// <example>In the following example User entity has three associations of different types.
    /// <code>
    /// public class User : Entity
    /// {
    ///   ...
    ///   
    ///   // One-to-one association with "User" propery of "Account" class.
    ///   [Association(PairTo = "User")]
    ///   public Account Account { get; private set; }
    ///   
    ///   // One-to-many association
    ///   [Association(PairTo = "Author")]
    ///   public EntitySet&lt;BlogPost&gt; BlogPostss { get; private set; }
    ///   
    ///   // Many-to-many association
    ///   [Association(PairTo = "Friends")]
    ///   public EntitySet&lt;User&gt; Friends { get; private set; }
    /// }
    /// </code>
    /// </example>
    public string PairTo { get; set; }


    // Constructors

    /// <inheritdoc/>
    public AssociationAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="pairTo">The pair to.</param>
    public AssociationAttribute(string pairTo)
    {
      PairTo = pairTo;
    }
  }
}