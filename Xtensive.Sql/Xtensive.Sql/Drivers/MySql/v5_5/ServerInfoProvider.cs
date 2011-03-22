// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.20

using Xtensive.Sql.Info;

namespace Xtensive.Sql.MySql.v5_5
{
    internal class ServerInfoProvider : v5_1.ServerInfoProvider
    {
        private const int MaxIdentifierLength = 128;


        /// <inheritdoc/>
        public override TableInfo GetTableInfo()
        {
            var tableInfo = new TableInfo();
            tableInfo.MaxIdentifierLength = MaxIdentifierLength;
            tableInfo.AllowedDdlStatements = DdlStatements.All;
            //From version  5.1.14
            tableInfo.PartitionMethods = PartitionMethods.Hash | PartitionMethods.Range | PartitionMethods.List;
            return tableInfo;
        }

        // Constructors

        public ServerInfoProvider(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
