// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using System.Linq;


namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint for delete data operation.
  /// </summary>
  [Serializable]
  public class DeleteDataHint : DataHint
  {
    /// <summary>
    /// Gets or sets a value indicating whether deletion must be performed after completion of copy data hint processing.
    /// Normally this flag is used to remove records related to types moved to other hierarchies -
    /// these records are still necessary during upgrade to be copied, but must be removed on its
    /// completion.
    /// </summary>
    public bool PostCopy { get; private set; }

    /// <summary>
    /// Gets value indicating whether cause of data deletion was moving table to another hierarchy.
    /// This flag is used to ideitify and prevent unsafe data clean-up even if database structure
    /// remain identical but logically table serves completely different entity.
    /// </summary>
    public bool TableChangedOwner { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Delete from '{0}' where ({1}){2}{3}",
        SourceTablePath,
        string.Join(" and ",
          Identities.Select(pair => pair.ToString()).ToArray()),
        PostCopy ? " (after data copying)" : string.Empty,
        TableChangedOwner ? " due to table owner changed" : string.Empty);
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities)
      : base(sourceTablePath, identities)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath">Source table path.</param>
    /// <param name="identities">Identities for data operation.</param>
    /// <param name="postCopy"><see cref="PostCopy"/> property value.</param>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities, bool postCopy)
      : base(sourceTablePath, identities)
    {
      PostCopy = postCopy;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath">Source table path.</param>
    /// <param name="identities">Identities for data operation.</param>
    /// <param name="postCopy"><see cref="PostCopy"/> property value.</param>
    /// <param name="tableChangedOwner"><see cref="TableChangedOwner"/> property value.</param>
    public DeleteDataHint(string sourceTablePath, IList<IdentityPair> identities, bool postCopy, bool tableChangedOwner)
      : base(sourceTablePath, identities)
    {
      PostCopy = postCopy;
      TableChangedOwner = tableChangedOwner;
    }
  }
}
