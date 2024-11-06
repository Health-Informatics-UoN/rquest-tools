import requests
from os import environ
import json
import time
import logging

import core.config as config
from core.execute_query import execute_query
from core.parser import parser
from core.rquest_dto.result import RquestResult


def main() -> None:
    args = parser.parse_args()
    logger = logging.getLogger(config.LOGGER_NAME)
    result = execute_query(parser)
    if not isinstance(result, RquestResult):
        raise TypeError("Payload does not match RQuest result schema")
    destination = environ.get("RESOLVE_DAEMON_POST_URL")
    url = f"{destination}/{result.uuid}/{result.collection_id}"

    data = json.dumps(result)
    for i in range(4):
        response = requests.post(url, data=data)
        if response.status_code == 200:
            break
        else:
            logger.warning(f"Resolve failed to post to {url} at {time.time()}")
            time.sleep(5)
