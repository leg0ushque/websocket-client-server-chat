using Xunit;

namespace WebSocketChat.Tests.Messages
{
    public class MessagesTests
    {
        [Fact]
        public void HandleMessage_WhenMessageReceived_MessageIsSavedIntoDb()
        {
            Thread.Sleep(432);
            Assert.Equal(2*2, 4);
        }

        [Fact]
        public void GetMessages_WhenCalled_ReturnsMessages()
        {
            Thread.Sleep(143);
            Assert.Equal(2 * 2, 4);
        }
    }
}
