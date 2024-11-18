from logging import Logger
import os
from core.db_manager import SyncDBManager, TrinoDBManager
import core.settings as settings


def setting_database(logger: Logger):
    logger.info("Setting up database connection...")
    if bool(os.getenv("USE_TRINO", False)):
        datasource_db_port = os.getenv("DATASOURCE_DB_PORT", 8080)
        try:
            db_manager = TrinoDBManager(
                username=os.getenv("DATASOURCE_DB_USERNAME", "trino-user"),
                password=os.getenv("DATASOURCE_DB_PASSWORD"),
                host=os.getenv("DATASOURCE_DB_HOST"),
                port=int(datasource_db_port),
                schema=os.getenv("DATASOURCE_DB_SCHEMA"),
                catalog=os.getenv("DATASOURCE_DB_CATALOG"),
            )
        except TypeError as e:
            logger.error(str(e))
            exit()
    else:
        datasource_db_port = os.getenv("DATASOURCE_DB_PORT")
        try:
            db_manager = SyncDBManager(
                username=os.getenv("DATASOURCE_DB_USERNAME"),
                password=os.getenv("DATASOURCE_DB_PASSWORD"),
                host=os.getenv("DATASOURCE_DB_HOST"),
                port=(
                    int(datasource_db_port) if datasource_db_port is not None else None
                ),
                database=os.getenv("DATASOURCE_DB_DATABASE"),
                drivername=os.getenv(
                    "DATASOURCE_DB_DRIVERNAME", settings.DEFAULT_DB_DRIVER
                ),
                schema=os.getenv("DATASOURCE_DB_SCHEMA"),
            )
        except TypeError as e:
            logger.error(str(e))
            exit()

    return db_manager
