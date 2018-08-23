// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

namespace Xtensive.Orm.Weaver
{
  public enum MessageCode
  {
    Unknown = 0,

    ErrorInternal = 1,
    ErrorInputFileIsNotFound = 2 ,
    ErrorStrongNameKeyIsNotFound = 3,
    ErrorUnableToLocateOrmAssembly = 4,
    ErrorUnableToFindReferencedAssembly = 5,
    ErrorUnableToRemoveBackingField = 6,
    ErrorEntityLimitIsExceeded = 7,
    ErrorLicenseIsInvalid = 8,
    ErrorSubscriptionExpired = 9,
    ErrorPersistentPropertiesWereNotProcessed = 10,

    WarningDebugSymbolsFileIsNotFound = 1000,
    WarningReferencedAssemblyFileIsNotFound = 1001,
    WarningPersistentPropertyHasNoSetterOrGetter =1002,
  }
}