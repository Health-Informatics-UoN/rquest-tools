namespace Hutch.Relay.Services.Contracts;

public interface IObfuscationService
{

  /// <summary>
  /// Suppresses numbers that are below a specified threshold.
  /// </summary>
  /// <param name="value">The value to be checked.</param>
  /// <param name="threshold">The threshold below which the value will be suppressed.</param>
  /// <returns>The original value if it is above the threshold, otherwise 0.</returns>
  int LowNumberSuppression(int value, int threshold);

  /// <summary>
  /// Rounds a value to the nearest specified number.
  /// </summary>
  /// <param name="value">The value to be rounded.</param>
  /// <param name="nearest">The number to which the value will be rounded.</param>
  /// <returns>The value rounded to the nearest specified number.</returns>
  int Rounding(int value, int nearest);
}
