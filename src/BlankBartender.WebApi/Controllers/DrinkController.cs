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
    private readonly IDetectionService _detectionService;
    private readonly IServoService _servoService;
    private readonly ISettingsService _settingsService;
    private SettingsValues _settinsValues;

    public DrinkController(ILightsService lightsService,     IDisplayService displayService,
                           ICocktailService cocktailService, IPinService pinService, 
                           IStatusService statusService,     IPumpService pumpService, 
                           IDetectionService detectionService,IServoService servoService,
                           ISettingsService settingsService)
    {
        _cocktailService = cocktailService;
        _statusService = statusService;
        _lightsService = lightsService;
        _displayService = displayService;
        _pinService = pinService;
        _statusService = statusService;
        _pumpService = pumpService;
        _servoService = servoService;
        _settingsService = settingsService;

        _pumps = _pumpService.GetConfiguration();
        _settinsValues = _settingsService.GetMachineSettings();
        if(_settinsValues.UseCameraAI)
        {
            _detectionService = detectionService;
        }
}

    [Route("available/all/")]
    public ActionResult GetAvailableDrinks()
    {
        _lightsService.TurnLight("green", true);
        _displayService.MachineReadyForUse();
        _drinks = _cocktailService.GetAvaiableCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [Route("all/")]
    public ActionResult GetDrinks()
    {
        _lightsService.TurnLight("green", true);
        _displayService.MachineReadyForUse();
        _drinks = _cocktailService.GetAllCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [Route("process")]
    public async Task<ActionResult> ProcessDrink(IEnumerable<Pump> model, string name = "")
    {
        _settinsValues = _settingsService.GetMachineSettings();
        //_lightsService.StartCocktailLights();
        Thread.Sleep(1580);
        if(_settinsValues.UseCameraAI)
        {
            _displayService.PlaceGlassMessage();
            var timeout = TimeSpan.FromSeconds(30);
            var stopTime = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < stopTime)
            {
                if (await _detectionService.DetectGlass())
                {
                    Console.WriteLine($"Glass detected success");
                    break;
                }
                else
                {
                    Console.WriteLine($"Glass detected failed");
                    Thread.Sleep(1580);
                }
            }
            if (DateTime.UtcNow > stopTime)
            {
                await _displayService.Clear();
                await _displayService.WriteFirstLineDisplay("cocktail cancel!");
                Thread.Sleep(1580);
                _lightsService.TurnLight("green", true);
                _displayService.MachineReadyForUse();
                return Ok();
            }
        }
       
        await _displayService.PrepareStartDisplay(name);

        async Task ExecutePumpAction(Pump pump)
        {
            _pinService.SwitchPin(pump.Pin, true);
            Console.WriteLine($"Start pump {pump.Number} on pin {pump.Pin} for {pump.Time.Value.ToString("00000.00")} ms");
            await Task.Delay((int)pump.Time);  // Replacing Thread.Sleep with Task.Delay in async methods.
            Console.WriteLine($"Stop pump {pump.Number} on pin {pump.Pin} that worked for {pump.Time.Value.ToString("00000.00")} ms");

            _pinService.SwitchPin(pump.Pin, false);
        }

        var tasks = model.Select(pump => Task.Run(() => ExecutePumpAction(pump))).ToList();

        try
        {
            var timeToMakeCocktail = (int)model.Max(x => x.Time) / 1050;
            if (_settinsValues.UseStirrer)
                timeToMakeCocktail += 16;

            Task.Run(() => _displayService.Countdown(timeToMakeCocktail));

            await Task.WhenAll(tasks);
            if (_settinsValues.UseStirrer)
            {
                Thread.Sleep(2500);
                //Stirring process part
                _servoService.MovePlatformToStirrer();
                _servoService.MoveStirrerToGlass();
                Console.WriteLine($"Start Stirrer");
                //await _stirrerService.StartStirrer();
                _pinService.SwitchPin(147, true);
                await Task.Delay(3000);
                Console.WriteLine($"wait");
                Console.WriteLine($"go up");
                _servoService.MoveStirrerToStart();
                Console.WriteLine($"Start stop");
                _pinService.SwitchPin(147, false);
                //_stirrerService.StopStirrer();
                _servoService.MovePlatformToStart();
            }

            await CocktailDoneLightsAndDisplay();
            _statusService.StopRunning();
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }


    [HttpGet]
    [Route("make/cocktail/{id}")]
    public async Task<ActionResult> MakeCocktail(int id)
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

        var recipe = drink.Ingradients.Select(ingridient =>
        {
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            var time = ingridient.Value * 1000 / pump.FlowRate;

            if (pump == null)
            {
                // TODO: Handle the exception if the pump is not found.
                throw new Exception($"Pump for ingredient {ingridient.Key} not found.");
            }
            pump.Time = time;
            Console.WriteLine($"Found and added ingredient {ingridient.Key} amount:{ingridient.Value.ToString("0.00")} (taking {time / 1000:0.00} seconds) corresponding to Pump {pump.Number}");
            return pump;
        }).ToList();

        await _displayService.PrepareStartDisplay(drink.Name);
        ProcessDrink(recipe, drink.Name);
        return Ok();
    }


    [HttpPost]
    [Route("make/cocktail/custom")]
    public async Task<ActionResult> MakeCustomCocktail(Drink drink)
    {
        _statusService.StartRunning();
        _pumps = _pumpService.GetConfiguration();


        if (drink == null || string.IsNullOrEmpty(drink.Name))
        {
            // TODO: Handle error if the drink is null or has no name.
            return BadRequest("Invalid drink provided.");
        }

        Console.WriteLine($"Received request for {drink.Name}");

        var recipe = drink.Ingradients.Select(ingridient =>
        {
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            var time = ingridient.Value * 1000 / pump.FlowRate;

            if (pump == null)
            {
                throw new Exception($"Pump for ingredient {ingridient.Key} not found.");
            }

            pump.Time = time;
            Console.WriteLine($"Added ingredient {ingridient.Key} amount:{ingridient.Value:0.00} (taking {time / 1000:0.00} seconds) to Pump{pump.Number}");
            return pump;

        }).ToList();

        var timeToMakeCocktail = (int)recipe.Max(x => x.Time) / 1000;

        await _displayService.PrepareStartDisplay(drink.Name);

        Console.WriteLine($"Start pouring");
        return await ProcessDrink(recipe, drink.Name);
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
