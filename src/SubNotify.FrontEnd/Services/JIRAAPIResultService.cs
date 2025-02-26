using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using SubNotify.Core;
using System.Linq.Expressions;

namespace SubNotify.FrontEnd.Services
{
    public class JIRAAPIResultService
    {
        private readonly IRepository<JIRAAPIResult> _repository;


        public JIRAAPIResultService(IRepository<JIRAAPIResult> Repository)
        {
            this._repository = Repository;
        }

        public IEnumerable<JIRAAPIResult> GetAll()
        {
            return _repository.GetAll().OrderByDescending(x => x.TimestampUTC);
        }

        public void Update(JIRAAPIResult obj)
        {
            _repository.Update(obj);
        }

        public void Delete(JIRAAPIResult JIRAAPIResult) {
            _repository.Delete(JIRAAPIResult);
        }

        public void InsertOrUpdate(JIRAAPIResult JIRAAPIResult) 
        {
            _repository.Update(JIRAAPIResult);
        }

        public JIRAAPIResult Get(string? id)
        {
            if (id == null) {
                throw new Exception("Id cannot be null");
            } else {
                return _repository.GetById(id);
            }
        }
                
        public List<JIRAAPIResult> GetForSubEvent(SubEvent subEvent)
        {
            return _repository.Find(x => (x.SubEventID == subEvent.Id)).OrderByDescending(x => x.TimestampUTC).ToList();
        }
    }
}
