import logging
import json

from core.obfuscation import get_results_modifiers_from_str
import core.settings as settings
from core.execute_query import execute_query
from core.rquest_dto.result import RquestResult
from core.parser import parser
from core.logger import logger_func
from core.setting_database import setting_database


logger = logger_func(settings.LOGGER_NAME)


def save_to_output(result: RquestResult, destination: str) -> None:
    """Save the result to a JSON file.

    Args:
        result (RquestResult): The object containing the result of a query.
        destination (str): The name of the JSON file to save the results.

    Raises:
        ValueError: A path to a non-JSON file was passed as the destination.
    """
    if not destination.endswith(".json"):
        raise ValueError("Please specify a JSON file (ending in '.json').")

    try:
        with open(destination, "w") as output_file:
            file_body = json.dumps(result.to_dict())
            output_file.write(file_body)
    except Exception as e:
        logger.error(str(e), exc_info=True)


def main() -> None:
    # Setting database connection
    db_manager = setting_database(logger=logger)
    # Resolve passed args.
    args = parser.parse_args()

    with open(args.body) as body:
        query_dict = json.load(body)
    results_modifier = get_results_modifiers_from_str(args.results_modifiers)

    result = execute_query(
        query_dict, results_modifier, logger=logger, db_manager=db_manager
    )
    save_to_output(result, args.output)
    logger.info(f"Saved results to {args.output}")
