#!/bin/bash
set -e
set -u

create_user_and_database() {
    if [ "dotest" == "$POSTGRES_USER" ]; then
        echo "CONFLICT! POSTGRES root user is the same as created by the init script. Change user name"
        return 1
    else
        psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
        CREATE ROLE dotest WITH
          LOGIN
          SUPERUSER
          INHERIT
          CREATEDB
          CREATEROLE
          ENCRYPTED PASSWORD 'md54914e0f5dbdc04a925392ec05a1a0b91' VALID UNTIL 'infinity';
          CREATE DATABASE dotest WITH OWNER = dotest ENCODING = 'UTF8' TABLESPACE = pg_default CONNECTION LIMIT = -1;
          GRANT ALL PRIVILEGES ON DATABASE dotest TO dotest;
EOSQL
        return 0;
    fi
}

create_shemas() {
    psql -v ON_ERROR_STOP=1 --username "dotest" --dbname "dotest" <<-EOSQL
        CREATE SCHEMA "Model1" AUTHORIZATION dotest;
        CREATE SCHEMA "Model2" AUTHORIZATION dotest;
        CREATE SCHEMA "Model3" AUTHORIZATION dotest;
        CREATE SCHEMA "Model4" AUTHORIZATION dotest;
        CREATE SCHEMA "Model5" AUTHORIZATION dotest;
        CREATE SCHEMA "dbo" AUTHORIZATION dotest;
        CREATE SCHEMA "n1" AUTHORIZATION dotest;
        CREATE SCHEMA "n2" AUTHORIZATION dotest;
        CREATE SCHEMA "n3" AUTHORIZATION dotest;
        CREATE SCHEMA "test1" AUTHORIZATION dotest;
        CREATE SCHEMA "test2" AUTHORIZATION dotest;
        CREATE SCHEMA "test3" AUTHORIZATION dotest;
        CREATE SCHEMA "test4" AUTHORIZATION dotest;
EOSQL
}


create_user_and_database
if [ $? -eq 1 ]; then
    echo "User and database creation failed. Exit"
    exit 1
fi

create_shemas