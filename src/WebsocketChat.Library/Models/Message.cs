using System;

namespace WebsocketChat.Library.Models
{
    public class Message
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string MessageText { get; set; }
    }
}
