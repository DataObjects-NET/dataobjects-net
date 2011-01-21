﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;

namespace Xtensive.Sql.Drivers.Firebird
{
    internal static class Constants
    {
        public const string DefaultSchemaName = ""; // "Firebird";

        public const string TimeSpanFormatString = @"{0}{1},{2},{3},{4},{5:000}";
        public const string DateTimeFormatString = @"'cast ('\'yyyy\.MM\.dd HH\:mm\:ss\'' as timestamp)'";
    }
}
