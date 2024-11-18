import argparse

parser = argparse.ArgumentParser(
    prog="bunny-cli",
    description="This program takes a JSON string containing an RQuest query and solves it.",
)
parser.add_argument(
    "--body",
    dest="body",
    required=True,
    help="The JSON file containing the query",
)
parser.add_argument(
    "-o",
    "--output",
    dest="output",
    required=False,
    type=str,
    default="output.json",
    help="The path to the output file",
)
parser.add_argument(
    "-m",
    "--modifiers",
    dest="results_modifiers",
    required=False,
    type=str,
    default="[]",  # when parsed will produce an empty list
    help="The results modifiers",
)
