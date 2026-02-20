using CoursesApplication.Domain.DomainModels;
using CoursesApplication.Domain.Identity;
using CoursesApplication.Repository.Data;
using CoursesApplication.Repository.Implementation;
using CoursesApplication.Repository.Interface;
using CoursesApplication.Service.Implementation;
using CoursesApplication.Service.Interface;
using CoursesApplication.Web.External;
using CoursesApplication.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------- DB provider selection -----------------
var env = builder.Environment;
var provider = builder.Configuration["DbProvider"] ?? "Sqlite"; // "Sqlite" | "Postgres" | "SqlServer" | "InMemory"

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) || env.IsEnvironment("Test"))
    {
        options.UseInMemoryDatabase("InMemoryDatabase");
    }
    else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
        // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
    }
    else
    {
        throw new InvalidOperationException($"Unsupported DbProvider '{provider}'. Use Sqlite, Postgres, SqlServer, or InMemory.");
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ----------------- Identity with Roles -----------------
builder.Services
    .AddDefaultIdentity<User>(o =>
    {
        o.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
});

builder.Services.AddScoped<PasswordHasher<User>>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClassroomRepository, ClassroomRepository>();
builder.Services.AddScoped<ITakenSeatRepository, TakenSeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookBorrowingService, BookBorrowingService>();
builder.Services.AddScoped<IBookCopyService, BookCopyService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowingBookLogService, BorrowingBookLogService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ITakenSeatLogService, TakenSeatLogService>();
builder.Services.AddScoped<ITakenSeatService, TakenSeatService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();

// HttpClient(s)
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("BookApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5177/");
    c.Timeout = TimeSpan.FromSeconds(45);
});

// External import client/services
builder.Services.AddScoped<IBookApiClient, BookApiClient>(); 
builder.Services.AddScoped<IBookImportService, BookImportService>();

var app = builder.Build();

// ----------------- Dev/Test setup -----------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var manager = services.GetRequiredService<UserManager<User>>();
    var passwordHasher = services.GetRequiredService<PasswordHasher<User>>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

// ----------------- Pipeline (order matters) -----------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // <<< BEFORE Authorization
app.UseAuthorization();

// ----------------- Seed roles/users (single place) -----------------
// ----------------- Seed roles/users (single place) -----------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Run migrations so Identity tables exist (skip this for InMemory/Test)
    if (!app.Environment.IsEnvironment("Test") && !provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        await db.Database.MigrateAsync();
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await EnsureSeedAsync(userManager, roleManager);
}


// ----------------- Endpoints -----------------
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();




// ----------------- Seeder -----------------
static async Task EnsureSeedAsync(UserManager<User> users, RoleManager<Role> roles)
{
    // 1) Ensure roles exist
    var roleNames = new[] { "Librarian", "Keeper", "Student" };
    foreach (var r in roleNames)
    {
        if (!await roles.RoleExistsAsync(r))
            await roles.CreateAsync(new Role { Name = r });
    }

    // 2) Ensure users exist and are in the right roles
    await SeedUserAsync(users,
        email: "marko.markovski@gmail.com",
        password: "Lib123!@#",
        name: "Marko",
        surname: "Markovski",
        rolesToAdd: new[] { "Librarian" });

    await SeedUserAsync(users,
        email: "andreaa_janevska@outlook.com",
        password: "Keep123!@#",
        name: "Andrea",
        surname: "Janevska",
        rolesToAdd: new[] { "Keeper" });
}

static async Task SeedUserAsync(
    UserManager<User> users,
    string email,
    string password,
    string name,
    string surname,
    string[] rolesToAdd)
{
    // Since you set UserName = email, search by email (or FindByNameAsync(email))
    var user = await users.FindByEmailAsync(email);
    if (user == null)
    {
        user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Name = name,
            Surname = surname
        };

        var create = await users.CreateAsync(user, password);
        if (!create.Succeeded)
            throw new Exception("Failed to create user: " +
                                string.Join("; ", create.Errors.Select(e => e.Description)));
    }

    // Add roles if missing
    foreach (var role in rolesToAdd)
        if (!await users.IsInRoleAsync(user, role))
            await users.AddToRoleAsync(user, role);
}



public partial class Program { }
