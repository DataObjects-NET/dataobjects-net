// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.06.17

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Enumeartes possible full-text change tracking modes.
  /// </summary>
  public enum ChangeTrackingMode
  {
    /// <summary>
    /// The tracked changes will be propagated as data is modified (automatic population).
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The tracked changes will be propagated manually (manual population).
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Storage does not keep a list of changes to the indexed data. Indexes will be populated after they are created.
    /// </summary>
    Off = 2,

    /// <summary>
    /// Storage does not keep a list of changes to the indexed data. Indexes' population should be started manually.
    /// </summary>
    OffWithNoPopulation = 3,

    /// <summary>
    /// Default change tracking mode.
    /// </summary>
    Default = Auto,
  }
}
