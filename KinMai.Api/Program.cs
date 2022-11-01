using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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
AWSCredential.Region = builder.Configuration.GetSection("AWSCognito")["Region"];
AWSCredential.AccessKey = builder.Configuration.GetSection("AWSCognito")["AccessKey"];
AWSCredential.SecretKey = builder.Configuration.GetSection("AWSCognito")["SecretKey"];

// aws
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();

// unit of work
builder.Services.AddScoped<IAuthenticationUnitOfWork, AuthenticationUnitOfWork>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// authentication
builder.Services.AddCognitoIdentity();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["AWSCognito:Authority"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false
    };
});

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

