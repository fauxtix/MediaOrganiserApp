using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] dynamic notification)
    {
        // Processar a notificação
        Console.WriteLine(notification);
        return Ok();
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string hubChallenge)
    {
        // Verificação do WebSub (Pub/SubHubbub)
        return Content(hubChallenge);
    }
}
