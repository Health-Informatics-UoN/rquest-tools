import core.settings as settings
import polling

from core.task_api_client import TaskApiClient


class Poll:

    def __init__(self, client: TaskApiClient) -> None:
        self.endpoint = "/task/nextjob/{}".format(settings.COLLECTION_ID)
        self.client = client

    def poll(self):
        if settings.TASK_API_BASE_URL is not None:
            data = polling.poll(
                lambda: self.client.get(self.endpoint),
                step=settings.POLLING_INTERVAL,
                poll_forever=True
                )
        else:
            raise Exception("TASK_API_BASE_URL has not been set")

        return data
