using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

namespace DigitalSignage.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Departments.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _unitOfWork.Departments.GetAllAsync();
        }

        public async Task<Department> CreateAsync(Department entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Departments.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<Department> UpdateAsync(Department entity)
        {
            await _unitOfWork.Departments.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Departments.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> GetByCompanyIdAsync(int companyId)
        {
            return await _unitOfWork.Departments.GetDepartmentsByCompanyAsync(companyId);
        }

        public async Task<Department?> GetDepartmentWithPagesAsync(int departmentId)
        {
            return await _unitOfWork.Departments.GetDepartmentWithPagesAsync(departmentId);
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsByCompanyAsync(int companyId)
        {
            return await _unitOfWork.Departments.GetActiveDepartmentsByCompanyAsync(companyId);
        }

        public async Task<PagedResult<Department>> GetDepartmentsPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Departments.GetDepartmentsPagedAsync(companyId, pageNumber, pageSize, searchTerm, isActive);
        }
    }
}
