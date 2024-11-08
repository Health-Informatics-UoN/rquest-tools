import logging
import json

from core.obfuscation import get_results_modifiers_from_str
import core.settings as settings
from core.execute_query import execute_query
from core.rquest_dto.result import RquestResult
from core.parser import parser


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
    logger = logging.getLogger(settings.LOGGER_NAME)
    try:
        with open(destination, "w") as output_file:
            file_body = json.dumps(result.to_dict())
            output_file.write(file_body)
    except Exception as e:
        logger.error(str(e), exc_info=True)


def main() -> None:
    args = parser.parse_args()

    with open(args.body) as body:
        query_dict = json.load(body)
    results_modifier = get_results_modifiers_from_str(args.results_modifiers)

    result = execute_query(query_dict, results_modifier)
    save_to_output(result, args.output)
    logger = logging.getLogger(settings.LOGGER_NAME)
    logger.info(f"Saved results to {args.output}")
