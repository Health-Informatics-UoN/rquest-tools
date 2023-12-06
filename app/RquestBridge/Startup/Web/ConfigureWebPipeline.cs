namespace RquestBridge.Startup.Web;

public static class ConfigureWebPipeline
{
  /// <summary>
  /// Configure the HTTP Request Pipeline for an ASP.NET Core app
  /// </summary>
  /// <param name="app"></param>
  /// <returns></returns>
  public static WebApplication UseWebPipeline(this WebApplication app)
  {
    if (!app.Environment.IsDevelopment())
    {
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI();

    // Routing before Auth
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // API Controllers
    app.MapControllers();

    return app;
  }
}
