using HalloDoc.DataAccessLayer.DataContext;using HalloDoc.Hubs;using Microsoft.EntityFrameworkCore;using Npgsql;using Repository;using Repository.IRepository;var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();builder.Services.AddSession();builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));builder.Services.AddSignalR(e =>{    e.MaximumReceiveMessageSize = 102400000;});

// Register NpgsqlConnection as a transient service
builder.Services.AddTransient<NpgsqlConnection>(sp =>{    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");    var connection = new NpgsqlConnection(connectionString);    connection.Open();    return connection;});builder.Services.AddScoped<IPatientRepository, PatientRepository>();builder.Services.AddScoped<IAdminRepository, AdminRepository>();builder.Services.AddScoped<IAuthenticateRepository, AuthenticateRepository>();builder.Services.AddScoped<IProviderRepository, ProviderRepository>();builder.Services.AddScoped<IChatRepository, ChatRepository>();var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()){    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();}app.UseHttpsRedirection();app.UseStaticFiles();app.UseSession();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute(    name: "default",
    //pattern: "{controller=Home}/{action=patientSite}/{id?}");
    pattern: "{controller=Admin}/{action=Index}/{id?}");

    app.MapHub<ChatHub>("/chatHub"); // Add SignalR hub endpoint if you have one

app.Run();