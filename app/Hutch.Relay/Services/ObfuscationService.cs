using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.EntityFrameworkCore;

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
