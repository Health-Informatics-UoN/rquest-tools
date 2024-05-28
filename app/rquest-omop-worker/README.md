# RQuest OMOP worker

## Use with postgres
The following environment variables should be set with running against a postgres database:

```
DATASOURCE_DB_USERNAME=<username>
DATASOURCE_DB_PASSWORD=<password>
DATASOURCE_DB_HOST=<hostname>
DATASOURCE_DB_PORT=<port number>
DATASOURCE_DB_DATABASE=<db name>
DATASOURCE_DB_DRIVERNAME=postgresql+psycopg2
DATASOURCE_DB_SCHEMA=public
```

## Use with Trino

The following environment variables should be set with running against Trino:

```
DATASOURCE_DB_USERNAME=postgres
DATASOURCE_DB_HOST=<trino host>
DATASOURCE_DB_PORT=<port number>
DATASOURCE_DB_CATALOG=<trino catalog>
DATASOURCE_DB_DATABASE=<db in catalog>
DATASOURCE_DB_SCHEMA=<schema in catalog>
USE_TRINO=True
```
