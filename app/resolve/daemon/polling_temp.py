from settings import *

import polling
import requests

if RELAY_BASE_URL is not None:
    polling.poll(
        lambda: requests.get(RELAY_BASE_URL).status_code == 200,
        step=POLLING_INTERVAL,
        timeout=POLLING_TIMEOUT
        )
else:
    raise Exception("RELAY_BASE_URL has not been set")
