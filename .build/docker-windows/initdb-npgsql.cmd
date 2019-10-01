: Perform all actions as $POSTGRES_USER
set PGUSER=%POSTGRES_USER%

: Standard test account for Npgsql
psql -c "CREATE ROLE npgsql_tests SUPERUSER LOGIN PASSWORD 'npgsql_tests';"
psql -c "CREATE DATABASE npgsql_tests OWNER npgsql_tests;"

: Domain account for Azure Pipelines.
psql -c "CREATE ROLE vsts SUPERUSER LOGIN;"
psql -c "CREATE DATABASE vsts OWNER vsts;"

: TODO: Something is wrong with the raster setup (missing dep?)
:       so we need to configure PostGIS manually.
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\postgis.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\postgis_comments.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\spatial_ref_sys.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\topology.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\topology_comments.sql

: -- only if you compiled with raster (GDAL)
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\rtpostgis.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\raster_comments.sql

: --if you built with sfcgal support --
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\sfcgal.sql
: psql -d npgsql_tests -f C:\pgsql\share\contrib\postgis-2.5\sfcgal_comments.sql
