import core.settings as settings
import polling
import requests

if settings.RELAY_BASE_URL is not None:
    polling.poll(
        lambda: requests.get(settings.RELAY_BASE_URL).status_code == 200,
        step=settings.POLLING_INTERVAL,
        timeout=settings.POLLING_TIMEOUT
        )
else:
    raise Exception("RELAY_BASE_URL has not been set")
