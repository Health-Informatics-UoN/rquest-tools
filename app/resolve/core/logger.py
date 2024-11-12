import logging
import core.settings as settings
import sys

logger = logging.getLogger(settings.LOGGER_NAME)
LOG_FORMAT = logging.Formatter(
    settings.MSG_FORMAT,
    datefmt=settings.DATE_FORMAT,
)
console_handler = logging.StreamHandler(sys.stdout)
console_handler.setFormatter(LOG_FORMAT)
logger = logging.getLogger(settings.LOGGER_NAME)
logger.setLevel(logging.INFO)
logger.addHandler(console_handler)
