from os import environ
from dotenv import load_dotenv

load_dotenv()

DEFAULT_DB_DRIVER = "postgresql"

# Logging configuration
LOGGER_NAME = "hutch"
BACKUP_LOGGER_NAME = "backup"
MSG_FORMAT = "%(levelname)s - %(asctime)s - %(message)s"
DATE_FORMAT = "%d-%b-%y %H:%M:%S"

TASK_API_BASE_URL = environ.get("TASK_API_BASE_URL")
TASK_API_USERNAME = environ.get("TASK_API_USERNAME")
TASK_API_PASSWORD = environ.get("TASK_API_PASSWORD")
LOW_NUMBER_SUPPRESSION_THRESHOLD = environ.get("LOW_NUMBER_SUPPRESSION_THRESHOLD")
ROUNDING_TARGET = environ.get("ROUNDING_TARGET")
