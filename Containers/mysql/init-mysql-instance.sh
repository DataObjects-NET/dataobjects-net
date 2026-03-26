#!/bin/bash

set -e
set -u

create_database(){
    if [ -n "$MYSQL_DATABASE" ]; then
        echo "Database has been declared on container run. Skip creation of $1 database"
    else
        mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e "create database $1 character set utf8;"
    fi
}

# Tries to create main user for test database instance, required by tests setup.
# If user was declared on container creation, this method skips creation and just
# grants privileges to declared user to database from parameter or to declared in
# environment variable
#Parameters
#    $1 - user name to be created
#    $2 - database name to grant privileges on
create_main_user(){
    local database="$2"
    [ -n "$MYSQL_DATABASE" ] && database="$MYSQL_DATABASE"

    local user="$1"
    [ -n "$MYSQL_USER" ] && user="$MYSQL_USER"
    if [ -ne "$MYSQL_USER"]; then
        echo "Creating user $user"
        mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e 'create user "'"$user"'"@"'"%"'" identified by "'"$user"'";'
    else
        echo "User was declared on container run as "MYSQL_USER". Skip creatation of user $1"
    fi

    echo "Try grant all privileges on $database database to user $user ..."
    mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e 'grant all privileges on *.* to "'"$user"'"@"'"%"'" WITH GRANT OPTION;'
    mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e 'FLUSH PRIVILEGES;'
    echo "Privileges granted"
}

# Tries to create a user with read-only permitions, user required by some tests.
#Parameters
#    $1 - user name to be created
#    $2 - database name to grant privileges on
create_readonly_user(){
    if [ "$1" == "$MYSQL_USER" ];then
        echo "read-only user name matches the main user. Skip creation and granting privileges for $1 (  $MYSQL_USER  )"
    else
        local database="$2"
        [ -n "$MYSQL_DATABASE" ] && database="$MYSQL_DATABASE"

        local user="$1"

        echo "Creating user $user"
        mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e 'create user "'"$1"'"@"'"%"'" identified by "'"$1"'";'

        echo "Try grant read-only privileges on $database database to user $user ..."
        mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e 'grant create view, execute, select, show view on '"$database"'.* to "'"$user"'"@"'"%"'";'
        echo "Privileges granted"
    fi
}

create_database "dotest"

create_main_user "dotest" "dotest"

create_readonly_user "readonlydotest" "dotest"

echo "Current list of users..."
mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "mysql" -e "SELECT user, host FROM mysql.user;"
echo "----------------------------------------------"

# fix "variable unbound" error within mysql entrypoint :-)
export MYSQL_ONETIME_PASSWORD=''