// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.SqlServer
{
  /// <summary>
  /// Enumeration of a possible MSSQL engines.
  /// </summary>
  public enum SqlServerEdition : long
  {
    /// <summary>
    /// Indicates that MSSQL server edition is unknown.
    /// </summary>
    /// <value><see langword="0"/></value>
    Unknown = 0,
    
    /// <summary>
    /// Identifies MSSQL server Desktop Engine edition (Not available for SQL Server 2005.)
    /// </summary>
    /// <value><see langword="-1253826760"/></value>
    DesktopEdition = -1253826760,

    /// <summary>
    /// Identifies MSSQL server Express edition.
    /// </summary>
    /// <value><see langword="-1592396055"/></value>
    ExpressEdition = -1592396055,
    
    /// <summary>
    /// Identifies MSSQL server Standard edition.
    /// </summary>
    /// <value><see langword="-1534726760"/></value>
    StandardEdition = -1534726760,
    
    /// <summary>
    /// Identifies MSSQL server Workgroup edition.
    /// </summary>
    /// <value><see langword="1333529388"/></value>
    WorkgroupEdition = 1333529388,
    
    /// <summary>
    /// Identifies MSSQL server Enterprise edition.
    /// </summary>
    /// <value><see langword="1804890536"/></value>
    EnterpriseEdition = 1804890536,
    
    /// <summary>
    /// Identifies MSSQL server Personal edition.
    /// </summary>
    /// <value><see langword="-323382091"/></value>
    PersonalEdition = -323382091,
    
    /// <summary>
    /// Identifies MSSQL server Developer edition.
    /// </summary>
    /// <value><see langword="-2117995310"/></value>
    DeveloperEdition = -2117995310,

    /// <summary>
    /// Identifies MSSQL server Enterprise Evaluation edition.
    /// </summary>
    /// <value><see langword="610778273"/></value>
    EnterpriseEvaluationEdition = 610778273,

    /// <summary>
    /// Identifies Windows Embedded SQL.
    /// </summary>
    /// <value><see langword="1044790755"/></value>
    WindowsEmbeddedSql = 1044790755,

    /// <summary>
    /// Identifies Express Edition with Advanced Services.
    /// </summary>
    /// <value><see langword="4161255391"/></value>
    ExpressEditionWithAdvancedServices = 4161255391
  }
}