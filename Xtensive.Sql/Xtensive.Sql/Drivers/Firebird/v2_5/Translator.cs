// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{

    internal class Translator : SqlTranslator
    {
        public override string DateTimeFormatString { get { return @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fff\'' as timestamp)'"; } }
        public override string TimeSpanFormatString { get { throw new System.NotImplementedException("There is no timespan datatype in Firebird"); } }

        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
