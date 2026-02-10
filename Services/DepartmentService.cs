using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _departmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _departmentRepository.GetAllAsync();
        }

        public async Task<Department> CreateAsync(Department entity)
        {
            return await _departmentRepository.AddAsync(entity);
        }

        public async Task<Department> UpdateAsync(Department entity)
        {
            await _departmentRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _departmentRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Department>> GetByCompanyIdAsync(int companyId)
        {
            return await _departmentRepository.FindAsync(d => d.CompanyID == companyId);
        }
    }
}
