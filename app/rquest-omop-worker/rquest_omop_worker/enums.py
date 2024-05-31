from enum import Enum


class DistributionQueryType(str, Enum):
    """Enum representing the types of distribution query types."""

    DEMOGRAPHICS = "DEMOGRAPHICS"
    GENERIC = "GENERIC"
    ICD_MAIN = "ICD-MAIN"

    @classmethod
    def get_value(cls, value: str):
        """Get the enum value of the distribution query type.

        If no corressponding enum value exists, `None` will be returned.

        Args:
            value (str): The value to get the enum value for.

        Returns:
            Union[DistributionQueryType, None]: Return the enum value corresponding to `value` or `None`.
        """
        return cls._value2member_map_.get(value)
