using RquestBridge.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Configure Services

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Options
builder.Services
  .Configure<RQuestOptions>(builder.Configuration.GetSection("RQuest"))
  .Configure<RQuestTaskApiOptions>(builder.Configuration.GetSection("Credentials"))
  .Configure<WorkflowOptions>(builder.Configuration.GetSection("Workflow"))
  .Configure<CrateAgentOptions>(builder.Configuration.GetSection("Crate:Agent"))
  .Configure<CrateProjectOptions>(builder.Configuration.GetSection("Crate:Project"))
  .Configure<CrateOrganizationOptions>(builder.Configuration.GetSection("Crate:Organisation"))
  .Configure<BridgeOptions>(builder.Configuration.GetSection("Bridge"));

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
