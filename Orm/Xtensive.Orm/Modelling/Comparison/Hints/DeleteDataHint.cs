// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Delete from '{0}' where ({1}){2}",
        SourceTablePath,
        string.Join(" and ",
          Identities.Select(pair => pair.ToString()).ToArray()),
          PostCopy ? " (after data copying)" : string.Empty);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities)
      : base(sourceTablePath, identities)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="postCopy"><see cref="PostCopy"/> property value.</param>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities, bool postCopy)
      : base(sourceTablePath, identities)
    {
      PostCopy = postCopy;
    }
  }
}