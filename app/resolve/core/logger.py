import logging
import core.settings as settings
import sys

loggers = {}


def logger_func(name):

    if loggers.get(name):
        return loggers.get(name)
    else:
        # Set up the logger
        LOG_FORMAT = logging.Formatter(
            settings.MSG_FORMAT,
            datefmt=settings.DATE_FORMAT,
        )
        logger = logging.getLogger(name)
        logger.setLevel(logging.INFO)
        handler = logging.StreamHandler(sys.stdout)
        handler.setFormatter(LOG_FORMAT)
        logger.addHandler(handler)
        loggers[name] = logger

        return logger
