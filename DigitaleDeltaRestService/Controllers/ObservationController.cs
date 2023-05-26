namespace DigitaleDeltaRestService.Controllers;

using Microsoft.AspNetCore.Mvc;

// API-20: Include the major version number in the URI
[ApiController]
[Route("v3/observation")]
public class ObservationController : ControllerBase
{
	// /: POST (body: Observation)
	// /status/{id}: GET
	// /history: GET
	// /bulk/observation: POST
	// /bulk/{id}: GET
}