from os import environ

DEFAULT_DB_DRIVER = "postgresql"

# Logging configuration
LOGGER_NAME = "hutch"
BACKUP_LOGGER_NAME = "backup"
MSG_FORMAT = "%(levelname)s - %(asctime)s - %(message)s"
DATE_FORMAT = "%d-%b-%y %H:%M:%S"

TASK_API_BASE_URL = environ.get("TASK_API_BASE_URL")
USERNAME = environ.get("USERNAME")
PASSWORD = environ.get("PASSWORD")
