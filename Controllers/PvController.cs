using System;
using Microsoft.AspNetCore.Mvc;
using nigo.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace nigo.Controllers
{
    //[Authorize]
    //[EnableCors("AllowGoluxSoftWebApp")] 
    [Route("api/[controller]")]
    [ApiController]
    public class PvWattsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public PvWattsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = new HttpClient();
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> GetPvWattsData(PvParams pvModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }
            
            try
            {
                string apiUrl = $"https://developer.nrel.gov/api/pvwatts/v8.json?" +
                                $"lat={pvModel.lat}&lon={pvModel.lon}&system_capacity={pvModel.systemCapacity}" +
                                $"&azimuth={pvModel.azimuth}&tilt={pvModel.tilt}&array_type={pvModel.arrayType}" +
                                $"&module_type={pvModel.moduleType}&losses={pvModel.losses}&dataset={pvModel.dataset}";


                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                request.Headers.Add("x-api-key", "QaNO9CImgVK9NoxnOaSavi4nS0GsSwFiR5pnN6qI");
                
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                
                if (response.IsSuccessStatusCode)
                {
                    PvResponse responseData = await response.Content.ReadFromJsonAsync<PvResponse>();

                    return Ok(responseData);
                }

                if ((int)response.StatusCode == 422)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var errorJson = JObject.Parse(errorResponse);

                    var errors = errorJson["errors"].Values<string>().ToList();

                    return StatusCode((int)response.StatusCode, new { errors = errors });
                }
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}

