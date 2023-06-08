var builder = WebApplication.CreateBuilder(args);

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddDbContext<EmployeeDbContext>(opt => opt.UseInMemoryDatabase("EmployeeList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(o =>
{
  o.TokenValidationParameters = new TokenValidationParameters
  {
    ValidIssuer = issuer,
    ValidAudience = audience,
    IssuerSigningKey = securityKey,
    ValidateIssuerSigningKey = true
  };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfig.ConfigureSwaggerGen);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseStaticFiles();
app.UseSwaggerUI(SwaggerConfig.ConfigureSwaggerUI);


app.MapGet("/", [AllowAnonymous] () => "Hello World!");


app.MapPost("/security/getToken", [AllowAnonymous] (RequestAccess user) =>
{
    if (user.UserName=="admin" && user.Password=="password")
    {
        var claims = new List<Claim>()
        {
            new Claim("Id", "1"),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, "Administrator"),
            new(ClaimTypes.Role, "User"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims, 
            expires: DateTime.UtcNow.AddHours(1), 
            signingCredentials: credentials
        );
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return Results.Ok(new { AccessToken = accessToken });
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.MapGet("/employees", [Authorize] async (EmployeeDbContext dbContext) =>
{
  var employees = await dbContext.Employees.ToListAsync();
  return employees;
});

app.MapGet("/employees/{id}", [Authorize] async (int id, EmployeeDbContext dbContext) =>
{
  var employee = await dbContext.Employees.Where(t => t.Id == id).FirstOrDefaultAsync();
  return employee;
});

app.MapPost("/employees", [Authorize] async (Employee employee, EmployeeDbContext dbContext) =>
{
  dbContext.Employees.Add(employee);
  await dbContext.SaveChangesAsync();
  return employee;
});

app.MapPut("/employees", [Authorize] async (Employee employee, EmployeeDbContext dbContext) =>
{
  dbContext.Entry(employee).State = EntityState.Modified;
  await dbContext.SaveChangesAsync();
  return employee;
});

app.MapGet("/api/role-check", [Authorize] (ClaimsPrincipal user) =>
{
    if (user.IsInRole("Administrator"))
    {
        return Results.Ok(new{ UserRole = "Administrator"});
    }
    if (user.IsInRole("User"))
    {
        return Results.Ok(new{ UserRole = "User"});
    }
    return Results.NoContent();
});

await app.RunAsync();
record RequestAccess (string UserName, string Password);

public partial class Program
{
  // Expose the Program class for use with WebApplicationFactory<T>
}