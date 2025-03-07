using ReadExcel.Models;

namespace ReadExcel.Repositories
{
    public interface IRepairPersonRepository
    {
        IEnumerable<RepairPerson> GetRepairPersons();
        RepairPerson GetRepairPersonById(string id);
        void AddRepairPerson(RepairPerson repairPerson);
    }
}
