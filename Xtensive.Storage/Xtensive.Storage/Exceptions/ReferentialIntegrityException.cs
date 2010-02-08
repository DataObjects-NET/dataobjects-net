// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Thrown on attempt to remove an object having
  /// reference with <see cref="OnRemoveAction.Deny"/>
  /// option pointing to it.
  /// </summary>
  [Serializable]
  public class ReferentialIntegrityException : StorageException
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferentialIntegrityException(AssociationInfo association, Entity initiator, Entity referencingObject, Entity referencedObject)
      : base(string.Format(
      Strings.ReferentialIntegrityViolationOnAttemptToRemoveXKeyY, initiator.GetType().GetFullName(), initiator.Key))
    {
      Association = association;
      Initiator = initiator.Key;
      ReferencingObject = referencingObject.Key;
      ReferencedObject = referencedObject.Key;
    }
  }
}