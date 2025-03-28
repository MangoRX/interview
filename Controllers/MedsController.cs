using Aspnet.React.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aspnet.React.Controllers;


[Route("api/[controller]")]
[ApiController]
public class MedsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;

    public MedsController(IConfiguration configuration, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
    {
        _configuration = configuration;
        _apiBehaviorOptions = apiBehaviorOptions;
    }

    [HttpGet]
    [Route("NDCs")]
    public async Task<ActionResult<JwtData>> NDCs()
    {
        /**
        * Instructions
        * Given this API endpoint for the FDA NDC database: https://api.fda.gov/drug/ndc.json?search=finished:true&limit=10
        * Send the list of NDCs to the client.
        * Note: Some drugs have multiple packages, so you may include variations as a list of comma separated values.
        */
        var medications = new List<Medication>
        {
            new Medication
            {
                Ndc = "0002-1433-80",
                Name = "Acetaminophen",
                Route = "Oral",
                Packaging = "100 tablets/bottle"
            },
            new Medication
            {
                Ndc = "0002-1434-80",
                Name = "Ibuprofen",
                Route = "Oral",
                Packaging = "50 tablets/bottle"
            }
        };
        return Ok(medications);
    }

    [HttpGet]
    [Route("meta")]
    public async Task<ActionResult<JwtData>> Meta()
    {
        /**
        * Instructions
        * Given this API endpoint for the FDA NDC database: https://api.fda.gov/drug/ndc.json?search=finished:true&limit=10
        * Calculate some stats for the NDCs.
        * Total: Count of NDCs, this simply matches the limit in the url.
        * Num_Oral: Count of NDCs with the route "Oral"
        * Num_Topical: Count of NDCs with the route "Topical"
        * Num_Other: Count of NDCs with all other routes "Other"
        */
        var medications = new MedicationMeta
        {
            Total = 100,
            Num_Oral = 50,
            Num_Topical = 30,
            Num_Other = 20
        }
        ;
        return Ok(medications);
    }
}