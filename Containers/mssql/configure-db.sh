#!/bin/bash

# Wait 60 seconds (or amount of time requested on run) for SQL Server to start up and start accepting connections.
# Next, wait for same periond to let databases to reach working state by ensuring that
# calling SQLCMD does not return an error code, which will ensure that sqlcmd is accessible
# and that system and user databases return "0" which means all databases are in an "online" state
# https://docs.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-databases-transact-sql?view=sql-server-2017 

if [ -f /var/opt/mssql/data/DO-Tests.mdf ]; then
    echo "INFO: Databases already initialized. Skip database initialization."
    exit 0
else
    echo "INFO: Databases don't exist. Trying to initialize"
fi

INIT_WAIT=60
[ -n "$MSSQL_INIT_WAIT" ] && INIT_WAIT=$MSSQL_INIT_WAIT

echo "INFO: Initial wait for $INIT_WAIT secs to let SQL Server start and accept connections."
sleep $INIT_WAIT;

DBSTATUS=1
ERRCODE=1
i=0

#Different versions of Sql Server images have sqlcmd in different places
if [ -d "/opt/mssql-tools18/" ]; then
    echo "INFO: Trying to login and check databases' states. If the databases are not ready wait for the same time..."
    while [[ $i -lt $INIT_WAIT ]] && [[ $ERRCODE -ne 0 ]]; do
        ((i=i+1))
        DBSTATUS=$(/opt/mssql-tools18/bin/sqlcmd -h -1 -t 1 -U sa -P $MSSQL_SA_PASSWORD -C -Q "SET NOCOUNT ON; Select SUM(state) from sys.databases")
        ERRCODE=$?
    sleep 1
    done

    if [ $ERRCODE -ne 0 ] OR [ $DBSTATUS -ne 0 ]; then 
        echo "INFO: SQL Server took more than 120 seconds to start up or one or more databases are not in an ONLINE state"
        exit 1
    fi

    # Run the setup script to create the DB and the schema in the DB
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -d master -i setup.sql

else
    echo "INFO: Trying to login and check databases' states. If the databases are not ready wait for the same time..."
    while [[ $i -lt $INIT_WAIT ]] && [[ $ERRCODE -ne 0 ]]; do
        ((i=i+1))
        DBSTATUS=$(/opt/mssql-tools/bin/sqlcmd -h -1 -t 1 -U sa -P $MSSQL_SA_PASSWORD -Q "SET NOCOUNT ON; Select SUM(state) from sys.databases")
        ERRCODE=$?
        sleep 1
    done

    if [ $ERRCODE -ne 0 ] OR [ $DBSTATUS -ne 0 ]; then 
        echo "INFO: SQL Server took more than 120 seconds to start up or one or more databases are not in an ONLINE state"
        exit 1
    fi

    # Run the setup script to create the DB and the schema in the DB
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -d master -i setup.sql
fi