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

### setting environment variables for testing, to ensure they are stored and read correctly
#environ["relayBaseURL"] = ""
environ["polling_interval"] = "3"


### currently no guards to ensure that POLLING_INTERVAL and POLLING_TIMEOUT are >=0
RELAY_BASE_URL = environ.get("relay_base_URL") #returns None if not set
POLLING_INTERVAL = int(environ.get("polling_interval", "5"))
POLLING_TIMEOUT = int(environ.get("polling_timeout", "5")) #set a timeout if the host is unresponsive - which may occur if it's been set incorrectly
COLLECTION_ID = environ.get("collection_id")
