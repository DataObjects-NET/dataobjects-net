# Containers

Containers here are ment to be used for running tests. When built correctly after run they will have required database structure.
Dockerfiles and initialization scripts are grouped into folders and hereafter all examples of build commands assume that they are executed in context of one of the folders.

Across all instances of different RDBMSs we use 'dotest' user with 'dotest' password as cridentials for connection. In some cases user can be changed on container run, but not in all cases. We recommend using default settings, thought test connection password is weak it is fine for test environment and easy to remember.

## MS SQL Server

Dockerfiles are placed in 'mssql' folder.

### Build image

Assuming commands are executed in context of 'mssql' folder, images can be built as following

```console
docker buildx build -f do-mssql-2017 -t do-mssql:2017 .
```
```console
docker buildx build -f do-mssql-2019 -t do-mssql:2019 .
```
```console
docker buildx build -f do-mssql-2022 -t do-mssql:2022 .
```

Don't forget about the dot at the end.

More information about ```buildx build``` [in docker documentation](https://docs.docker.com/reference/cli/docker/buildx/build/).

### Run container

If images are built like in the examples above then containers can be run like so

```console
docker run --name DO_SQL2017 -h DO_SQL2017 -e ACCEPT_EULA=Y -e MSSQL_PID="Developer" -e MSSQL_SA_PASSWORD="<your sa password>" -e TZ=<timezone of host> -e MSSQL_INIT_WAIT=60 -p 1417:1433 -d do-mssql:2017
```
```console
docker run --name DO_SQL2019 -h DO_SQL2019 -e ACCEPT_EULA=Y -e MSSQL_PID="Developer" -e MSSQL_SA_PASSWORD="<your sa password>" -e TZ=<timezone of host> -e MSSQL_INIT_WAIT=60 -p 1419:1433 -d do-mssql:2019
```
```console
docker run --name DO_SQL2022 -h DO_SQL2022 -e ACCEPT_EULA=Y -e MSSQL_PID="Developer" -e MSSQL_SA_PASSWORD="<your sa password>" -e TZ=<timezone of host> -e MSSQL_INIT_WAIT=60 -p 1422:1433 -d do-mssql:2022
```

Here,

``` --name``` - name of the container, we use names like in the examples but you can choose any other name.

``` -h``` or ```--hostname``` - it will be required in connection strings, if you are familiar with MS SQL Server installation process then you probably know about named instances. The hosthame here will be name of instance.

``` -e ACCEPT_EULA=Y ``` - accepting EULA, more on [official docker page](https://hub.docker.com/r/microsoft/mssql-server)

``` -e MSSQL_PID="Developer"``` - the edition the container will run with. We develop so we use "Developer" edition (available options listed on [official docker page](https://hub.docker.com/r/microsoft/mssql-server) )

``` -e MSSQL_SA_PASSWORD="<your sa password>" ``` - password for 'sa' user. It should meet MS SQL Server password policies. (more on [official docker page](https://hub.docker.com/r/microsoft/mssql-server) )

``` -e TZ=<timezone of host>``` - Some of tests still assume that test runner and storage insance are in the same timezone. If you run and test locally on your machine, set it to host timezone, otherwise false falling tests appear.

``` -e MSSQL_INIT_WAIT=60 ``` - NOTE, this is not a standard variable. MS SQL Server takes some time to start and begin recieving connections, to not get false connection errors we need to wait for some time. This parameter defines how long we need to wait before first attempt to connect. Depending on how performant host machine is the value might require increase or let be decreased. Default wait time is 60 (seconds).

``` -p 1417:1433``` - host-to-container port mappings. If serveral containers are run on the same host they are required to have different host ports. We use following pattern - first two digits of default port (1433) and last two digits of MS SQL Server version e.g. 17 for 2017, 19 for 2019, so on.


### Connect to instance in docker

To access an instance run in docker use "```<pc name or ip>\hostname, <host port>```" as ```DataSource```, similar to connections to named instances.

Say, Docker's host has name is "WILLY" and containers run with commands above, then connection strings may look like:

```
Data Source=WILLY\DO_SQL2017,1417;Initial Catalog=DO-Tests;User Id=dotest;Password=dotest;MultipleActiveResultSets=True;
```
```
Data Source=WILLY\DO_SQL2019,1419;Initial Catalog=DO-Tests;User Id=dotest;Password=dotest;MultipleActiveResultSets=True;
```
```
Data Source=WILLY\DO_SQL2022,1422;Initial Catalog=DO-Tests;User Id=dotest;Password=dotest;MultipleActiveResultSets=True;
```



## PostgreSQL

Dockerfiles are placed in 'postgres' folder.

### Build image

Assuming commands are executed in context of 'postgres folder, images can be built with following commands

```console
docker buildx build -f do-postgre-9_0 -t do-postgres:9.0 .
```
```console
docker buildx build -f do-postgre-9_1 -t do-postgres:9.1 .
```
```console
docker buildx build -f do-postgre-9_2 -t do-postgres:9.2 .
```
```console
docker buildx build -f do-postgre-9_6 -t do-postgres:9.6 .
```
```console
docker buildx build -f do-postgre-10 -t do-postgres:10.0 .
```
```console
docker buildx build -f do-postgre-11 -t do-postgres:11.0 .
```
```console
docker buildx build -f do-postgre-12 -t do-postgres:12.0 .
```
```console
docker buildx build -f do-postgre-13 -t do-postgres:13.0 .
```
```console
docker buildx build -f do-postgre-14 -t do-postgres:14.0 .
```
```console
docker buildx build -f do-postgre-15 -t do-postgres:15.0 .
```


NOTE Images older than 9.6 are unable to update tzdata so some tests can fail due to specific way of working with offset on ```TIMESTAMP WITH TIMEZONE```.


### Run container

Assuming the images are built like in examples above containers can be run like so

```console
docker run --name postgre-9.0 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 5490:5432 -d do-postgres:9.0
```
```console
docker run --name postgre-9.1 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 5491:5432 -d do-postgres:9.1
```
```console
docker run --name postgre-9.2 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 5492:5432 -d do-postgres:9.2
```
```console
docker run --name postgre-9.6 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 5496:5432 -d do-postgres:9.6
```
```console
docker run --name postgre-10 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54100:5432 -d do-postgres:10.0
```
```console
docker run --name postgre-11 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54110:5432 -d do-postgres:11.0
```
```console
docker run --name postgre-12 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54120:5432 -d do-postgres:12.0
```
```console
docker run --name postgre-13 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54130:5432 -d do-postgres:13.0
```
```console
docker run --name postgre-14 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54140:5432 -d do-postgres:14.0
```
```console
docker run --name postgre-15 -e POSTGRES_PASSWORD=<your password> -e POSTGRES_HOST_AUTH_METHOD=md5 -e TZ=<timezone of host> -p 54150:5432 -d do-postgres:15.0
```


Here,

``` --name postgre-15``` - name of the container

```-e POSTGRES_PASSWORD=<your password>``` - superuser password, required by base image.

```-e POSTGRES_HOST_AUTH_METHOD=md5``` - option that controlls 'auth-method'. For test purposes 'md5' ok.

```-e TZ=<timezone of host>``` - Some of tests still assume that test runner and storage insance are in the same timezone. If you run and test locally on your machine, set it to host timezone, otherwise false falling tests appear.

```-p 54150:5432``` - host-to-container port mappings. If serveral containers are run on the same host they require to have different ports. We use following pattern - first two digits of standard port (5432) and PostgreSQL version after that e.g. 150 for 15.0, 96 for 9.6.


During first run of container database structure and users will be created. By default it creates 'dotest' user, 'dotest' database with several schemas within.
We intentionally don't use superuser as owner of database and user for connections, if you change superuser name by using ```POSTGRES_USER``` variable, don't name it 'dotest', otherwise conflict will occur.


More information about standard options [on official image page on docker hub](https://hub.docker.com/_/postgres)


### Connect to instance in docker

To access an instance run in docker use host name and the port you have mapped container to. If we have docker on the same "WILLY" host then connection strings may look like:

```
HOST=WILLY;PORT=5490;DATABASE=dotest;USER ID=dotest;PASSWORD=dotest;
```
```
HOST=WILLY;PORT=5496;DATABASE=dotest;USER ID=dotest;PASSWORD=dotest;
```
```
HOST=WILLY;PORT=54120;DATABASE=dotest;USER ID=dotest;PASSWORD=dotest;
```
```
"HOST=WILLY;PORT=54150;DATABASE=dotest;USER ID=dotest;PASSWORD=dotest;
```

for containers postgre-9.0, postgre-9.6, postgre-12, postgre-15 respectively, you get the idea.


# MySQL

Dockerfiles are located in 'mysql' folder.

### Build image

Assuming commands are executed in context of 'mysql' folder, images can be built with following commands


```console
docker buildx build -f do-mysq-5_5 -t do-mysql:5.5 .
```
```console
docker buildx build -f do-mysq-5_6 -t do-mysql:5.6 .
```
```console
docker buildx build -f do-mysq-5_7 -t do-mysql:5.7 .
```
```console
docker buildx build -f do-mysq-8_0 -t do-mysql:8.0 .
```


### Run container


```console
docker run --name mysql-5.5 -p 3355:3306 -e MYSQL_ROOT_PASSWORD=<your password> -e MYSQL_DATABASE=dotest -e MYSQL_USER=dotest -e MYSQL_PASSWORD=dotest -d do-mysql:5.5
```
```console
docker run --name mysql-5.6 -p 3356:3306 -e MYSQL_ROOT_PASSWORD=<your password> -e MYSQL_DATABASE=dotest -e MYSQL_USER=dotest -e MYSQL_PASSWORD=dotest -d do-mysql:5.6
```
```console
docker run --name mysql-5.7 -p 3357:3306 -e MYSQL_ROOT_PASSWORD=<your password> -e MYSQL_DATABASE=dotest -e MYSQL_USER=dotest -e MYSQL_PASSWORD=dotest -d do-mysql:5.7
```
```console
docker run --name mysql-8.0 -p 3380:3306 -e MYSQL_ROOT_PASSWORD=<your password> -e MYSQL_DATABASE=dotest -e MYSQL_USER=dotest -e MYSQL_PASSWORD=dotest -d do-mysql:8.0
```

Here,

``` --name mysql-8.0``` - name of container, can be changed, affects nothing

``` -p 3380:3306``` - host-to-container port mapping. We use following pattern - two first digits from default port (3306) folowed by version of MySQL e.g. 55 for MySQL 5.5, 80 for MySQL.

``` -e MYSQL_ROOT_PASSWORD=<your password>``` - root password, required by base image.

``` -e MYSQL_DATABASE=dotest``` - database name to be created, can be changed (don't forget to change connection string) or omitted (initialization script will create 'dotest' database).

``` -e MYSQL_USER=dotest```- user name to be created for connections, can be changed (don't forget to change connection string) or omitted (initialization script will create 'dotest' user with 'dotest' password).

``` -e MYSQL_PASSWORD=dotest``` - password, required if MYSQL_USER is defined.

More information about standard options [on official image page on docker hub](https://hub.docker.com/_/mysql)



### Connect to instance in docker

To access an instance run in docker use host name and port you have mapped container to. If we have docker on the same "WILLY" host then connection strings may look like:

```
Server=WILLY;Port=3355;Database=dotest;Uid=dotest;Pwd=dotest;Default Command Timeout=120;
```
```
Server=WILLY;Port=3356;Database=dotest;Uid=dotest;Pwd=dotest;Default Command Timeout=120;
```
```
Server=WILLY;Port=3357;Database=dotest;Uid=dotest;Pwd=dotest;Default Command Timeout=120;
```
```
Server=WILLY;Port=3380;Database=dotest;Uid=dotest;Pwd=dotest;Default Command Timeout=120;
```


# Firebird

Dockerfiles are located in 'firebird' folder.

### Build image

Assuming commands are executed in context of 'firebird' folder, image can be built with following commands


```console
docker buildx build -f do-firebird-3_0 -t do-firebird:3.0 .
```


### Run container

Assuming the image is built like in the example above container can be run like so


```console
docker run --name firebird-3 -p 3053:3050 -e FIREBIRD_ROOT_PASSWORD=<your password> -e FIREBIRD_USER=dotest -e FIREBIRD_PASSWORD=dotest -e FIREBIRD_DATABASE=DOTEST.fdb -e FIREBIRD_DATABASE_PAGE_SIZE=8192 -d do-firebird:3.0
```

Here,

```--name firebird-3``` - name of contaner, you can choose different name, it doesn't really matter.

```-e FIREBIRD_ROOT_PASSWORD=<your password>``` - root user password.

```-e FIREBIRD_USER=dotest``` - user for connections, can be changed.

```-e FIREBIRD_PASSWORD=dotest``` - user password. Required when user defined.

```-e FIREBIRD_DATABASE=DOTEST.fdb``` - database file name to create.

``` -e FIREBIRD_DATABASE_PAGE_SIZE=8192``` - page size for database.

```-p 3053:3050``` - host-to-container port mapping. We use following pattern - first three digits of standard port (3050) and  major version of Firebird.

Pair ```FIREBIRD_USER``` / ```FIREBIRD_PASSWORD``` can be omitted, in this case initialization script will handle it and create 'dotest' user with 'dotest' password.

More information about standard options [on official image page on docker hub](https://hub.docker.com/r/firebirdsql/firebird)


### Connect to instance in docker

To access an instance run in docker use host name and port you have mapped container to. If we have docker on the same "WILLY" host then connection string may look like:

```
User=dotest;Password=dotest;Database=dotest;DataSource=WILLY;Port=3053;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0
```

If you changed user/password for connections then don't forget to update it in connection string.


