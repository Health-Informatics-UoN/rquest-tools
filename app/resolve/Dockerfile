FROM python:3.10-buster
COPY . /app/resolve
WORKDIR /app/resolve
RUN pip install --no-cache-dir .
CMD [ "resolve-cli" ]
