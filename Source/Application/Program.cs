using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WindowsCertificateManagerOptions>(builder.Configuration.GetSection("WindowsCertificateManager"));

builder.Services.AddRazorPages();
builder.Services.AddSingleton<ICertificateFactory, CertificateFactory>();
builder.Services.AddSingleton<CertificateRepository>();
builder.Services.AddSingleton<ICertificateLoader>(serviceProvider => serviceProvider.GetRequiredService<CertificateRepository>());
builder.Services.AddSingleton<ICertificateRepository>(serviceProvider => serviceProvider.GetRequiredService<CertificateRepository>());
builder.Services.AddSingleton<ICertificateStoreFactory, CertificateStoreFactory>();
builder.Services.AddSingleton<CertificateStoreRepository>();
builder.Services.AddSingleton<ICertificateStoreLoader>(serviceProvider => serviceProvider.GetRequiredService<CertificateStoreRepository>());
builder.Services.AddSingleton<ICertificateStoreRepository>(serviceProvider => serviceProvider.GetRequiredService<CertificateStoreRepository>());

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();