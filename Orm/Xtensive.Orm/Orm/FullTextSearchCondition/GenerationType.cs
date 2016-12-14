// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition
{
  /// <summary>
  /// The generation type specifies how Search chooses the alternative word forms.
  /// </summary>
  public enum GenerationType
  {
    /// <summary>
    /// Chooses alternative inflection forms for the match words.
    /// </summary>
    Inflectional,

    /// <summary>
    /// Chooses words that have the same meaning, taken from a thesaurus
    /// </summary>
    Thesaurus
  }
}
