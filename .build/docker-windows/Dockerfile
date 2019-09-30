FROM stellirin/postgres-windows as upstream
FROM mcr.microsoft.com/windows/servercore:1809
LABEL description="Npgsql CI build image"

ARG PG_VERSION="11"
ARG EDB_VERSION="11.4-2"
ARG POSTGIS_VERSION="2.5.3"

ENV PG_VERSION ${PG_VERSION}
ENV EDB_VERSION ${EDB_VERSION}}
ENV POSTGIS_VERSION ${POSTGIS_VERSION}

# COPY --from=upstream /pgsql /pgsql
COPY --from=upstream docker-entrypoint.cmd /

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Invoke-WebRequest -Uri "https://get.enterprisedb.com/postgresql/postgresql-11.4-2-windows-x64-binaries.zip" -OutFile '/postgresql.zip'; \
    Expand-Archive '/postgresql.zip' -DestinationPath '/'; \
    Remove-Item -Path '/postgresql.zip' 

# Make the sample config easier to munge (and "correct by default")
RUN $SAMPLE_FILE = '/pgsql/share/postgresql.conf.sample'; \
    $SAMPLE_CONF = Get-Content $SAMPLE_FILE ; \
    $SAMPLE_CONF = $SAMPLE_CONF -Replace '#listen_addresses = ''localhost''','listen_addresses = ''*'''; \
    $SAMPLE_CONF | Set-Content $SAMPLE_FILE

# Install correct Visual C++ Redistributable Package
RUN if (($env:EDB_VER -eq '9.4.22-1') -or ($env:EDB_VER -eq '9.5.17-1') -or ($env:EDB_VER -eq '9.6.13-1') -or ($env:EDB_VER -eq '10.8-1')) { \
        Write-Host('Visual C++ 2013 Redistributable Package') ; \
        $URL2 = 'https://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe' ; \
    } else { \
        Write-Host('Visual C++ 2017 Redistributable Package') ; \
        $URL2 = 'https://download.visualstudio.microsoft.com/download/pr/11100230/15ccb3f02745c7b206ad10373cbca89b/VC_redist.x64.exe' ; \
    } ; \
    Invoke-WebRequest -Uri $URL2 -OutFile 'C:\\vcredist.exe' ; \
    Start-Process 'C:\\vcredist.exe' -Wait \
        -ArgumentList @( \
            '/install', \
            '/passive', \
            '/norestart' \
        )

RUN Invoke-WebRequest -Uri "http://download.osgeo.org/postgis/windows/pg${env:PG_VERSION}/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64.zip" -OutFile "/postgis.zip"; \
    Expand-Archive "/postgis.zip" "/postgis/"; \
    Remove-Item -Path '/postgis.zip' 

RUN mv "/postgis/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64/*" "/postgis"; \
    rm "postgis/postgis-bundle-pg${env:PG_VERSION}-${env:POSTGIS_VERSION}x64"; \
    cp "/postgis/*" "/pgsql" -Recurse -Force; \
    rm "/postgis*" -Recurse -Force

RUN cp "/pgsql/bin/postgisgui/libeay32.dll" "/pgsql/bin" -Force; \
    cp "/pgsql/bin/postgisgui/ssleay32.dll" "/pgsql/bin" -Force;

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
