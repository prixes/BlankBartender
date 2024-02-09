using BlankBartender.Shared;
using Microsoft.AspNetCore.Mvc;
using BlankBartender.WebApi.Services.Interfaces;
using System.Device.Gpio;
using BlankBartender.WebApi.Services;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DrinkController : ControllerBase
{
    private readonly IEnumerable<Pump> _pumps;
    private IEnumerable<Drink> _drinks;
    private readonly ILightsService _lightsService;
    private readonly IDisplayService _displayService;
    private readonly ICocktailService _cocktailService;
    private readonly IPinService _pinService;
    private readonly IStatusService _statusService;
    private readonly IPumpService _pumpService;
    private readonly IDetectionService _detectionService;
    private readonly IServoService _servoService;

    private readonly GpioController _gpioController = new GpioController();

    public DrinkController(ILightsService lightsService,     IDisplayService displayService,
                           ICocktailService cocktailService, IPinService pinService, 
                           IStatusService statusService,     IPumpService pumpService, 
                           IDetectionService detectionService,IServoService servoService)
    {
        _cocktailService = cocktailService;
        _statusService = statusService;
        _lightsService = lightsService;
        _displayService = displayService;
        _pinService = pinService;
        _statusService = statusService;
        _pumpService = pumpService;
        _servoService = servoService;

        _pumps = _pumpService.GetConfiguration();
        _detectionService = detectionService;
}

    [Route("available/all/")]
    public async Task<ActionResult> GetAvailableDrinks()
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
    public async Task<ActionResult> GetDrinks()
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
        _lightsService.StartCocktailLights();
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
        if(DateTime.UtcNow > stopTime)
        {
            await _displayService.Clear();
            await _displayService.WriteFirstLineDisplay("cocktail cancel!");
            Thread.Sleep(1580);
            _lightsService.TurnLight("green", true);
            _displayService.MachineReadyForUse();
            return Ok();
        }

        await _displayService.PrepareStartDisplay(name);

        async Task ExecutePumpAction(Pump pump)
        {
            _pinService.SwitchPin(pump.Pin, true);
            Console.WriteLine($"Start pump {pump.Number} on pin {pump.Pin} for {pump.Time}ms");
            await Task.Delay((int)pump.Time);  // Replacing Thread.Sleep with Task.Delay in async methods.
            Console.WriteLine($"Stop pump {pump.Number} on pin {pump.Pin} that worked for {pump.Time} ms");

            _pinService.SwitchPin(pump.Pin, false);
        }

        var tasks = model.Select(pump => Task.Run(() => ExecutePumpAction(pump))).ToList();

        try
        {
            var timeToMakeCocktail = (int)model.Max(x => x.Time) / 1050;
            var countdownTask = _displayService.Countdown(timeToMakeCocktail);
            tasks.Add(countdownTask);
            await Task.WhenAll(tasks);

            //Stirring process part
            _servoService.MovePlatformToStirrer();
            _servoService.MoveStirrerToGlass();
            TurnStirrer(on:true);
            Thread.Sleep(4000);
            _servoService.MoveStirrerToStart();
            TurnStirrer(on: false);
            _servoService.MovePlatformToStart();

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
            var time = ingridient.Value * 1220 * pump.FlowRate;

            if (pump == null)
            {
                // TODO: Handle the exception if the pump is not found.
                throw new Exception($"Pump for ingredient {ingridient.Key} not found.");
            }
            pump.Time = time;
            Console.WriteLine($"Found and added ingredient {ingridient.Key} amount:{ingridient.Value} (taking {time / 1000} seconds) corresponding to Pump{pump.Number}");
            return pump;
        }).ToList();

        await _displayService.PrepareStartDisplay(drink.Name);

        return await ProcessDrink(recipe);
    }


    [HttpPost]
    [Route("make/cocktail/custom")]
    public async Task<ActionResult> MakeCustomCocktail(Drink drink)
    {
        _statusService.StartRunning();

        if (drink == null || string.IsNullOrEmpty(drink.Name))
        {
            // TODO: Handle error if the drink is null or has no name.
            return BadRequest("Invalid drink provided.");
        }

        Console.WriteLine($"Received request for {drink.Name}");

        var recipe = drink.Ingradients.Select(ingridient =>
        {
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            var time = ingridient.Value * 1220 * pump.FlowRate;

            if (pump == null)
            {
                throw new Exception($"Pump for ingredient {ingridient.Key} not found.");
            }

            pump.Time = time;
            Console.WriteLine($"Added ingredient {ingridient.Key} amount:{ingridient.Value} (taking {time / 1000} seconds) to Pump{pump.Number}");
            return pump;

        }).ToList();

        var timeToMakeCocktail = (int)recipe.Max(x => x.Time) / 1050;

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

    private void TurnStirrer(bool on)
    {
            var pinValue = on ? PinValue.Low : PinValue.High;
            if (_gpioController.Read(147) != pinValue)
                _gpioController.Write(147, pinValue);
    }
}
