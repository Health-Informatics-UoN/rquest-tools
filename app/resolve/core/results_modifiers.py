def result_modifiers(
    low_number_suppession_threshold: int,
    rounding_taget: int,
) -> list:
    result_modifiers = []
    if low_number_suppession_threshold:
        result_modifiers.append(
            {
                "id": "Low Number Suppression",
                "threshold": low_number_suppession_threshold,
            }
        )
    if rounding_taget:
        result_modifiers.append(
            {
                "id": "Rounding",
                "nearest": rounding_taget,
            }
        )
    return result_modifiers
