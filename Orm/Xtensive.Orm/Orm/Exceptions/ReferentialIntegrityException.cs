// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
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
  public sealed class ReferentialIntegrityException : StorageException
  {
    /// <summary>
    /// Gets the association.
    /// </summary>
    public AssociationInfo Association { get; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the initiator of removing action.
    /// </summary>
    public Key Initiator { get; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referencing object.
    /// </summary>
    public Key ReferencingObject { get; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referenced object.
    /// </summary>
    public Key ReferencedObject { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ReferentialIntegrityException(AssociationInfo association, 
      Entity initiator, 
      Entity referencingObject, 
      Entity referencedObject)
      : base(
        string.Format(Strings.ReferentialIntegrityViolationOnAttemptToRemoveXKeyY,
          initiator.GetType().GetFullName(), initiator.Key,
          association, referencingObject.Key, referencedObject.Key))
    {
      Association = association;
      Initiator = initiator.Key;
      ReferencingObject = referencingObject.Key;
      ReferencedObject = referencedObject.Key;
    }
  }
}