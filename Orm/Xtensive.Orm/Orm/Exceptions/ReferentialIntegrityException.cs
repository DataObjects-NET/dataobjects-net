// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

using Xtensive.Reflection;
using Xtensive.Orm.Model;


namespace Xtensive.Orm
{
  /// <summary>
  /// Thrown on attempt to remove an object having
  /// reference with <see cref="OnRemoveAction.Deny"/>
  /// option pointing to it.
  /// </summary>
  [Serializable]
  public sealed class ReferentialIntegrityException : StorageException
  {
    /// <summary>
    /// Gets the association.
    /// </summary>
    public AssociationInfo Association { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the initiator of removing action.
    /// </summary>
    public Key Initiator { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referencing object.
    /// </summary>
    public Key ReferencingObject { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referenced object.
    /// </summary>
    public Key ReferencedObject { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ReferentialIntegrityException(AssociationInfo association, 
      Entity initiator, 
      Entity referencingObject, 
      Entity referencedObject)
      : base(
        Strings.ReferentialIntegrityViolationOnAttemptToRemoveXKeyY.FormatWith(
          initiator.GetType().GetFullName(), initiator.Key,
          association, referencingObject.Key, referencedObject.Key))
    {
      Association = association;
      Initiator = initiator.Key;
      ReferencingObject = referencingObject.Key;
      ReferencedObject = referencedObject.Key;
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ReferentialIntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      // We can't serialize any of members declared in this type
    }
  }
}