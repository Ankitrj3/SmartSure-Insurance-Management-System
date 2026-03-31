using System.ComponentModel.DataAnnotations;
namespace SmartSure.AdminService.Models
{
    /// <summary>
    /// Represent or implements AuditLog.
    /// </summary>
    public class AuditLog
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = "";

        public Guid? EntityId { get; set; }

        public Guid? ActorId { get; set; }  // UserId who performed the action

        [StringLength(500)]
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
