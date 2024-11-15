using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Services;

public class ObfuscationService(ApplicationDbContext db): IObfuscationService
{
  public async Task<ObfuscationModel> LowNumberSuppression(string id)
  {
    var entity = await db.RelayTasks
                   .AsNoTracking() 
                   .SingleOrDefaultAsync(t => t.Id == id) 
                 ?? throw new KeyNotFoundException();
  
    
    return new ObfuscationModel();
  }

  public int LowNumberSuppression(int value, int threshold = 10)
  {
    return value > threshold ? value : 0;
  }
  
  /// <summary>
  /// Create a new RelayTask
  /// </summary>
  /// <param name="model">Model to Create.</param>
  /// <returns>The newly created RelayTask.</returns>
  public async Task<ObfuscationModel> Rounding(int s)
  {
    var entity = await db.RelayTasks
                   .AsNoTracking() 
                   .SingleOrDefaultAsync() 
                 ?? throw new KeyNotFoundException();

    
    return new ObfuscationModel();
  }

  public int Round(int value, int nearest = 10)
  {
    return nearest * (int)Math.Round((float)value / nearest);
  }
  
}
