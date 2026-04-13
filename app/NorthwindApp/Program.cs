using NorthwindApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Northwind API", Version = "v1" });
});

// Register data service
builder.Services.AddScoped<INorthwindDataService, NorthwindDataService>();

// Register chat service
builder.Services.AddScoped<IChatService, ChatService>();

// HTTP client for API calls from Razor Pages
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
builder.Services.AddHttpClient("NorthwindApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind API v1");
    c.RoutePrefix = "swagger";
});

app.MapRazorPages();
app.MapControllers();

// Redirect root to /Index
app.MapGet("/", () => Results.Redirect("/Index"));

app.Run();
