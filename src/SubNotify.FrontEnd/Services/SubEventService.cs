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
    public class SubEventService
    {
        private readonly IRepository<SubEvent> _repository;


        public SubEventService(IRepository<SubEvent> Repository)
        {
            this._repository = Repository;
        }

        public IEnumerable<SubEvent> GetAll()
        {
            return _repository.GetAll();
        }

        public void Update(SubEvent obj)
        {
            _repository.Update(obj);
        }

        public void Delete(SubEvent SubEvent) {
            _repository.Delete(SubEvent);
        }

        public void InsertOrUpdate(SubEvent SubEvent) 
        {
            _repository.Update(SubEvent);
        }

        public SubEvent Get(string? id)
        {
            if (id == null) {
                throw new Exception("Id cannot be null");
            } else {
                return _repository.GetById(id);
            }
        }

        public List<SubEvent> GetUpcoming(School school)
        {
            return _repository.Find(x => (x.SchoolGUID == school.Id) && (x.StartDate >= DateTime.Now)).ToList();
        }

        public List<SubEvent> GetActive(School school) 
        {
            return _repository.Find(x => (x.SchoolGUID == school.Id) && (x.EndDate >= DateTime.Now) && (x.StartDate <= DateTime.Now)).ToList();
        }
    }
}
