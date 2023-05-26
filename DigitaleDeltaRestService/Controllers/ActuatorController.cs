namespace DigitaleDeltaRestService.Controllers;

using Microsoft.AspNetCore.Mvc;

// API-20: Include the major version number in the URI
[ApiController]
[Route("v3/actuator")]
public class ActuatorController : ControllerBase
{
	// /: POST (body: Observation)
	// /: PATCH (body: Observation)
}