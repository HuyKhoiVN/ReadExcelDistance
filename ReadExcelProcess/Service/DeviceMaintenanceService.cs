using Microsoft.EntityFrameworkCore;
using ReadExcelProcess.Model;

namespace ReadExcelProcess.Service
{
    public class DeviceMaintenanceService : IDeviceMaintenanceService
    {
        private readonly SysDbContext _sysDbContext;

        public DeviceMaintenanceService(SysDbContext sysDbContext)
        {
            _sysDbContext = sysDbContext;
        }

        public async Task GenerateMaintenanceSchedulesAsync()
        {
            var devices = await _sysDbContext.Devices.Where(d => !string.IsNullOrEmpty(d.ContractNumber)).ToListAsync();
            foreach (var device in devices)
            {
                Contract contract = null;
                if (!string.IsNullOrEmpty(device.SubContractNumber))
                {
                    contract = await _sysDbContext.Contracts.
                        FirstOrDefaultAsync(c => c.ContractNumberChildren == device.SubContractNumber);
                }
                else
                {
                    contract = await _sysDbContext.Contracts.
                        FirstOrDefaultAsync(c => c.ContractNumberParent == device.ContractNumber);
                }
                if (contract == null || !contract.StartDate.HasValue ||
                !contract.EndDate.HasValue || !contract.TimeMaintenance.HasValue ||
                contract.TimeMaintenance.Value <= 0)
                {
                    continue;
                }
                DateTime contractStart = contract.StartDate.Value;
                DateTime contractEnd = contract.EndDate.Value;
                int frequencyMonths = contract.TimeMaintenance.Value;
                var existingSchedules = _sysDbContext.DeviceMaintenanceSchedules.Where(s => s.DeviceId == device.Id);
                _sysDbContext.DeviceMaintenanceSchedules.RemoveRange(existingSchedules);
                DateTime currentMaintenanceDate = contractStart;
                int maintenanceIndex = 1;
                int maintainceEarlyDate = -2;
                int maintainceDelayDate = 2;
                while (currentMaintenanceDate <= contractEnd)
                {
                    var schedule = new DeviceMaintenanceSchedule
                    {
                        DeviceId = device.Id,
                        EffectiveDate = currentMaintenanceDate,
                        MaintenanceStartDate = currentMaintenanceDate.AddDays(maintainceEarlyDate),
                        MaintenanceEndDate = currentMaintenanceDate.AddDays(maintainceDelayDate),
                        ContractNumber = !string.IsNullOrEmpty(device.SubContractNumber)
                                        ? device.SubContractNumber : device.ContractNumber,
                        MaintenanceTimes = maintenanceIndex,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                    };
                    await _sysDbContext.DeviceMaintenanceSchedules.AddAsync(schedule);

                    maintenanceIndex++;
                    currentMaintenanceDate = currentMaintenanceDate.AddMonths(frequencyMonths);
                }
            }
            await _sysDbContext.SaveChangesAsync();
        }
    }
}