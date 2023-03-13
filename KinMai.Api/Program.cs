using Dapper;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Resolver;
using KinMai.Dapper.Implement;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Implement;
using KinMai.S3.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;

var allowOrigin = "_allowOrigin";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowOrigin, policy =>
    {
        policy.WithOrigins("http://localhost:4200","https://kinmai.net").AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AWS Lambda support
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// resolver
AWSCredential.PoolId = builder.Configuration.GetSection("AWSCognito")["UserPoolId"];
AWSCredential.ClientId = builder.Configuration.GetSection("AWSCognito")["UserPoolClientId"];
AWSCredential.ClientSecret = builder.Configuration.GetSection("AWSCognito")["UserPoolClientSecret"];
AWSCredential.AccessKey = builder.Configuration.GetSection("AWSCognito")["AccessKey"];
AWSCredential.SecretKey = builder.Configuration.GetSection("AWSCognito")["SecretKey"];
ConnectionResolver.KinMaiConnection = builder.Configuration.GetSection("ConnectionStrings")["KinMaiConnection"];
ConnectionResolver.KinMaiFrontendUrl = builder.Configuration.GetSection("ConnectionStrings")["KinMaiFrontendUrl"];

// aws
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

// data access service
builder.Services.AddDbContext<KinMaiContext>(options =>
{
    options.UseNpgsql(ConnectionResolver.KinMaiConnection)
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }));
});

// unit of work
builder.Services.AddScoped<IAuthenticationUnitOfWork, AuthenticationUnitOfWork>();
builder.Services.AddScoped<IEntityUnitOfWork, EntityUnitOfWork>();
builder.Services.AddScoped<IDapperUnitOfWork>(_ => new DapperUnitOfWork(ConnectionResolver.KinMaiConnection));
builder.Services.AddScoped<ILogicUnitOfWork, LogicUnitOfWork>();
builder.Services.AddScoped<IS3UnitOfWork, S3UnitOfWork>();
builder.Services.AddScoped<IMailUnitOfWork, MailUnitOfWork>();

SqlMapper.AddTypeHandler(new StringListTypeHandler<List<string>>());
SqlMapper.AddTypeHandler(new IntListTypeHandler<List<int>>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(allowOrigin);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

