from os import environ
from core.db_manager import SyncDBManager
import core.settings as settings
from core.rquest_dto.query import AvailabilityQuery, DistributionQuery

def set_db_manager():
    datasource_db_port = environ.get("DATASOURCE_DB_PORT")
    db_manager = SyncDBManager(
        username=environ.get("DATASOURCE_DB_USERNAME"),
        password=environ.get("DATASOURCE_DB_PASSWORD"),
        host=environ.get("DATASOURCE_DB_HOST"),
        port=(
            int(datasource_db_port) if datasource_db_port is not None else None
        ),
        database=environ.get("DATASOURCE_DB_DATABASE"),
        drivername=environ.get(
            "DATASOURCE_DB_DRIVERNAME", settings.DEFAULT_DB_DRIVER
        ),
        schema=environ.get("DATASOURCE_DB_SCHEMA"),
    )

def send_query():
    query = AvailabilityQuery.from_dict(query_dict) ## query_dict is the body of the cli (the payload)
    result = query_solvers.solve_availability(db_manager=db_manager, query=query)

    query = DistributionQuery.from_dict(query_dict)
    result = query_solvers.solve_distribution(db_manager=db_manager, query=query)
