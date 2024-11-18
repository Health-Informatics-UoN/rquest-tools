using Hutch.Relay.Services.Contracts;

namespace Hutch.Relay.Services;

public class ObfuscationService(): IObfuscationService
{
  
  public int LowNumberSuppression(int value, int threshold)
  {
    return value > threshold ? value : 0;
  }

  public int Rounding(int value, int nearest)
  {
    return nearest * (int)Math.Round((float)value / nearest);
  }
  
}
