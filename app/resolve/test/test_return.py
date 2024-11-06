import pytest
from core.query_solvers import AvailabilityQuery, DistributionQuery, solve_availability
from core.db_manager import SyncDBManager
from core.rquest_dto.result import RquestResult
from dotenv import load_dotenv
import os
import core.config as config

load_dotenv()


@pytest.fixture
def db_manager():
    datasource_db_port = os.getenv("DATASOURCE_DB_PORT")
    return SyncDBManager(
        username=os.getenv("DATASOURCE_DB_USERNAME"),
        password=os.getenv("DATASOURCE_DB_PASSWORD"),
        host=os.getenv("DATASOURCE_DB_HOST"),
        port=(int(datasource_db_port) if datasource_db_port is not None else None),
        database=os.getenv("DATASOURCE_DB_DATABASE"),
        drivername=os.getenv("DATASOURCE_DB_DRIVERNAME", config.DEFAULT_DB_DRIVER),
        schema=os.getenv("DATASOURCE_DB_SCHEMA"),
    )


@pytest.fixture
def availability_query():
    return AvailabilityQuery(
        cohort={
            "groups": [
                {
                    "rules": [
                        {
                            "varname": "OMOP",
                            "varcat": "Person",
                            "type": "TEXT",
                            "oper": "=",
                            "value": "8507",
                        }
                    ],
                    "rules_oper": "AND",
                }
            ],
            "groups_oper": "OR",
        },
        uuid="unique_id",
        protocol_version="v2",
        char_salt="salt",
        collection="collection_id",
        owner="user1",
    )


@pytest.fixture
def availability_result():
    return RquestResult(
        uuid="unique_id",
        status="ok",
        collection_id="collection_id",
        count=6272,
        datasets_count=0,
        files=[],
        message="",
        protocol_version="v2",
    )


def test_solver_avaiability(db_manager, availability_query, availability_result):
    result = solve_availability(db_manager=db_manager, query=availability_query)
    assert result == availability_result
