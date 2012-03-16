// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  /// <summary>
  /// Levels of checking to be done when inserting or updating data through a view.
  /// </summary>
  [Serializable]
  public enum CheckOptions
  {
    /// <summary>
    /// The same as <see cref="None"/>.
    /// </summary>
    Default = None,

    /// <summary>
    /// None check options are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// This option is identical to <see cref="Cascaded"/> option except that you can update 
    /// a row so that it no longer can be retrieved through the view. 
    /// This can only happen when the view is directly or indirectly dependent on a view 
    /// that was defined with no WITH CHECK OPTION clause.
    /// </summary>
    Local = 1,

    /// <summary>
    /// This option specifies that every row that is inserted or updated through the view 
    /// must conform to the definition of the view. In addition, the search conditions 
    /// of all dependent views are checked when a row is inserted or updated. If a row 
    /// does not conform to the definition of the view, that row cannot be retrieved using the view.
    /// </summary>
    Cascaded = 2,
  }
}
