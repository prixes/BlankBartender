using BlankBartender.Shared;
using BlankBartender.WebApi.WorkerQueues;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Collections.Generic;
using System.Diagnostics;
using Iot.Device.ExplorerHat;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using System.Device.I2c;
using BlankBartender.WebApi.Services.Interfaces;
using BlankBartender.WebApi.Services;

namespace BlankBartender.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DrinkController : ControllerBase
{
    private readonly string _drinkConfigJson;
    private readonly string _pumpConfigJson;

    private const string _pumpConfigFileName = "pump-config.json";

    private readonly IEnumerable<Pump> _pumps;
    private IEnumerable<Drink> _drinks;
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<DrinkController> _logger;
    private readonly ILightsService _lightsService;
    private readonly IDisplayService _displayService;
    private readonly ICocktailService _cocktailService;
    private readonly IPinService _pinService;
    private readonly IStatusService _statusService;

    public DrinkController(IBackgroundTaskQueue queue, ILogger<DrinkController> logger, ILightsService lightsService, IDisplayService displayService,
                               ICocktailService cocktailService, IPinService pinService, IStatusService statusService)
    {
        _queue = queue;
        _logger = logger;
        _cocktailService = cocktailService;
        _statusService = statusService;
#if !DEBUG
        _lightsService = lightsService;
        _displayService = displayService;
        _pinService = pinService;
#endif
        var pumpFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", _pumpConfigFileName);

        if (System.IO.File.Exists(pumpFilePath))
        {
            _pumpConfigJson = System.IO.File.ReadAllText(pumpFilePath);
        }


        if (!string.IsNullOrEmpty(_pumpConfigJson))
        {
            JObject pumpJsonObject = JObject.Parse(_pumpConfigJson);

            _pumps = pumpJsonObject["pumps"].Select(p => new Pump
            {
                Number = int.Parse(p["number"].ToString()),
                Pin = short.Parse(p["pin"].ToString()),
                Value = p["value"].ToString()
            }).ToList();
        }
        _statusService = statusService;
    }

    public IActionResult StartProcessing()
    {
        _queue.QueueBackgroundWorkItem(async token =>
        {
            // put processing code here
        });

        return Ok();
    }

    [Route("available/all/")]
    public async Task<ActionResult> GetAvailableDrinks()
    {
#if !DEBUG
//        _lightsService.TurnLight("blue", false);
//        _lightsService.TurnLight("red", false);
        _displayService.MachineReadyForUse();
#endif
        _drinks = _cocktailService.GetAvaiableCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [Route("all/")]
    public async Task<ActionResult> GetDrinks()
    {
#if !DEBUG
//        _lightsService.TurnLight("blue", false);
//        _lightsService.TurnLight("red", false);
        _displayService.MachineReadyForUse();
#endif
        _drinks = _cocktailService.GetAllCocktails();
        return new JsonResult(new
        {
            Drinks = _drinks
        });
    }

    [Route("process")]
    public async Task<ActionResult> ProcessDrink(IEnumerable<Pump> model)
    {
        var action = async (Pump pump) =>
        {
#if !DEBUG
            _lightsService.StartCocktailLights();
            _pinService.SwitchPin(pump.Pin, true);
#endif
            //Console.WriteLine($"Start pump {pump.Name} on pin {pump.Pin} for {pump.Time}ms");
            Console.WriteLine($"Start pump {pump.Number} on pin {pump.Pin} for {pump.Time}ms");
            Thread.Sleep((int)pump.Time); // need to tweak it
            // Console.WriteLine($"Stop pump {pump.Name} on pin {pump.Pin} that worked for {pump.Time} ms");
            Console.WriteLine($"Stop pump {pump.Number} on pin {pump.Pin} that worked for {pump.Time} ms");
            //await Task.Delay(3000);
#if !DEBUG
            _pinService.SwitchPin(pump.Pin, false);
#endif
        };

        StatusCodeResult responseResult = Ok();
        var cancellationToken = new CancellationToken();
        List<Task> taskCollection = new List<Task>();

        foreach (var pump in model)
        {
            Task actionTask = new Task(async () => await action(pump));
            taskCollection.Add(actionTask);
            var continuation = actionTask.ContinueWith((Task task) =>
            {
                if (task.IsCompleted)
                {
                    responseResult = Ok();
                }
                else
                {
                    responseResult = BadRequest();
                }
            });
            _queue.QueueBackgroundWorkItem(async (CancellationToken token) =>
            {

                actionTask.Start();
            }/*async (CancellationToken token) => await actionTask*/);
        }


        Task.WaitAll(taskCollection.ToArray());

        await CocktailDoneLightsAndDisplay();
        _statusService.StopRunning();

        return responseResult;
    }

    [HttpGet]
    [Route("make/cocktail/{id}")]
    public async Task<ActionResult> MakeCocktail(int id)
    {
        _statusService.StartRunning();
        _drinks = _cocktailService.GetAvaiableCocktails();
        var drink = _drinks.FirstOrDefault(x => x.Id == id);
        //TODO if not exists handle exception
        Console.WriteLine($"Received request for {drink.Name}");
        var recipe = new List<Pump>();
        foreach (var ingridient in drink.Ingradients)
        {
            var time = ingridient.Value * 1220; // its like 1.22 sec for each mililiter of liquid if we follow pump spec
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            //TODO if not exists handle exception 
            // here if we change configuration of pump/alcohol tuple alcohol just could not be avaiable right now  so error handling should be returning that some/all ingredients are not available
            pump.Time = time;
            recipe.Add(pump);
            Console.WriteLine($"Found and added ingredient {ingridient.Key} amount:{ingridient.Value} (taking {time / 1000} seconds) coresponding to Pump{pump.Number}");
        }
        var timeToMakeCocktail = (int)recipe.Select(x => x.Time).Max() / 1050;

#if !DEBUG
        _displayService.PrepareStartDisplay(drink);

        Task actionTask = new Task(async () => await _displayService.Countdown(timeToMakeCocktail));
        actionTask.Start();
#else
        Console.WriteLine("dummy thread sleep");
        Thread.Sleep(5000);
#endif


        return await ProcessDrink(recipe);
    }

    [HttpPost]
    [Route("make/cocktail/custom")]
    public async Task<ActionResult> MakeCustomCocktail(Drink drink)
    {
        _statusService.StartRunning();
        //TODO if not exists handle exception
        Console.WriteLine($"Received request for {drink.Name}");
        var recipe = new List<Pump>();
        foreach (var ingridient in drink.Ingradients)
        {
            var time = ingridient.Value * 1220; // its like 1.22 sec for each mililiter of liquid if we follow pump spec
            var pump = _pumps.FirstOrDefault(x => x.Value == ingridient.Key);
            //TODO if not exists handle exception 
            // here if we change configuration of pump/alcohol tuple alcohol just could not be avaiable right now  so error handling should be returning that some/all ingredients are not available
            pump.Time = time;
            recipe.Add(pump);
            Console.WriteLine($"Found and added ingredient {ingridient.Key} amount:{ingridient.Value} (taking {time / 1000} seconds) coresponding to Pump{pump.Number}");
        }
        var timeToMakeCocktail = (int)recipe.Select(x => x.Time).Max() / 1050;

#if !DEBUG
        _displayService.PrepareStartDisplay(drink);

        Task actionTask = new Task(async () => await _displayService.Countdown(timeToMakeCocktail));

        actionTask.Start();
#endif
        Console.WriteLine($"start poring");
        return await ProcessDrink(recipe);
    }


    private async Task CocktailDoneLightsAndDisplay()
    {
#if !DEBUG
        Console.WriteLine($"Cocktail is done!");

        _lightsService.TurnLight("green", true);
        _displayService.CocktailReadyDisplay();

        Thread.Sleep(6000);

        _lightsService.TurnLight("red", false);
        _displayService.MachineReadyForUse();
#endif
    }
}
