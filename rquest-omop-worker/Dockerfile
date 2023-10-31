FROM python:3.10-buster
COPY . /app/rquest-omop-worker
WORKDIR /app/rquest-omop-worker
RUN pip install --no-cache-dir .
CMD [ "rquest-omop-worker" ]
