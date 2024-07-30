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
    public class GroupPermissionService
    {
        private readonly IRepository<GroupPermission> _repository;


        public GroupPermissionService(IRepository<GroupPermission> Repository)
        {
            this._repository = Repository;
        }

        public IEnumerable<GroupPermission> GetAll()
        {
            return _repository.GetAll();
        }

        public IEnumerable<GroupPermission> GetEnabled()
        {
            return _repository.GetAll().Where(x => x.IsEnabled == true);
        }

        public void Update(GroupPermission obj)
        {
            _repository.Update(obj);
        }

        public void Delete(GroupPermission GroupPermission) {
            _repository.Delete(GroupPermission);
        }

        public void InsertOrUpdate(GroupPermission GroupPermission) 
        {
            _repository.Update(GroupPermission);
        }

        public GroupPermission Get(string? id)
        {
            if (id == null) {
                throw new Exception("Id cannot be null");
            } else {
                return _repository.GetById(id);
            }
        }
    }
}
