using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// resolver
AWSCredential.PoolId = builder.Configuration.GetSection("AWSCognito")["UserPoolId"];
AWSCredential.ClientId = builder.Configuration.GetSection("AWSCognito")["UserPoolClientId"];
AWSCredential.ClientSecret = builder.Configuration.GetSection("AWSCognito")["UserPoolClientSecret"];
AWSCredential.AccessKey = builder.Configuration.GetSection("AWSCognito")["AccessKey"];
AWSCredential.SecretKey = builder.Configuration.GetSection("AWSCognito")["SecretKey"];

// aws
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

// unit of work
builder.Services.AddScoped<IAuthenticationUnitOfWork, AuthenticationUnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

