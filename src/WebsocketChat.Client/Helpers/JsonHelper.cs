using Newtonsoft.Json;
using System.Text;

namespace WebsocketChat.Client.Helpers
{
    public static class JsonHelper
    {
        public static async Task<T> DeserializeContentAsync<T>(this HttpResponseMessage responseMessage)
        {
            var jsonContent = await responseMessage?.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<T>(jsonContent);
            return response;
        }

        public static StringContent ObjectToStringContent(object objToStringContent)
        {
            var content = JsonConvert.SerializeObject(objToStringContent);
            return new StringContent(content, Encoding.UTF8, "application/json");
        }
    }
}
