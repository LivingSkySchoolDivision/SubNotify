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
            // Convert the inputted start and end days into UTC, so we are absolutely sure they are in UTC
            // If I run this on another system for testing, they end up in a different time zone and 
            // it really confuses things. The time should read "00:00:00" in UTC.
        
            SubEvent.StartDate = new DateTime(SubEvent.StartDate.Year, SubEvent.StartDate.Month, SubEvent.StartDate.Day, 0, 0, 0, DateTimeKind.Utc);
            SubEvent.EndDate = new DateTime(SubEvent.EndDate.Year, SubEvent.EndDate.Month, SubEvent.EndDate.Day, 0, 0, 0, DateTimeKind.Utc);

            _repository.Update(SubEvent);
        }

        public SubEvent Get(string? id)
        {
            if (id == null) {
                throw new Exception("Id cannot be null");
            } else {
                SubEvent e = _repository.GetById(id);
                //Console.WriteLine(e);
                return e;
            }
        }

        public List<SubEvent> GetUpcoming(School school)
        {
            return _repository.Find(x => (x.SchoolGUID == school.Id) && (x.StartDate >= DateTime.Today.AddDays(1))).ToList();
        }

        public List<SubEvent> GetActive(School school) 
        {
            return _repository.Find(x => (x.SchoolGUID == school.Id) && (x.EndDate >= DateTime.Today) && (x.StartDate <= DateTime.Today.AddHours(23).AddMinutes(59))).ToList();
        }

        public void Cancel(SubEvent SubEvent)
        {
            SubEvent.IsCancelled = true;
            Update(SubEvent);
        }

        public void UnCancel(SubEvent SubEvent)
        {
            SubEvent.IsCancelled = true;
            Update(SubEvent);
        }

        public void CancelUnCancelToggle(SubEvent SubEvent)
        {
            if (SubEvent.IsCancelled)
            {
                UnCancel(SubEvent);
            } else {
                Cancel(SubEvent);
            }
        }

    }
}
