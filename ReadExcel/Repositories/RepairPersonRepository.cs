using ReadExcel.Models;
using System;

namespace ReadExcel.Repositories
{
    public class RepairPersonRepository : IRepairPersonRepository
    {
        public List<RepairPerson> _repairPersons = new List<RepairPerson>();
        public void AddRepairPerson(RepairPerson repairPerson)
        {
            _repairPersons.Add(repairPerson);
        }

        public RepairPerson GetRepairPersonById(string id)
        {
            return _repairPersons.FirstOrDefault(r => r.Id == id);
        }

        public IEnumerable<RepairPerson> GetRepairPersons()
        => _repairPersons.ToList();
    }
}
