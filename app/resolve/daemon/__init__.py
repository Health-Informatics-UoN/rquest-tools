from settings import *
import requests
import polling

def main() -> None:
    print("hello rabbit world!")
    print ('testing')

    retval = polling.poll(
        lambda: requests.get(relay_base_url).status_code == 200,
        step=polling_interval,
        poll_forever=True)

    print (retval)

    pass
pass

if __name__ == '__main__':
    main()
