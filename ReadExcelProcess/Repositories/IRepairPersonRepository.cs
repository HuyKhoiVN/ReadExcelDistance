using ReadExcelProcess.Models;

namespace ReadExcelProcess.Repositories
{
    public interface IRepairPersonRepository
    {
        IEnumerable<RepairPerson> GetRepairPersons();
        RepairPerson GetRepairPersonById(string id);
        void AddRepairPerson(RepairPerson repairPerson);
    }
}
