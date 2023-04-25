FROM mcr.microsoft.com/mssql/server:2022-latest
USER root

ENV ACCEPT_EULA=Y \
    MSSQL_SA_PASSWORD=dbatools.I0

RUN echo >>/etc/apt/sources.list "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list)" \
    && apt-get update \
    && apt-get install -y apt-utils mssql-server-fts




