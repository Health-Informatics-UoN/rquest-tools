import time
import logging
import core.settings as settings
from core.execute_query import execute_query
from core.rquest_dto.result import RquestResult
from core.task_api_client import TaskApiClient
from core.results_modifiers import results_modifiers
import asyncio
import tracemalloc

tracemalloc.start()


async def main() -> None:
    client = TaskApiClient()
    logger = logging.getLogger(settings.LOGGER_NAME)
    # Task Api Client class init.

    # Building results modifiers
    modifiers_list = results_modifiers(
        low_number_suppression_threshold=int(
            settings.LOW_NUMBER_SUPPRESSION_THRESHOLD or 0
        ),
        rounding_target=int(settings.ROUNDING_TARGET or 0),
    )

    while True:
        # Polling to get query from Relay
        polling_endpoint = f"task/nextjob/{settings.COLLECTION_ID}"
        print("Getting query...")
        query = client.get(endpoint=polling_endpoint)
        print("query: ", query)
        if query.status_code == 200:
            query_dict: dict = query.json()
            print(f"Found one job. Query is {query.text}")
            result = execute_query(query_dict, results_modifiers=modifiers_list)
            print("Querying when having the query from polling")
            if not isinstance(result, RquestResult):
                raise TypeError("Payload does not match RQuest result schema")

            # Build return endpoint after having result
            return_endpoint = f"task/result/{result.uuid}/{result.collection_id}"
            print("returning results...")
            # Try to send the results back to Relay
            for _ in range(4):
                response = client.post(endpoint=return_endpoint, data=result.to_dict())

                # Resolve will stop retrying to post results when response was successful or there is a client error
                if (
                    200 <= response.status_code < 300
                    or 400 <= response.status_code < 500
                ):
                    print("result returned successfully")
                    break
                else:
                    logger.warning(
                        f"Resolve failed to post to {return_endpoint} at {time.time()}"
                    )
                    await asyncio.sleep(5)

        elif query.status_code == 204:
            print("There is no job available at the moment. Trying again...")

        await asyncio.sleep(5)


asyncio.run(main())
