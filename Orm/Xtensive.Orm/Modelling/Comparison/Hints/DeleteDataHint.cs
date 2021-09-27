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
    internal enum DeleteDataHintInfo : byte
    {
      None = 0,
      PostCopy = 1,
      TableMovement = 2,
      All = PostCopy | TableMovement
    }

    private readonly DeleteDataHintInfo info = DeleteDataHintInfo.None;

    /// <summary>
    /// Gets a value indicating whether deletion must be performed after completion of copy data hint processing.
    /// Normally this flag is used to remove records related to types moved to other hierarchies -
    /// these records are still necessary during upgrade to be copied, but must be removed on its
    /// completion.
    /// </summary>
    public bool PostCopy => info.HasFlag(DeleteDataHintInfo.PostCopy);

    /// <summary>
    /// Gets a value indicating whether cause of data deletion is due to table have changed its owner type.
    /// </summary>
    public bool DueToTableOwnerChange => info.HasFlag(DeleteDataHintInfo.TableMovement);

    /// <summary>
    /// Gets a value indication whether deletion is unsafe. Deletion is considered insafe
    /// if PostCopy or DueToTableOwnerChange equals <see langword="true"/>.
    /// </summary>
    public bool IsUnsafe => info != DeleteDataHintInfo.None;


    /// <summary>
    /// Internal infomations about this hint.
    /// <para>
    /// It is used in some cases to create new hint 
    /// based on this one. It can be done by open
    /// constructors but enum is better.
    /// </para>
    /// </summary>
    internal DeleteDataHintInfo Info => info;

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Delete from '{0}' where ({1}){2}{3}",
        SourceTablePath,
        string.Join(" and ",
          Identities.Select(pair => pair.ToString()).ToArray()),
        PostCopy ? " (after data copying)" : string.Empty,
        DueToTableOwnerChange ? " due to table changed owner type" : string.Empty);
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
      if (postCopy) {
        info |= DeleteDataHintInfo.PostCopy;
      }
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath">Source table path.</param>
    /// <param name="identities">Identities for data operation.</param>
    /// <param name="postCopy"><see cref="PostCopy"/> property value.</param>
    /// <param name="dueToOnwerChange"><see langword="true"/> if reason of deletion is the table <paramref name="sourceTablePath"/>
    /// has changed assigned type.</param>
    public DeleteDataHint(string sourceTablePath, IList<IdentityPair> identities, bool postCopy, bool dueToOnwerChange)
      : this(sourceTablePath, identities)
    {
      if (postCopy) {
        info |= DeleteDataHintInfo.PostCopy;
      }
      if (dueToOnwerChange) {
        info |= DeleteDataHintInfo.TableMovement;
      }
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceTablePath"></param>
    /// <param name="identities"></param>
    /// <param name="deleteDataInfo"></param>
    internal DeleteDataHint(string sourceTablePath, IList<IdentityPair> identities, DeleteDataHintInfo deleteDataInfo)
      : base(sourceTablePath, identities)
    {
      info = deleteDataInfo;
    }
  }
}
