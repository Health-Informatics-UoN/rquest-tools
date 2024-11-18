using Hutch.Relay.Services.Contracts;

namespace Hutch.Relay.Services;

public class ObfuscationService(): IObfuscationService
{
  /// <summary>
  /// Applies low number suppression to the value. If value is greater than the threshold, it will remain the same. Otherwise, it will be set to 0.
  /// </summary>
  /// <param name="value">The value to apply suppression to.</param>
  /// <param name="threshold">The threshold value of suppression.</param>
  /// <returns>The suppressed value.</returns>
  public int LowNumberSuppression(int value, int threshold)
  {
    return value > threshold ? value : 0;
  }

  /// <summary>
  /// Applies rounding to the value. The value will be rounded to nearest. For example, if nearest = 10, 95 would be rounded to 100, as would 104 and any integer in between.
  /// </summary>
  /// <param name="value">The value to be rounded.</param>
  /// <param name="nearest">The number to round to.</param>
  /// <returns>The rounded value.</returns>
  public int Rounding(int value, int nearest)
  {
    return nearest * (int)Math.Round((float)value / nearest);
  }
  
}
