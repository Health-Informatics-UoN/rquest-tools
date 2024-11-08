from select import select

from settings import *
import json




class Process_payload:
    def __init__(self, payload: str) -> None:
        self.payload = json.loads(payload)
        self.jobtype = self.set_job_type()

    def append_jobid(self):
        json_payload['uuid']+='-'+COLLECTION_ID

    def set_job_type(self):
        if "analysis" not in self.payload:
            self.jobtype = 'A'
        elif self.payload['analysis'] == 'DISTRIBUTION':
            self.jobtype = 'B'
        else:
            self.jobtype = 'C'

        pass

if __name__ == "__main__":

    payloadA = """
    {
        "cohort": {
            "groups_oper": "OR",
            "groups": [
                {
                    "rules_oper": "AND",
                    "rules": [
                        {
                            "type": "TEXT",
                            "varname": "OMOP",
                            "oper": "=",
                            "value": "260139",
                            "time": "",
                            "ext": "",
                            "unit": "",
                            "regex": ""
                        }
                    ]
                }
            ]
        },
        "protocol_version": "v2",
        "char_salt": "52ee5332-d209-4cf6-86d2-7f7569292c23",
        "uuid": "774453ea-6537-469c-b8bf-23d16137cc97",
        "collection": "RQ-CC-d8e8c4aa-a8af-4229-b479-618788c44122",
        "owner": "user1"
    }
    """
    payloadB = """
    {
        "code": "ICD-MAIN",
        "analysis": "DISTRIBUTION",
        "uuid": "8200b3a0-cf61-455a-b384-60a3e4770b92",
        "collection": "RQ-CC-d8e8c4aa-a8af-4229-b479-618788c44122",
        "owner": "user1"
    }
    """





    payloadC = """
        {
        "task_id": "job-2022-10-24-10:15:25-RQ-dd0bf7f6-38a8-4337-95b9-5f24bfaca753",
        "project": "RQ-dd0bf7f6-38a8-4337-95b9-5f24bfaca753",
        "owner": "user1",
        "cohort_x": {
            "groups": [
                {
                    "rules": [
                        {
                            "varname": "OMOP",
                            "type": "TEXT",
                            "oper": "=",
                            "value": "8507"
                        }
                    ],
                    "rules_oper": "AND"
                }
            ],
            "groups_oper": "OR"
        },
        "cohort_y": {
            "groups": [
                {
                    "rules": [
                        {
                            "varname": "OMOP",
                            "type": "TEXT",
                            "oper": "=",
                            "value": "8507"
                        }
                    ],
                    "rules_oper": "AND"
                }
            ],
            "groups_oper": "OR"
        },
        "gene": "rs7412,rs429358",
        "collection": "RQ-CC-d3d61136-05bb-4388-8aa6-31c54e0c5735",
        "plink_maf": "0.01",
        "plink_maxmaf": "0.4999",
        "plink_hwe": "1.0E-5",
        "plink_geno": "0.05",
        "analysis": "GWAS",
        "protocol_version": "v2",
        "uuid": "5a5fff4c-56bc-4f69-90ac-1c3762a1f776"
    }
    """


    ### use loads (with an s) to load it from a string
    json_payload = json.loads(payloadB)

    loadA = Process_payload(payloadA)
    loadB = Process_payload(payloadB)
    loadC = Process_payload(payloadC)

    loadA.set_job_type()
    print (loadA.jobtype)
    loadB.set_job_type()
    print(loadB.jobtype)
    loadC.set_job_type()
    print(loadC.jobtype)
    pass
