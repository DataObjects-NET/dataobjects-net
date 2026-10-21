// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes an index.
  /// </summary>
  public class IndexInfo : EntityInfo
  {
    private IndexFeatures features = IndexFeatures.None;
    private int maxLength;
    private int maxNumberOfColumns;
    private PartitionMethods partitionMethods;
    private bool? nullsDistinctByDefault;

    /// <summary>
    /// Gets or sets the maximal index length in bytes.
    /// </summary>
    public int MaxLength {
      get { return maxLength; }
      set {
        EnsureNotLocked();
        maxLength = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximum number of columns per index.
    /// </summary>
    public int MaxNumberOfColumns {
      get { return maxNumberOfColumns; }
      set {
        EnsureNotLocked();
        maxNumberOfColumns = value;
      }
    }

    /// <summary>
    /// Gets or sets supported partition methods.
    /// </summary>
    public PartitionMethods PartitionMethods {
      get { return partitionMethods; }
      set {
        EnsureNotLocked();
        partitionMethods = value;
      }
    }

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public IndexFeatures Features {
      get { return features; }
      set {
        EnsureNotLocked();
        features = value;
      }
    }

    /// <summary>
    /// Gets or sets whether NULL values are treated as distinct by unique indexes by default.
    /// <see langword="null">Null</see> means that there is no consistent policy.
    /// Distinct NULL values means that unique indexes allow MORE THAN ONE NULL value for column.
    /// Not distinct NULL values means that unique indexes allow ONLY ONE NULL value for column.
    /// </summary>
    public bool? NullDistinctByDefault
    {
      get => nullsDistinctByDefault;
      set {
        EnsureNotLocked();
        nullsDistinctByDefault = value;
      }
    }
  }
}
