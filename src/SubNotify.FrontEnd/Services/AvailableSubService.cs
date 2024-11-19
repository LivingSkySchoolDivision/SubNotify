using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using SubNotify.Core;

namespace SubNotify.FrontEnd.Services
{
    public class AvailableSubService
    {
        private readonly IRepository<AvailableSub> _repository;


        public AvailableSubService(IRepository<AvailableSub> Repository)
        {
            this._repository = Repository;
        }

        public IEnumerable<AvailableSub> GetAll()
        {
            return _repository.GetAll().OrderBy(x => x.DisplayName);
        }

        public IEnumerable<AvailableSub> GetEnabled()
        {
            return _repository.GetAll().Where(x => x.IsEnabled == true).OrderBy(x => x.DisplayName);
        }

        public void Update(AvailableSub obj)
        {
            _repository.Update(obj);
        }

        public void Delete(AvailableSub AvailableSub) {
            _repository.Delete(AvailableSub);
        }

        public void InsertOrUpdate(AvailableSub AvailableSub) 
        {
            _repository.Update(AvailableSub);
        }

        public AvailableSub Get(string? id)
        {
            if (id == null) {
                throw new Exception("Id cannot be null");
            } else {
                return _repository.GetById(id);
            }
        }

        public List<AvailableSub> GetEnabledForSchoolGUID(Guid schoolGuid)
        {
            return this.GetEnabled().Where(x => x.IsEnabled).Where(x => x.SchoolGUIDs.Contains(schoolGuid)).OrderBy(x => x.DisplayName).ToList();
        }
    }
}
