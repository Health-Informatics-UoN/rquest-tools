import time
import core.settings as settings
from core.execute_query import execute_query
from core.rquest_dto.result import RquestResult
from core.settings import RETRIES
from core.task_api_client import TaskApiClient
from core.results_modifiers import results_modifiers
from core.logger import logger
from core.setting_database import setting_database
import payload
import polling
#import polling2

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

    ########################################################################################
    ### should it just be without settings.TASK_API_TYPE and get the type from the object?
    # polling_endpoint = (
    #     f"task/nextjob/{settings.COLLECTION_ID}.{settings.TASK_API_TYPE}"
    #     if settings.TASK_API_TYPE
    #     else f"task/nextjob/{settings.COLLECTION_ID}"
    # )
    #
    polling_endpoint = f"task/nextjob/{settings.COLLECTION_ID}"
    #########################################################################################


    ### polling requires function to poll, success criteria (could be defined as return.x, for example),
    ### return value (not necessarily the same as success criteria, but may not need to be specified as it's the return of the function)
    ### and function to do after

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


#    response = polling2.poll(lambda: client.get(endpoint=polling_endpoint), check_success = http_success, step = settings.POLLING_INTERVAL, poll_forever = True)
    poll = polling.Polling(lambda: client.get(endpoint=polling_endpoint), http_success, 3)
    response = poll.poll()

    ### should only hit here with response == None if it failed due to timeout or steps.
    if response is not None:
        ### success! populate the data object with the query response
        data = query(db_manager, response, modifiers_list)
######################################### return the data
        return_poll = polling.Polling(lambda: client.post(endpoint=data.return_endpoint, data=data.payload), lambda: return_poll_success(data = data), 5, repeats = RETRIES)
        return_poll_response = return_poll.poll()



if __name__ == '__main__':
    #main()
    def square(x):
        return x*x


    def success(x):

        return x==4

    poll = polling.Polling(lambda: square(2), success, 3)
    response = poll.poll()

    pass
