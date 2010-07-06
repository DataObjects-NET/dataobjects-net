// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint for delete data operation.
  /// </summary>
  [Serializable]
  public class DeleteDataHint : DataHint
  {
    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(
        "Delete from '{0}' where ({1})",
        SourceTablePath,
        string.Join(" and ",
          Identities.Select(pair => pair.ToString()).ToArray()));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DeleteDataHint(string sourceTablePath,  IList<IdentityPair> identities)
      :base(sourceTablePath, identities)
    {
    }
  }
}