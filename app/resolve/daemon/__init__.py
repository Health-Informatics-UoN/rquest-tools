import requests
import time
import logging
from requests.auth import HTTPBasicAuth
import core.settings as settings
from core.execute_query import execute_query
from core.parser import parser
from core.rquest_dto.result import RquestResult
from core.settings import TASK_API_BASE_URL, USERNAME, PASSWORD


def main() -> None:
    basicAuth = HTTPBasicAuth(USERNAME, PASSWORD)
    logger = logging.getLogger(settings.LOGGER_NAME)
    result = execute_query(parser)
    if not isinstance(result, RquestResult):
        raise TypeError("Payload does not match RQuest result schema")

    return_url = f"{TASK_API_BASE_URL}/{result.uuid}/{result.collection_id}"

    for i in range(4):
        response = requests.post(return_url, data=result.to_dict(), auth=basicAuth)
        # Resolve will stop retrying to post results when response was successful or there is a client error
        if 200 <= response.status_code < 300 or 400 <= response.status_code < 500:
            break
        else:
            logger.warning(f"Resolve failed to post to {return_url} at {time.time()}")
            time.sleep(5)
