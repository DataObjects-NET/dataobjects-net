// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Common.Mssql
{
  /// <summary>
  /// Enumeration of a possible MSSQL engines.
  /// </summary>
  public enum MssqlEngineEdition
  {
    /// <summary>
    /// Identifies engine of the Personal and Desktop Engine editions (Not available for SQL Server 2005.)
    /// </summary>
    /// <value><see langword="1"/></value>
    Personal   = 1,
    
    /// <summary>
    /// Identifies engine of the Standard and Workgroup editions.
    /// </summary>
    /// <value><see langword="2"/></value>
    Standard   = 2,
    
    /// <summary>
    /// Identifies engine of the Enterprise, Enterprise Evaluation, and Developer editions.
    /// </summary>
    /// <value><see langword="3"/></value>
    Enterprise = 3,
    
    /// <summary>
    /// Identifies engine of the Express, Express Edition with Advanced Services,
    /// and Windows Embedded SQL editions.
    /// </summary>
    /// <value><see langword="4"/></value>
    Express    = 4
  }
}
