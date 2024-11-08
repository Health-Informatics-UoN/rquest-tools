from settings import *

import polling
import requests

class Poll:

    def __init__(self):
        self.collection_id_path = "/task/nextjob/{}".format(COLLECTION_ID)

        pass

    def poll(self):
        if RELAY_BASE_URL is not None:
            data = polling.poll(
                lambda: requests.get(RELAY_BASE_URL + self.collection_id_path),
                step=POLLING_INTERVAL,
                poll_forever=True
                )
        else:
            raise Exception("RELAY_BASE_URL has not been set")
