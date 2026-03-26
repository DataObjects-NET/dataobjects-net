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
    [Flags]
    internal enum DeleteDataHintState : byte
    {
      None = 0,
      PostCopy = 1,
      TableMovement = 2,
    }

    private readonly DeleteDataHintState state = DeleteDataHintState.None;

    /// <summary>
    /// Gets a value indicating whether deletion must be performed after completion of copy data hint processing.
    /// Normally this flag is used to remove records related to types moved to other hierarchies -
    /// these records are still necessary during upgrade to be copied, but must be removed on its
    /// completion.
    /// </summary>
    public bool IsPostCopyCleanup => (state & DeleteDataHintState.PostCopy) > 0;

    /// <summary>
    /// Gets a value indicating whether cause of data deletion is due to table have changed its owner type.
    /// </summary>
    public bool IsOwnerChangeCleanup => (state & DeleteDataHintState.TableMovement) > 0;

    /// <summary>
    /// Gets a value indication whether deletion is unsafe. Deletion is considered insafe
    /// if PostCopy or DueToTableOwnerChange equals <see langword="true"/>.
    /// </summary>
    public bool IsUnsafe => state != DeleteDataHintState.None;


    /// <summary>
    /// Internal infomations about this hint.
    /// <para>
    /// It is used in some cases to create new hint 
    /// based on this one. It can be done by open
    /// constructors but enum is better.
    /// </para>
    /// </summary>
    internal DeleteDataHintState State => state;

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Delete from '{0}' {1}{2}{3}",
        SourceTablePath,
        (Identities.Count > 0)
          ? "where (" + string.Join(" and ", Identities.Select(pair => pair.ToString()).ToArray()) + ")"
          : string.Empty,
        IsPostCopyCleanup ? " (after data copying)" : string.Empty,
        IsOwnerChangeCleanup ? " (due to table changed owner type)" : string.Empty);
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
    /// <param name="postCopy"><see cref="IsPostCopyCleanup"/> property value.</param>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities, bool postCopy)
      : base(sourceTablePath, identities)
    {
      if (postCopy) {
        state |= DeleteDataHintState.PostCopy;
      }
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath">Source table path.</param>
    /// <param name="identities">Identities for data operation.</param>
    /// <param name="postCopy"><see cref="IsPostCopyCleanup"/> property value.</param>
    /// <param name="dueToOnwerChange"><see langword="true"/> if reason of deletion is the table <paramref name="sourceTablePath"/>
    /// has changed assigned type.</param>
    public DeleteDataHint(string sourceTablePath, IList<IdentityPair> identities, bool postCopy, bool dueToOnwerChange)
      : base(sourceTablePath, identities)
    {
      if (postCopy) {
        state |= DeleteDataHintState.PostCopy;
      }
      if (dueToOnwerChange) {
        state |= DeleteDataHintState.TableMovement;
      }
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath">Source table path.</param>
    /// <param name="identities">Identities for data operation.</param>
    /// <param name="state">Hint state.</param>
    internal DeleteDataHint(string sourceTablePath, IList<IdentityPair> identities, DeleteDataHintState state)
      : base(sourceTablePath, identities)
    {
      this.state = state;
    }
  }
}
