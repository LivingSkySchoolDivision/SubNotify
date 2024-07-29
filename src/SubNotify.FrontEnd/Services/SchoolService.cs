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
    public class SchoolService
    {
        private readonly IRepository<School> _repository;


        public SchoolService(IRepository<School> Repository)
        {
            this._repository = Repository;
        }

        public IEnumerable<School> GetAll()
        {
            return _repository.GetAll();
        }

        public IEnumerable<School> GetEnabled()
        {
            return _repository.GetAll().Where(x => x.IsEnabled == true);
        }

        public void Update(School obj)
        {
            _repository.Update(obj);
        }

        public void Delete(School school) {
            _repository.Delete(school);
        }

        public void InsertOrUpdate(School school) 
        {
            _repository.Update(school);
        }

        public School Get(string id)
        {
            return _repository.GetById(id);
        }
    }
}
