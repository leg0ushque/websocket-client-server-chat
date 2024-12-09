using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsocketChat.Library.Entities
{
    public interface IEntity
    {
        public string Id { get; set; }
    }
}
