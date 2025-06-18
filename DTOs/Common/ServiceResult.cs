
using System.Collections.Generic;
using System.Linq;
namespace kalamon_University.DTOs.Common
{
    //��� ������ ������� �������� ���� �� ����� ������ (���: ��� ���ӡ ����� ����� ..).
    public class ServiceResult
    {   //���� �� ��� ���� ������� ����� (true) �� ���� (false). ������ ����������: ���
        public bool Success { get; protected set; } = false; // Default to false
                                                             //����� �������� ���� ���� ����� ����� �������. ���� ����� ��� ���� �������
        public List<string> Errors { get; protected set; } = new List<string>();
        //����� ���� ���� ����� ������� (�����: "�� ����� �����")
        public string Message { get; protected set; } // General message
                                                      //����� ����� ����� �� �����
        public static ServiceResult Succeeded(string message = "Operation successful.")
        {
            return new ServiceResult { Success = true, Message = message };
        }
        //����� ����� ����ɡ ����� ����� �� ������� �������
        public static ServiceResult Failed(params string[] errors)
        {
            return new ServiceResult { Success = false, Errors = errors.ToList() };
        }
        //��� ������ɡ ��� ���� List �� IEnumerable ��� ������
        public static ServiceResult Failed(IEnumerable<string> errors)
        {
            return new ServiceResult { Success = false, Errors = errors.ToList() };
        }
        
        public void AddError(string error)
        {
            if (Errors == null) Errors = new List<string>();
            Errors.Add(error);
            Success = false; // �� ��� ���� �� ������� ������ �� ���� �������
        }
        public void AddErrors(IEnumerable<string> errors)
        {
            if (Errors == null) Errors = new List<string>();
            Errors.AddRange(errors);
            Success = false;
        }
    }
    
}