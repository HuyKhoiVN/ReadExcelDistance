using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;
using ReadExcelProcess.Model;
using ReadExcelProcess.Repositories;
using ReadExcelProcess.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.AddDebug();

builder.Services.AddDbContext<SysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();
builder.Services.AddTransient<IRepairPersonRepository, RepairPersonRepository>();
builder.Services.AddTransient<IExcelService, ExcelService>();
builder.Services.AddTransient<IDistanceMatrixService, DistanceMatrixService>();
builder.Services.AddTransient<IDeviceImportService, DeviceImportService>();
builder.Services.AddTransient<IOfficerImportService, OfficerImportService>();
builder.Services.AddTransient<IProvinceService, ProvinceService>();
builder.Services.AddTransient<IContractImportService, ContractImportService>();
builder.Services.AddTransient<IDeviceMaintenanceService, DeviceMaintenanceService>();
builder.Services.AddHttpClient<GeoCodingService>();
builder.Services.AddTransient<IGeoCodingService>(ge =>
{
    var httpClientFactory = ge.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(GeoCodingService));
    return new GeoCodingService(httpClient);
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=index}");

    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/Home/index");
        return Task.CompletedTask;
    });
});


app.Run();
