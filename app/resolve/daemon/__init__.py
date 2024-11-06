import requests
from os import environ
import json

from core.execute_query import execute_query
from core.parser import parser
from core.rquest_dto.result import RquestResult


def main() -> None:
    result = execute_query(parser)
    if not isinstance(result, RquestResult):
        raise TypeError("Payload does not match RQuest result schema")
    destination = environ.get("RESOLVE_DAEMON_POST_URL")
    url = f"http://{destination}/{result.uuid}/{result.collection_id}"

    data = json.dumps(result)
    requests.post(url, data=data)
