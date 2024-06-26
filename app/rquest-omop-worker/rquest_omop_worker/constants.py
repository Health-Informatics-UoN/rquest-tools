from rquest_omop_worker.enums import DistributionQueryType


DISTRIBUTION_TYPE_FILE_NAMES_MAP = {
    DistributionQueryType.DEMOGRAPHICS: "demographics.distribution",
    DistributionQueryType.GENERIC: "code.distribution"
}
