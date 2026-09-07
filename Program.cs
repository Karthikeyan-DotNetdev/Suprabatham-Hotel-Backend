using AppSettings;
using CommonServices;
using laptop_service;
using laptop_service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Text;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

#region UserCode
// Define CorsPolicy
// ===============================================================================
/* builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
    builder =>
    {
         builder.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
        builder.WithOrigins("https://smartpos.brositecom.com").AllowAnyMethod().AllowAnyHeader();
      
        //builder.WithOrigins("https://ibot.brosonetech.com").AllowAnyMethod().AllowAnyHeader();
        builder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();   
    });
}); */

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
    policyBuilder =>
    {
        policyBuilder
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
// ===============================================================================
//Uncommand if for production environment
//builder.Services.AddHsts(options =>
//{
//    options.Preload = true;
//    options.IncludeSubDomains = true;
//    options.MaxAge = TimeSpan.FromDays(60);
//    options.ExcludedHosts.Add("botposapi.brosonetech.com");
//});

// Uncommand if for production environment
//builder.Services.AddHttpsRedirection(options =>
//{
//    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
//    options.HttpsPort = 443;
//});
#endregion UserCode

// Add services to the container.
builder.Services.AddControllersWithViews();
// Register HttpClient
builder.Services.AddHttpClient();

// Register AppSettings and Print Services
builder.Services.AddScoped<laptop_service.Services.AppSettings.DatabaseService>();
builder.Services.AddScoped<laptop_service.Services.AppSettings.ReceiptPrinterSettingService>();
builder.Services.AddScoped<laptop_service.Services.AppSettings.KOTPrinterSettingService>();
builder.Services.AddScoped<laptop_service.Services.AppSettings.AppSettingsService>();
builder.Services.AddScoped<laptop_service.Services.PrintService>();

#region UserCode
builder.Services.AddControllers();//WEBAPI

#region Swagger_Middleware
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "LAPTOP SERVICE API",
        Description = "LAPTOP SERVICE Swagger Surface",
        Contact = new OpenApiContact
        {
            Name = "BROS ONE TECH",
            Email = "brosonetech@gmail.com",
            Url = new Uri("https://www.brosonetech.com/")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://www.brosonetech.com/LICENSE")
        }
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.<br/>
                        Enter 'Bearer' [space] and then your token in the text input below.<br/>
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

    opt.OperationFilter<RequestHeader>();
});
#endregion Swagger_Middleware

//Adding formatter for Json
builder.Services.AddControllers().AddNewtonsoftJson().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//// Adding formatter for XML
builder.Services.AddControllers().AddXmlSerializerFormatters().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// This is for custom model data validation
// To validate model data globally, uncommand here.
// Or for individual API requestvalidation, use [SuppressModelStateInvalidFilter] for required controller/action
// https://stackoverflow.com/questions/67799780/whay-cannot-hit-the-breakpoint-in-controller-when-modelstate-isvalid-is-false
// Keep in mind that this disables the filter globally. If you want to be able to disable it for specific actions, see https://stackoverflow.com/a/56350823/12431728
// So, we use custom filter option implemented in ApiResponseModel
// To use the filter just add [SuppressModelStateInvalidFilter] above the desired controller action method.
//builder.Services.Configure<ApiBehaviorOptions>(options =>
//{
//    options.SuppressModelStateInvalidFilter = true;
//});


// Set Signleton Configuration service
AppSettingService.Instance.SetConfiguration(builder.Configuration);

// Set App Version in "AppSettingService" class
AppSetting.ApiVersion = AppSettingService.Instance.GetValue("ApiVersion", "0");
Console.WriteLine("AppSetting.ApiVersion : {0}", AppSetting.ApiVersion);

AppSetting.UseHttpsRedirection = AppSettingService.Instance.GetValue("UseHttpsRedirection", true);
Console.WriteLine("AppSetting.UseHttpsRedirection : {0}", AppSetting.UseHttpsRedirection);

AppSetting.CipherKey = AppSettingService.Instance.GetValue("CipherKey", "");
Console.WriteLine("AppSetting.CipherKey : {0}", AppSetting.CipherKey);
AppSetting.IsDataLogEnabled = AppSettingService.Instance.GetValue("IsDataLogEnabled", "0");
Console.WriteLine("AppSetting.IsDataLogEnabled : {0}", AppSetting.IsDataLogEnabled);

AppSetting.IsProduction = AppSettingService.Instance.GetValue("IsProduction", true);
Console.WriteLine("AppSetting.IsProduction : {0}", AppSetting.IsProduction);

AppSetting.AccountActivationUrl = AppSettingService.Instance.GetValue("AccountActivationUrl", "0");
Console.WriteLine("AppSetting.AccountActivationUrl : {0}", AppSetting.AccountActivationUrl);

AppSetting.Jwt = AppSettingService.Instance.Get<JwtConfig>("JwtConfig");
Console.WriteLine("AppSetting.Jwt.Key : {0}", AppSetting.Jwt.Key);
Console.WriteLine("AppSetting.Jwt.Issuer : {0}", AppSetting.Jwt.Issuer);
Console.WriteLine("AppSetting.Jwt.Audience : {0}", AppSetting.Jwt.Audience);
Console.WriteLine("AppSetting.Jwt.TimeOutMinutes : {0}", AppSetting.Jwt.TimeOutMinutes);

//Get OTP Settings
AppSetting.otp = AppSettingService.Instance.Get<OTPConfig>("OTPConfig");


// Set Email Service Configuration (Used to send Forgot Password)
AppSetting.AdminUsername = AppSettingService.Instance.GetValue("AdminUsername", "");
AppSetting.AdminPassword = AppSettingService.Instance.GetValue("AdminPassword", "");
AppSetting.AdminMailID = AppSettingService.Instance.GetValue("AdminMailID", "");
AppSetting.SignupNotificationMailID = AppSettingService.Instance.GetValue("SignupNotificationMailID", "");
AppSetting.MailConfig = AppSettingService.Instance.Get<MailConfig>("MailConfig");

Console.WriteLine("AppSetting.AdminMailID : {0}", AppSetting.AdminMailID);
Console.WriteLine("AppSetting.SignupNotificationMailID : {0}", AppSetting.SignupNotificationMailID);
Console.WriteLine("AppSetting.MailConfig.SenderMailID : {0}", AppSetting.MailConfig.SenderMailID);
Console.WriteLine("AppSetting.MailConfig.FromMailID : {0}", AppSetting.MailConfig.FromMailID);
Console.WriteLine("AppSetting.MailConfig.ReplyToMailID : {0}", AppSetting.MailConfig.ReplyToMailID);
Console.WriteLine("AppSetting.MailConfig.FriendlyName : {0}", AppSetting.MailConfig.FriendlyName);
Console.WriteLine("AppSetting.MailConfig.SmtpClient : {0}", AppSetting.MailConfig.SmtpClient);
Console.WriteLine("AppSetting.MailConfig.SmtpPort : {0}", AppSetting.MailConfig.SmtpPort);
Console.WriteLine("AppSetting.MailConfig.SmtpEnableSsl : {0}", AppSetting.MailConfig.SmtpEnableSsl);
Console.WriteLine("AppSetting.MailConfig.Username : {0}", AppSetting.MailConfig.Username);
Console.WriteLine("AppSetting.MailConfig.Password : {0}", AppSetting.MailConfig.Password);

// Not used this connectionString, because of multitenant concept.
AppSetting.MySQLConnectionString = AppSettingService.Instance.GetValue("ConnectionStrings:MySQL", "");
Console.WriteLine("\r\nAppSetting.MySQLConnectionString : {0}", AppSetting.MySQLConnectionString);

// Read Tenant Database Server List from the Master_Db
string ConStr = SqlService.GetMasterDatabaseConnectionStirng();
AppSetting.TenantDatabaseServerMaster = TenantService.GetTenantDatabaseServerMaster(ConStr);
if (AppSetting.TenantDatabaseServerMaster.Rows.Count > 0)
{
    Console.WriteLine("\r\nSuccess : Reading Tenant_Database_Server_Master List Done.");
    string tenantConnectionString = "";
    foreach (DataColumn dataColumn in AppSetting.TenantDatabaseServerMaster.Columns)
    {
        tenantConnectionString += dataColumn.ColumnName.ToString() + " | ";
    }
    Console.WriteLine(tenantConnectionString);
    foreach (DataRow dataRow in AppSetting.TenantDatabaseServerMaster.Rows)
    {
        tenantConnectionString = "";
        foreach (DataColumn dataColumn in AppSetting.TenantDatabaseServerMaster.Columns)
        {
            tenantConnectionString += dataRow[dataColumn.ColumnName.ToString()].ToString() + " | ";
        }
        Console.WriteLine(tenantConnectionString);
    }
}
else
{
    Console.WriteLine("\r\nFail : Reading Tenant_Database_Server_Master List Failed.");
}
Console.WriteLine("\r\n");

#endregion UserCode

#region UserCode
// JWT Authentication Bearer Token Service
//services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        // Generally Token Expired after 5 minuts of expiry datetime. To make token invalid immediate after expiry time set ClockSkew to TimeSpan.Zero
        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AppSetting.Jwt.Issuer,
        ValidAudience = AppSetting.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Jwt.Key))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            bool tokenValidateStatus = false;
            tokenValidateStatus = JwtService.GetJwtClaimIsValid(context);

            if (tokenValidateStatus == false)
            {
                context.Fail("Unauthorized");
            }

            return Task.CompletedTask;
        }
    };
});
#endregion UserCode
// ==============================================================================================
//builder.WebHost.UseUrls("http://*:5000"); // This will work only after project published.
/*var app = builder.Build();

#region UserCode
var path = Directory.GetCurrentDirectory();
ILoggerFactory loggerFactory = new LoggerFactory();
loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
#endregion UserCode

#region UserCode
// Set App Version in "AppSetting" class
AppSetting.ServerAppPath = app.Environment.ContentRootPath;
AppSetting.ServerwwwwrootPath = app.Environment.WebRootPath;
Console.WriteLine("AppSetting.ServerAppPath : {0}", AppSetting.ServerAppPath);
Console.WriteLine("AppSetting.ServerwwwwrootPath : {0}", AppSetting.ServerwwwwrootPath);
#endregion UserCode

app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (AppSetting.UseHttpsRedirection)
{
    app.UseHttpsRedirection();
}

//
// 🔹 Enable Uploads Folder Access
//
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/Uploads"
});

//
// 🔹 Default static files (wwwroot)
//
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // WEBAPI

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();*/

var app = builder.Build();

#region UserCode
var path = Directory.GetCurrentDirectory();
ILoggerFactory loggerFactory = new LoggerFactory();
loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
#endregion UserCode

#region UserCode
// Set App Version in "AppSetting" class
AppSetting.ServerAppPath = app.Environment.ContentRootPath;
AppSetting.ServerwwwwrootPath = app.Environment.WebRootPath;
Console.WriteLine("AppSetting.ServerAppPath : {0}", AppSetting.ServerAppPath);
Console.WriteLine("AppSetting.ServerwwwwrootPath : {0}", AppSetting.ServerwwwwrootPath);
#endregion UserCode

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (AppSetting.UseHttpsRedirection)
{
    app.UseHttpsRedirection();
}

//
// 🔹 Enable Uploads Folder Access
//
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/Uploads"
});

//
// 🔹 Default static files (wwwroot)
//
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // WEBAPI

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ==========================================================================
