using System;

namespace WebsocketChat.Library.Models
{
    public class SentMessageModel
    {
        public bool? IsReceived { get; set; }

        public string SenderNickname { get; set; }

        public string Message { get; set; }

        public DateTime Date { get; set; }
    }
}
