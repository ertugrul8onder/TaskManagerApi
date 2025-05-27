using AutoMapper;
using Models; // Assuming your DTOs and Entities are in the Models namespace

namespace TaskManagerApi.Mappings
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            // For creating a new task
            CreateMap<CreateTaskDto, Models.Task>();

            // For updating an existing task
            CreateMap<UpdateTaskDto, Models.Task>();

            // For potentially pre-populating forms or other scenarios (optional for now)
            CreateMap<Models.Task, CreateTaskDto>(); 
            CreateMap<Models.Task, UpdateTaskDto>();

            // This mapping is useful if you want to clone a Task entity or map it to a generic Task DTO
            // If you introduce a specific TaskResponseDto, you would map to that instead for Get operations.
            CreateMap<Models.Task, Models.Task>(); 
        }
    }
}
