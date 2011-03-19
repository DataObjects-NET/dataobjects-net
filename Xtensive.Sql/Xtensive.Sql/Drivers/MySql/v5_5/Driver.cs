// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.20

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;


namespace Xtensive.Sql.MySql.v5_5
{
    internal class Driver : MySql.Driver
    {
        protected override Sql.TypeMapper CreateTypeMapper()
        {
            return new TypeMapper(this);
        }

        protected override SqlCompiler CreateCompiler()
        {
            return new Compiler(this);
        }

        protected override SqlTranslator CreateTranslator()
        {
            return new Translator(this);
        }

        protected override Model.Extractor CreateExtractor()
        {
            return new Extractor(this);
        }

        protected override Info.ServerInfoProvider CreateServerInfoProvider()
        {
            return new ServerInfoProvider(this);
        }

        // Constructors

        public Driver(CoreServerInfo coreServerInfo)
            : base(coreServerInfo)
        {
        }
    }
}
