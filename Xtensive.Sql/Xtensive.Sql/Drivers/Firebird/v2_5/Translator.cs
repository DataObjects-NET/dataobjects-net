// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.13

using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    internal class Translator : SqlTranslator
    {
        public override string DateTimeFormatString { get { return @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\'\)"; } }
        public override string TimeSpanFormatString { get { return "(INTERVAL '{0}{1} {2}:{3}:{4}.{5:000}' DAY(6) TO SECOND(3))"; } }

        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
