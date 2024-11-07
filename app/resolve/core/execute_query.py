import argparse
import os
import sys
import logging
import json

import core.config as config
from core import query_solvers
from core.rquest_dto.query import AvailabilityQuery, DistributionQuery
from core.obfuscation import (
    get_results_modifiers_from_str,
    apply_filters_v2,
)
from core.db_manager import SyncDBManager, TrinoDBManager
from core.rquest_dto.result import RquestResult


def execute_query(parser: argparse.ArgumentParser) -> RquestResult:
    # Set up the logger
    LOG_FORMAT = logging.Formatter(
        config.MSG_FORMAT,
        datefmt=config.DATE_FORMAT,
    )
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(LOG_FORMAT)
    logger = logging.getLogger(config.LOGGER_NAME)
    logger.setLevel(logging.INFO)
    logger.addHandler(console_handler)

    # parse command line arguments
    args = parser.parse_args()

    # check only of -a or -d is given
    if args.is_availability and args.is_distribution:
        logger.error("Only one of `-a` or `-d` can be specified at once.")
        parser.print_help()
        exit()

    # check one of -a or -d is given
    if not (args.is_availability or args.is_distribution):
        logger.error("Specify one of `-a` or `-d`.")
        parser.print_help()
        exit()

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
                    "DATASOURCE_DB_DRIVERNAME", config.DEFAULT_DB_DRIVER
                ),
                schema=os.getenv("DATASOURCE_DB_SCHEMA"),
            )
        except TypeError as e:
            logger.error(str(e))
            exit()

    logger.info("Processing query...")
    with open(args.body) as body:
        query_dict = json.load(body)
    result_modifers = get_results_modifiers_from_str(args.results_modifiers)
    if args.is_availability:
        try:
            query = AvailabilityQuery.from_dict(query_dict)
            result = query_solvers.solve_availability(
                db_manager=db_manager, query=query
            )
            result.count = apply_filters_v2(result.count, result_modifers)
            return result
        except TypeError as te:  # raised if the distribution query json format is wrong
            logger.error(str(te), exc_info=True)
        except ValueError as ve:
            # raised if there was an issue saving the output or
            # the query json has incorrect values
            logger.error(str(ve), exc_info=True)
    else:
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
