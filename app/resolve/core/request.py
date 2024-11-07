from requests.models import Response
from enum import Enum
import requests
from requests.auth import HTTPBasicAuth
from core.settings import USERNAME, PASSWORD


class SupportedMethod(Enum):
    POST = "post"
    GET = "get"
    PUT = "put"
    PATCH = "patch"
    DELETE = "delete"


def request(method: SupportedMethod, url: str, data: dict = None, **kwargs) -> Response:
    """
    Sends an HTTP request using the specified method to the given URL with optional data and additional parameters.

    Args:
        method (SupportedMethod): The HTTP method to use for the request. Must be one of the SupportedMethod enum values.
        url (str): The URL to which the request is sent.
        data (dict, optional): The data to send in the body of the request. Defaults to None.
        **kwargs: Additional keyword arguments to pass to the requests method. This can include parameters such as headers, params, verify, etc.

    Returns:
        Response: The response object returned by the requests library.

    """
    basicAuth = HTTPBasicAuth(USERNAME, PASSWORD)
    if method.name == "POST":
        response = requests.post(url=url, data=data, auth=basicAuth, **kwargs)
        return response
    elif method.name == "GET":
        response = requests.get(url=url, auth=basicAuth, **kwargs)
        return response
