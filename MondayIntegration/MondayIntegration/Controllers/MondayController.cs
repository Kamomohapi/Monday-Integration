using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MondayIntegration.Entities;
using Newtonsoft.Json;
using System.Text;

namespace MondayIntegration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MondayController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public MondayController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpPost("Webhooks")]

        public async Task<IActionResult> HandleWebHook([FromBody] MondayWebhookPayload payload)
        {
            if (payload == null || payload.mondayEvent == null)
                return BadRequest("Invalid payload");

            string boardId = "1867981782";

            var subitemId = await CreateSubitem(boardId, payload.mondayEvent.itemId);

            if (!string.IsNullOrEmpty(subitemId))
            {
                // Step 2: Update status of the subitem
                await UpdateSubitemStatus(subitemId, "Working on it");
            }

            return Ok();
        }
        private async Task<string> CreateSubitem(string boardId, string parentId)
        {
            var query = new
            {
                query = $"mutation {{ create_subitem (parent_item_id: {parentId}, board_id: {boardId}) {{ id }} }}"
            };

            var response = await SendGraphQLRequest(query);
            return response?.data?.create_subitem?.id;
        }
        private async Task UpdateSubitemStatus(string subitemId, string status)
        {
            var query = new
            {
                query = $"mutation {{ change_column_value (item_id: {subitemId}, column_id: \"status\", value: \"{status}\") {{ id }} }}"
            };

            await SendGraphQLRequest(query);
        }

        private async Task<dynamic> SendGraphQLRequest(object queryObject)
        {
            string apiUrl = "https://api.monday.com/v2";
            string apiKey = "eyJhbGciOiJIUzI1NiJ9.eyJ0aWQiOjM0NjUyODQzNiwiYWFpIjoxMSwidWlkIjo1NTc0NDcyMCwiaWFkIjoiMjAyNC0wNC0xM1QwOTo0MjoyNy4wMDBaIiwicGVyIjoibWU6d3JpdGUiLCJhY3RpZCI6MjEyNTIzMjAsInJnbiI6ImV1YzEifQ.Ou_Etys5jbpHvCktaYWq0AtZtUOEzZao1G70JSfYN2k";

            var requestContent = new StringContent(JsonConvert.SerializeObject(queryObject), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

            var response = await _httpClient.PostAsync(apiUrl, requestContent);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<dynamic>(jsonResponse);
        }

    }
}
