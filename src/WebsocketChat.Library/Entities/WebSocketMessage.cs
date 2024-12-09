using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsocketChat.Library.Entities
{
    public class WebSocketMessage : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [NotMapped]
        public string Token { get; set; }

        [NotMapped]
        public bool IsSystemMessage { get; set; } = false;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; }

        [NotMapped]
        public string UserNickname { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(2000)")]
        public string MessageText { get; set; }
    }


}
