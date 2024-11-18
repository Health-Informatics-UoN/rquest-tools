FROM python:3.10-buster
COPY . /app/bunny
WORKDIR /app/bunny

RUN pip install poetry

RUN poetry install

ENTRYPOINT ["poetry", "run", "bunny-daemon"]
