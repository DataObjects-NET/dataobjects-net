// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

namespace Xtensive.Orm.Weaver
{
  internal enum MessageCode
  {
    Unknown = 0,

    ErrorInternal = 1,
    ErrorInputFileIsNotFound,
    ErrorStrongNameKeyIsNotFound,
    ErrorUnableToLocateOrmAssembly,
    ErrorUnableToFindReferencedAssembly,
    ErrorUnableToRemoveBackingField,
    ErrorEntityLimitIsExceeded,
    ErrorLicenseIsInvalid,
    ErrorSubscriptionExpired,

    WarningDebugSymbolsFileIsNotFound = 1000,
    WarningReferencedAssemblyFileIsNotFound,
  }
}