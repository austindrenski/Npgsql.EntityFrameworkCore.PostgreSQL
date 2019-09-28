$CONTEXT_DIR="$PSScriptRoot/docker-windows"

docker build -t npgsql/postgres-windows:11  --build-arg PG_VERSION=11  $CONTEXT_DIR
docker build -t npgsql/postgres-windows:10  --build-arg PG_VERSION=10  $CONTEXT_DIR
docker build -t npgsql/postgres-windows:9.6 --build-arg PG_VERSION=9.6 $CONTEXT_DIR

docker push npgsql/postgres-windows:11
docker push npgsql/postgres-windows:10
docker push npgsql/postgres-windows:9.6
