using BlankBartender.Shared;
using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Services.Interfaces;
using BlankBartender.WebApi.Configuration;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DrinkController : ControllerBase
{
    private IEnumerable<Pump> _pumps;
    private IEnumerable<Drink>? _drinks;
    private readonly ILightsService _lightsService;
    private readonly IDisplayService _displayService;
    private readonly ICocktailService _cocktailService;
    private readonly IPinService _pinService;
    private readonly IStatusService _statusService;
    private readonly IPumpService _pumpService;
    private readonly IDetectionService? _detectionService;
    private readonly IServoService _servoService;
    private readonly ISettingsService _settingsService;
    private SettingsValues _settingsValues;

    public DrinkController(ILightsService lightsService, IDisplayService displayService,
                           ICocktailService cocktailService, IPinService pinService,
                           IStatusService statusService, IPumpService pumpService,
                           IDetectionService detectionService, IServoService servoService,
                           ISettingsService settingsService)
    {
        _cocktailService = cocktailService;
        _lightsService = lightsService;
        _displayService = displayService;
        _pinService = pinService;
        _statusService = statusService;
        _pumpService = pumpService;
        _servoService = servoService;
        _settingsService = settingsService;
        _detectionService = detectionService;

        _pumps = _pumpService.GetConfiguration();
        _settingsValues = _settingsService.GetMachineSettingsAsync().Result;   
    }

    [HttpGet("available/all/")]
    public ActionResult GetAvailableDrinks()
    {

#if !DEBUG
        _lightsService.TurnLight("green", true);
        _displayService.MachineReadyForUse();
#endif
        _drinks = _cocktailService.GetAvaiableCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [HttpGet("all/")]
    public ActionResult GetDrinks()
    {
#if !DEBUG
        _lightsService.TurnLight("green", true);
        _displayService.MachineReadyForUse();
#endif
        _drinks = _cocktailService.GetAllCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [HttpPost("process")]
    public async Task<ActionResult> ProcessDrink([FromBody] IEnumerable<Pump> model, string name = "", CancellationToken cancellationToken = default)
    {
        _settingsValues = _settingsService.GetMachineSettingsAsync().Result;
        //_lightsService.StartCocktailLights();
        await Task.Delay(1580, cancellationToken);

        if (_settingsValues.UseCameraAI)
        {
            _displayService.PlaceGlassMessage();
            var timeout = TimeSpan.FromSeconds(30);
            var stopTime = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < stopTime)
            {
                var detected = await (_detectionService?.DetectGlass() ?? Task.FromResult(false));
                if (detected)
                {
                    Console.WriteLine($"Glass detected success");
                    break;
                }
                else
                {
                    Console.WriteLine($"Glass detected failed");
                    await Task.Delay(1580, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            if (DateTime.UtcNow > stopTime)
            {
                await _displayService.Clear();
                await _displayService.WriteFirstLineDisplay("cocktail cancel!");
                await Task.Delay(1580, cancellationToken);
                _lightsService.TurnLight("green", true);
                _displayService.MachineReadyForUse();
                return Ok();
            }
        }

        await _displayService.PrepareStartDisplay(name);

        async Task ExecutePumpAction(Pump pump)
        {
            _pinService.SwitchPin(pump.Pin, true);
            Console.WriteLine($"Start pump {pump.Number} on pin {pump.Pin} for {pump.Time?.ToString("00000.00")} ms");
            var delayMs = (int)(pump.Time ?? 0m);
            await Task.Delay(delayMs, cancellationToken);
            Console.WriteLine($"Stop pump {pump.Number} on pin {pump.Pin} that worked for {pump.Time?.ToString("00000.00")} ms");

            _pinService.SwitchPin(pump.Pin, false);
        }

        try
        {
            var maxTime = model?.Max(x => x.Time) ?? 0m;
            var timeToMakeCocktail = (int)(maxTime / 1050m);
            if (_settingsValues.UseStirrer)
                timeToMakeCocktail += 18;
            if (_settingsValues.UseIceDispenser)
                timeToMakeCocktail += 19;

            // start countdown without blocking
            _ = _displayService.Countdown(timeToMakeCocktail);

            if (_settingsValues.UseIceDispenser)
            {
                _servoService.MovePlatformToIceDispenser();
                Console.WriteLine($"Start Ice Dispenser");
                _servoService.MoveAngleServo300();
                _servoService.MovePlatformFromIceToStart();
                await Task.Delay(1000, cancellationToken);
            }

            var tasks = model.Select(pump => ExecutePumpAction(pump)).ToList();
            await Task.WhenAll(tasks);

            if (_settingsValues.UseStirrer)
            {
                await Task.Delay(2500, cancellationToken);
                //Stirring process part
                _servoService.MovePlatformToStirrer();
                _servoService.MoveStirrerToGlass();
                Console.WriteLine($"Start Stirrer");
                _pinService.SwitchPin(147, true);
                await Task.Delay(3000, cancellationToken);
                Console.WriteLine($"wait");
                Console.WriteLine($"go up");
                _servoService.MoveStirrerToStart();
                Console.WriteLine($"Start stop");
                _pinService.SwitchPin(147, false);
                _servoService.MovePlatformToStart();
            }

            await CocktailDoneLightsAndDisplay();
            await _statusService.StopRunning();
            return Ok();
        }
        catch (OperationCanceledException)
        {
            await _statusService.StopRunning();
            return new StatusCodeResult(499);
        }
        catch
        {
            return BadRequest();
        }
    }


    [HttpGet("make/cocktail/{id}")]
    public async Task<ActionResult> MakeCocktail(int id, CancellationToken cancellationToken = default)
    {
        _statusService.StartRunning();
        _drinks = _cocktailService.GetAvaiableCocktails();
        _pumps = _pumpService.GetConfiguration();

        var drink = _drinks.FirstOrDefault(x => x.Id == id);
        if (drink == null)
        {
            // TODO: Handle the exception if the drink is not found.
            return NotFound($"No cocktail with ID {id} found.");
        }

        Console.WriteLine($"Received request for {drink.Name}");

        var recipe = drink.Ingredients.Select(ingridient =>
        {
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key) ?? throw new Exception($"Pump for ingredient {ingridient.Key} not found.");
            var time = ingridient.Value * 1000m / pump.FlowRate;
            pump.Time = time;
            Console.WriteLine($"Found and added ingredient {ingridient.Key} amount:{ingridient.Value:0.00} (taking {time / 1000:0.00} seconds) corresponding to Pump {pump.Number}");
            return pump;
        }).ToList();

        await _displayService.PrepareStartDisplay(drink.Name);
        var result = await ProcessDrink(recipe, drink.Name, cancellationToken);
        return result;
    }


    [HttpPost("make/cocktail/custom")]
    public async Task<ActionResult> MakeCustomCocktail(Drink drink, CancellationToken cancellationToken = default)
    {
        _statusService.StartRunning();
        _pumps = _pumpService.GetConfiguration();


        if (drink == null || string.IsNullOrEmpty(drink.Name))
        {
            // TODO: Handle error if the drink is null or has no name.
            return BadRequest("Invalid drink provided.");
        }

        Console.WriteLine($"Received request for {drink.Name}");

        var recipe = new List<Pump>();
        foreach (var ingridient in drink.Ingredients)
        {
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            if (pump == null)
            {
                return BadRequest($"Pump for ingredient {ingridient.Key} not found.");
            }
            var time = ingridient.Value * 1000m / pump.FlowRate;
            pump.Time = time;
            Console.WriteLine($"Added ingredient {ingridient.Key} amount:{ingridient.Value:0.00} (taking {time / 1000:0.00} seconds) to Pump{pump.Number}");
            recipe.Add(pump);
        }

        var maxRecipeTime = recipe.Max(x => x.Time) ?? 0m;
        var timeToMakeCocktail = (int)(maxRecipeTime / 1000m);

        await _displayService.PrepareStartDisplay(drink.Name);

        Console.WriteLine($"Start pouring");
        return await ProcessDrink(recipe, drink.Name, cancellationToken);
    }

    [HttpPost("cocktail/create")]
    public async Task<ActionResult> AddCocktail(Drink newDrink)
    {
        try
        {
            _cocktailService.AddCocktail(newDrink);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
        return Ok();
    }

    private async Task CocktailDoneLightsAndDisplay()
    {
        Console.WriteLine($"Cocktail is done!");

        _lightsService.TurnLight("green", true);
        _displayService.CocktailReadyDisplay();

        await Task.Delay(4000);

        _lightsService.TurnLight("red", false);
        _displayService.MachineReadyForUse();
    }


}
