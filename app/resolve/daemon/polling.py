import datetime
import time

class Polling:

    def __init__(self, poll_function, success_criterion, delay = 1, success_function = None, timeout = None, repeats = None):
        self._stop = False

        self.poll_function = poll_function
        self.success_criterion = success_criterion
        self.success_function = success_function

        self.timeout = timeout
        self.repeats = repeats
        self.delay = delay


        pass

    def poll(self):
        start_time = datetime.datetime.now()
        times_repeated = 0
        while not self._stop:
            result = self.poll_function() ### need to ensure args get passed

            if self.success_criterion(result) and self.success_function is not None:
                return self.success_function(result) ###need ot ensure args get passed
            if self.success_criterion(result):
                self._stop = True
                return result
            else:
                ### failure - do we retry, or abort?
                if self.timeout is not None and (datetime.datetime.now() - start_time).total_seconds() > self.timeout:
                    ### fail due to timeout
                    return None
                if self.repeats is not None and times_repeated == self.repeats:
                    ### fail due to repeating
                    return None
                times_repeated += 1
                time.sleep(self.delay)
                pass

            pass

    def stop(self):
        self._stop = True
