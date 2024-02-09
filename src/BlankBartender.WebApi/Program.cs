using BlankBartender.WebApi.Notifications;
using BlankBartender.WebApi.Services;
using BlankBartender.WebApi.Services.Interfaces;
using BlankBartender.WebApi.WorkerQueues;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using OpenCvSharp;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureEndpointDefaults(listenOptions =>
    {
    });
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IStatusService, StatusService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<IDisplayService, DisplayService>();
builder.Services.AddSingleton<ILightsService, LightsService>();
builder.Services.AddSingleton<IPinService, PinService>();
builder.Services.AddSingleton<IPumpService, PumpService>();
builder.Services.AddSingleton<VideoCapture>();
builder.Services.AddSingleton<IDetectionService, DetectionService>();
builder.Services.AddSingleton<IServoService, ServoService>();
builder.Services.AddTransient<ICocktailService, CocktailService>();

var app = builder.Build();

    var light = new GpioController();
    light.OpenPin(120, PinMode.Output);
    if(light.Read(120) == PinValue.High)
        light.Write(120, PinValue.Low);

    using I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(2, 0x27));
    using var driver = new Pcf8574(i2c);
    using var lcd = new Lcd2004(registerSelectPin: 0,
                            enablePin: 2,
                            dataPins: new int[] { 4, 5, 6, 7 },
                            backlightPin: 3,
                            backlightBrightness: 0.1f,
                            readWritePin: 1,
                            controller: new GpioController(PinNumberingScheme.Logical, driver));
    int currentLine = 0;
    lcd.Clear();
    lcd.SetCursorPosition(0, 0);
    lcd.Write("Machine is ready");
    lcd.SetCursorPosition(0, 1);
    lcd.Write("     for use");

app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyHeader();
    options.AllowAnyMethod();
});

app.UseOpenApi();
app.UseSwaggerUi3();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapHub<ProcessingHub>("/ProcessingHub");

app.Run();
