from dotenv import load_dotenv
from os import environ

load_dotenv()


TASK_API_BASE_URL = environ.get("TASK_API_BASE_URL")
