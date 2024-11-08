import os
import sys
import logging
from typing import Dict, List

import core.settings as settings
from core import query_solvers
from core.rquest_dto.query import AvailabilityQuery, DistributionQuery
from core.obfuscation import (
    apply_filters_v2,
)
from core.db_manager import SyncDBManager, TrinoDBManager
from core.rquest_dto.result import RquestResult


def execute_query(query_dict: Dict, result_modifiers: List) -> RquestResult:
    """
    Executes either an availability query or a distribution query, and returns results filtered by modifiers

    Parameters
    ----------
    query_dict: Dict
        A dictionary carrying the payload for the query. If there is an 'analysis' item in the query, it's a distribution query. Otherwise, it executes an availability query
    result_modifers: List
        A list of modifiers applied to the results of the query before returning them to Relay

    Returns
        RquestResult
    """
    # Set up the logger
    LOG_FORMAT = logging.Formatter(
        settings.MSG_FORMAT,
        datefmt=settings.DATE_FORMAT,
    )
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(LOG_FORMAT)
    logger = logging.getLogger(settings.LOGGER_NAME)
    logger.setLevel(logging.INFO)
    logger.addHandler(console_handler)

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

    logger.info("Processing query...")

    if "analysis" in query_dict.keys():
        try:
            query = DistributionQuery.from_dict(query_dict)
            result = query_solvers.solve_distribution(
                db_manager=db_manager, query=query
            )
            return result
        except TypeError as te:  # raised if the distribution query json format is wrong
            logger.error(str(te), exc_info=True)
        except ValueError as ve:
            # raised if there was an issue saving the output or
            # the query json has incorrect values
            logger.error(str(ve), exc_info=True)

    else:
        try:
            query = AvailabilityQuery.from_dict(query_dict)
            result = query_solvers.solve_availability(
                db_manager=db_manager, query=query
            )
            result.count = apply_filters_v2(result.count, result_modifiers)
            return result
        except TypeError as te:  # raised if the distribution query json format is wrong
            logger.error(str(te), exc_info=True)
        except ValueError as ve:
            # raised if there was an issue saving the output or
            # the query json has incorrect values
            logger.error(str(ve), exc_info=True)
