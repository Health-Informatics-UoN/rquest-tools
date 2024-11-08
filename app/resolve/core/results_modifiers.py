def results_modifiers(
    low_number_suppression_threshold: int,
    rounding_target: int,
) -> list:
    results_modifiers = []
    if low_number_suppression_threshold:
        results_modifiers.append(
            {
                "id": "Low Number Suppression",
                "threshold": low_number_suppression_threshold,
            }
        )
    if rounding_target:
        results_modifiers.append(
            {
                "id": "Rounding",
                "nearest": rounding_target,
            }
        )
    return results_modifiers
