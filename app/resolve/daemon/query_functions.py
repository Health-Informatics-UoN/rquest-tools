import time
from core.execute_query import execute_query
from core.rquest_dto.result import RquestResult
from core.logger import logger
import payload


def http_success(response):
    ### note that any other failure will stay in the infinite polling loop
    if response.status_code == 204:
        logger.info("Looking for job...")
    return response.status_code == 200


def query(db_manager, response, modifiers_list):
    logger.info("Job received. Resolving...")

    # Convert Response to Dict
    query_dict: dict = response.json()

    # Start querying
    result = execute_query(
        query_dict,
        results_modifiers=modifiers_list,
        logger=logger,
        db_manager=db_manager,
    )

    # Check the payload shape
    if not isinstance(result, RquestResult):
        raise TypeError("Payload does not match RQuest result schema.")

    return_data = payload.Payload(result)

    return return_data


def return_poll_success(response):
    if (
        200 <= response.status_code < 300
        or 400 <= response.status_code < 500
    ):
        logger.info("Job resolved.")
        return True
    else:
        logger.warning(f"Resolve failed to post to {data.return_endpoint} at {time.time()}. Trying again...")
        return False
