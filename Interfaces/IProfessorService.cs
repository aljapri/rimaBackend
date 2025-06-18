using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kalamon_University.Models.Entities;

namespace kalamon_University.Interfaces
{
    /// <summary>
    /// ����� ������ ����� �������ɡ ���� �������� �������� (CRUD).
    /// </summary>
    public interface IProfessorService
    {
        /// <summary>
        /// ��� ����� ����� �������� �� ������ �������� �������� ���.
        /// </summary>
        /// <returns>����� ����� ��� ���� ��������.</returns>
        Task<IEnumerable<Professor>> GetAllAsync();

        /// <summary>
        /// ����� �� ����� ���� �������� ������ ����� �� (UserId).
        /// </summary>
        /// <param name="professorId">������ ������ (Guid) �������.</param>
        /// <returns>���� ������� ��� �� ������ ���� (�� ������ ���� ������)� ���� ����� null.</returns>
        Task<Professor?> GetByIdAsync(Guid professorId);

        /// <summary>
        /// ����� ����� ���� ��� ����� ��������.
        /// </summary>
        /// <param name="professor">���� ������� ���� ����� ��� �������� �������.</param>
        /// <returns>���� ������� ��� ������ �����.</returns>
        Task<Professor> AddAsync(Professor professor);

        /// <summary>
        /// ����� ������ ����� �����.
        /// </summary>
        /// <param name="professor">���� ������� ���� ����� ��� �������� �������.</param>
        /// <returns>Task ���� ��� ������ �������.</returns>
        Task UpdateAsync(Professor professor);

        /// <summary>
        /// ��� ����� �� ����� �������� ����� ��� �����.
        /// </summary>
        /// <param name="professorId">������ ������ (Guid) ������� ������ ����.</param>
        /// <returns>True ��� ��� ����� ����� ����͡ � False ��� �� ��� ������ ��� �������.</returns>
        Task<bool> DeleteAsync(Guid professorId);
    }
}