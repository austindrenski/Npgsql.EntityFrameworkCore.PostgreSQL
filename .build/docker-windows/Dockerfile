FROM stellirin/postgres-windows as upstream
FROM mcr.microsoft.com/windows/servercore:1809
LABEL description="Npgsql CI build image"

ARG PG_VERSION=11
ENV PG_VERSION ${PG_VERSION}
ENV POSTGIS_VERSION 2.5.3

COPY --from=upstream /pgsql /pgsql
COPY --from=upstream docker-entrypoint.cmd /

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Invoke-WebRequest -Uri "http://download.osgeo.org/postgis/windows/pg${env:PG_VERSION}/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64.zip" -OutFile "/postgis.zip"; \
    Expand-Archive "/postgis.zip" "/postgis/";

RUN mv "/postgis/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64/*" "/postgis"; \
    rm "postgis/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64"; \
    cp "/postgis/*" "/pgsql" -Recurse -Force; \
    rm "/postgis*" -Recurse -Force;

# Data directory needs to be empty for initialization scripts to run, so stash one level up.
COPY ./server.crt /pgsql/data/../server.crt
COPY ./server.key /pgsql/data/../server.key
# RUN chown root:postgres $PGDATA/../server.key
# RUN chmod 0640 $PGDATA/../server.key

RUN mkdir /docker-entrypoint-initdb.d
COPY ./initdb-npgsql.cmd /docker-entrypoint-initdb.d/initdb-npgsql.cmd

#### In order to set system PATH, ContainerAdministrator must be used
USER ContainerAdministrator
RUN setx /M PATH "/pgsql/bin;${env:Path}"
USER ContainerUser
ENV PGDATA "/pgsql/data"

ENTRYPOINT ["/docker-entrypoint.cmd"]

EXPOSE 5432

CMD ["postgres", "-c", "max_prepared_transactions=10", "-c", "ssl=true", "-c", "ssl_cert_file=../server.crt", "-c", "ssl_key_file=../server.key"]