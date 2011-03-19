// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.20

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.MySql.v5_5
{
    internal class Translator : v5_1.Translator
    {
   
        /// <inheritdoc/>
        public override string Translate(SqlFunctionType type)
        {
            switch (type)
            {
                case SqlFunctionType.CurrentTimeStamp:
                    return "NOW()";
                default:
                    return base.Translate(type);
            }
        }

        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
