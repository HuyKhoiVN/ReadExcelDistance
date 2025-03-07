using Newtonsoft.Json;
using ReadExcel.DTO;
using ReadExcel.Models;
using ReadExcel.Repositories;

namespace ReadExcel.Service
{
    public class OptimizationService : IOptimizationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAssignmentService _assignmentService;
        private readonly IRepairPersonRepository _repairPersonRepository;

        public OptimizationService(
            IHttpClientFactory httpClientFactory,
            IAssignmentService assignmentService,
            IRepairPersonRepository repairPersonRepository)
        {
            _httpClientFactory = httpClientFactory;
            _assignmentService = assignmentService;
            _repairPersonRepository = repairPersonRepository;
        }
        public async Task ProcessOptimizationDataAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var requestUri = "http://10.14.117.15:8000/optimize";

            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var optimizationResult = JsonConvert.DeserializeObject<OptimizationResultDto>(json);

            if (optimizationResult != null && optimizationResult.assignments != null)
            {
                foreach (var scheduled in optimizationResult.assignments)
                {
                    var repairPerson = new RepairPerson
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"Worker {scheduled.worker}",
                        TotalWorkTime = scheduled.total_time,
                        assignments = new List<Assignment>()
                    };

                    foreach (var taskId in scheduled.tasks)
                    {
                        var assignmentDetail = _assignmentService.GetAssignmentByTaskId(taskId);
                        if (assignmentDetail != null)
                        {
                            var assignment = new Assignment
                            {
                                Id = Guid.NewGuid().ToString(),
                                Location = assignmentDetail.Location,
                                RepairTime = assignmentDetail.RepairTime,
                                RepairPersonId = repairPerson.Id
                            };

                            repairPerson.assignments.Add(assignment);
                        }
                    }
                    _repairPersonRepository.AddRepairPerson(repairPerson);
                }
            }
        }
    }
}
