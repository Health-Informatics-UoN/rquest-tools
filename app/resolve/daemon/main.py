import core.settings as settings
from core.settings import RETRIES
from core.task_api_client import TaskApiClient
from core.results_modifiers import results_modifiers
from core.logger import logger
from core.setting_database import setting_database
import polling
from functools import partial
import query_functions

def main() -> None:

    # Setting database connection
    db_manager = setting_database(logger=logger)
    # Task Api Client class init.
    client = TaskApiClient()

    # Building results modifiers
    modifiers_list = results_modifiers(
        low_number_suppression_threshold=int(
            settings.LOW_NUMBER_SUPPRESSION_THRESHOLD or 0
        ),
        rounding_target=int(settings.ROUNDING_TARGET or 0),
    )

    polling_endpoint = f"task/nextjob/{settings.COLLECTION_ID}"


    ### finished setup, ready to do some actual work
    poll = polling.Polling(lambda: client.get(endpoint=polling_endpoint), query_functions.http_success, 3)
    response = poll.poll()

    ### should only hit here with response == None if it failed due to timeout or steps.
    if response is not None:
        ### success! populate the data object with the query response
        data = query_functions.query(db_manager, response, modifiers_list)
        # return the data
        return_poll = polling.Polling(lambda: client.post(endpoint=data.return_endpoint, data=data.payload), partial(query_functions.return_poll_success, data = data), 5, repeats = RETRIES)
        return_poll.poll()



if __name__ == '__main__':
    #main()
    def square(x):
        return x*x


    def success(x, data = None):

        return x==data

    poll = polling.Polling(lambda: square(2), partial(success, data=4), 3)
    response = poll.poll()

    pass
