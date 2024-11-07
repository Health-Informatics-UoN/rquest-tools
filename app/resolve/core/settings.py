from dotenv import load_dotenv
import os

load_dotenv()


TASK_API_BASE_URL = os.getenv("TASK_API_BASE_URL")
