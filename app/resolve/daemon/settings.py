from os import environ

### setting environment variables for testing, to ensure they are stored and read correctly
#environ["relayBaseURL"] = ""
environ["polling_interval"] = "3"


### currently no guards to ensure that POLLING_INTERVAL and POLLING_TIMEOUT are >=0
RELAY_BASE_URL = environ.get("relay_base_URL") #returns None if not set
POLLING_INTERVAL = int(environ.get("polling_interval", "5"))
POLLING_TIMEOUT = int(environ.get("polling_timeout", "5")) #set a timeout if the host is unresponsive - which may occur if it's been set incorrectly


