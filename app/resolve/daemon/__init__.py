import requests
import time
import logging

import core.config as config
from core.execute_query import execute_query
from core.parser import parser
from core.rquest_dto.result import RquestResult
from core.settings import TASK_API_BASE_URL


def main() -> None:
    logger = logging.getLogger(config.LOGGER_NAME)
    result = execute_query(parser)
    if not isinstance(result, RquestResult):
        raise TypeError("Payload does not match RQuest result schema")

    return_url = f"{TASK_API_BASE_URL}/{result.uuid}/{result.collection_id}"

    for i in range(4):
        response = requests.post(return_url, data=result.to_dict())
        # Resolve will stop retrying to return the resolve when response was successful or there is a client error
        if 200 <= response.status_code < 300 or 400 <= response.status_code < 500:
            break
        else:
            logger.warning(f"Resolve failed to post to {return_url} at {time.time()}")
            time.sleep(5)
