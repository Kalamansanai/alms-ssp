using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class TaskInstance : BaseEntity
    {
        public Task Task { get; set; } = default!;
        public List<Event> Events { get; set; } = default!;
    }
}