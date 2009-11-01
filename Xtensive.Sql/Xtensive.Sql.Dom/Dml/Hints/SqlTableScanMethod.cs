// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public enum SqlTableScanMethod
  {
    /// <summary>
    /// DataTable scan method is selected by query optimizer.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The Full hint instructs the optimizer to perform a full table scan for the specified table.
    /// </summary>
    Full = 1,

    /// <summary>
    /// The Cluster hint instructs the optimizer to use a cluster scan to access the specified table.
    /// </summary>
    Cluster = 2,

    /// <summary>
    /// The Hash hint instructs the optimizer to use a hash scan to access the specified table.
    /// </summary>
    Hash = 3,

    /// <summary>
    /// The Index hint instructs the optimizer to use an index scan for the specified table.
    /// <para>The behavior of the hint depends on the index specification:</para>
    /// <list type="bullet">
    /// <item>If the Index hint specifies a single available index, then the database performs a scan on this index. 
    /// The optimizer does not consider a full table scan or a scan of another index on the table.</item>
    /// <item>For a hint on a combination of multiple indexes, Oracle recommends using IndexCombine rather than Index, 
    /// because it is a more versatile hint. If the Index hint specifies a list of available indexes, then the optimizer 
    /// considers the cost of a scan on each index in the list and then performs the index scan with the lowest cost. 
    /// The database can also choose to scan multiple indexes from this list and merge the results, 
    /// if such an access path has the lowest cost. The database does not consider a full table scan 
    /// or a scan on an index not listed in the hint.</item>
    /// <item>If the index hint specifies no indexes, then the optimizer considers the cost of a scan on each 
    /// available index on the table and then performs the index scan with the lowest cost. 
    /// The database can also choose to scan multiple indexes and merge the results, if such an access path has the lowest cost. 
    /// The optimizer does not consider a full table scan.</item>
    /// </list>
    /// </summary>
    Index = 4,

    /// <summary>
    /// The IndexAsc hint instructs the optimizer to use an index scan for the specified table. 
    /// If the statement uses an index range scan, then Oracle Database scans the index entries in ascending order of 
    /// their indexed values.
    /// <para/>
    /// The default behavior for a range scan is to scan index entries in ascending order of their indexed values, 
    /// or in descending order for a descending index. This hint does not change the default order of the index, 
    /// and therefore does not specify anything more than the Index hint. However, you can use the IndexAsc hint 
    /// to specify ascending range scans explicitly should the default behavior change.
    /// </summary>
    IndexAsc = 5,

    /// <summary>
    /// The IndexCombine hint instructs the optimizer to use a bitmap access path for the table. 
    /// If index specification is omitted from the IndexCombine hint, then the optimizer uses whatever Boolean combination 
    /// of indexes has the best cost estimate for the table. If you specify index specification, then the optimizer 
    /// tries to use some Boolean combination of the specified indexes.
    /// </summary>
    IndexCombine = 6,

    /// <summary>
    /// The IndexJoin hint instructs the optimizer to use an index join as an access path. 
    /// For the hint to have a positive effect, a sufficiently small number of indexes must exist that contain all 
    /// the columns required to resolve the query.
    /// </summary>
    IndexJoin = 7,

    /// <summary>
    /// The IndexDesc hint instructs the optimizer to use a descending index scan for the specified table. 
    /// If the statement uses an index range scan and the index is ascending, then Oracle scans the index entries in 
    /// descending order of their indexed values. In a partitioned index, the results are in descending order 
    /// within each partition. For a descending index, this hint effectively cancels out the descending order, 
    /// resulting in a scan of the index entries in ascending order.
    /// </summary>
    IndexDesc = 8,

    /// <summary>
    /// The fastFullIndex hint instructs the optimizer to perform a fast full index scan rather than a full table scan.
    /// </summary>
    FastFullIndex = 9,

    /// <summary>
    /// The Index skip scan hint instructs the optimizer to perform an index skip scan for the specified table. 
    /// If the statement uses an index range scan, then Oracle scans the index entries in ascending order of their 
    /// indexed values. In a partitioned index, the results are in ascending order within each partition.
    /// </summary>
    IndexSkip = 10,

    /// <summary>
    /// The IndexSkipAsc hint instructs the optimizer to perform an index skip scan for the specified table. 
    /// If the statement uses an index range scan, then Oracle Database scans the index entries in ascending order of 
    /// their indexed values. In a partitioned index, the results are in ascending order within each partition.
    /// </summary>
    IndexSkipAsc = 11,

    /// <summary>
    /// The IndexScipScanDesc hint instructs the optimizer to perform an index skip scan for the specified table. 
    /// If the statement uses an index range scan and the index is ascending, then Oracle scans the index entries in 
    /// descending order of their indexed values. In a partitioned index, the results are in descending order within 
    /// each partition. For a descending index, this hint effectively cancels out the descending order, 
    /// resulting in a scan of the index entries in ascending order.
    /// </summary>
    IndexSkipDesc = 12,

    /// <summary>
    /// The NoIndex hint instructs the optimizer not to use one or more indexes for the specified table.
    /// <para>Each parameter serves the same purpose as in Index hint with the following modifications:</para>
    /// <list type="bullet">
    /// <item>If this hint specifies a single available index, then the optimizer does not consider a scan on this index. 
    /// Other indexes not specified are still considered.</item>
    /// <item>If this hint specifies a list of available indexes, then the optimizer does not consider a scan on any of the 
    /// specified indexes. Other indexes not specified in the list are still considered.</item>
    /// <item>If this hint specifies no indexes, then the optimizer does not consider a scan on any index on the table. 
    /// This behavior is the same as a NoIndex hint that specifies a list of all available indexes for the table. </item>
    /// </list>
    /// </summary>
    NoIndex = 13,

    /// <summary>
    /// The NoFastFullIndex hint instructs the optimizer to exclude a fast full index scan of the specified indexes 
    /// on the specified table.
    /// </summary>
    NoFastFullIndex = 14,

    /// <summary>
    /// The NoIndexSkip hint instructs the optimizer to exclude a skip scan of the specified indexes on the specified table.
    /// </summary>
    NoIndexSkip = 15,
  }
}