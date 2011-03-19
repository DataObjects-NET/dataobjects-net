// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.20

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.MySql.v5_5
{
    internal class Extractor : v5_1.Extractor
    {

        // Constructors

        public Extractor(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
