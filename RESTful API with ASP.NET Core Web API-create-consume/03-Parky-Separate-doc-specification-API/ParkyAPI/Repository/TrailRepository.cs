using Microsoft.EntityFrameworkCore;
using ParkyAPI.Data;
using ParkyAPI.Models;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.Repository
{
    public class TrailRepository : ITrailRepository
    {

        private ApplicationDbContext _db;

        public TrailRepository(ApplicationDbContext db)
        {
            this._db = db;
        }

        public bool CreateTrail(Trail trail)
        {
            this._db.Trails.Add(trail);
            return this.Save();
        }

        public bool DeleteTrail(Trail trail)
        {
            this._db.Trails.Remove(trail);
            return this.Save();
        }

        public Trail GetTrail(int trailId)
        {
            return this._db.Trails.Include(c => c.NationalPark).FirstOrDefault(i => i.Id == trailId);
        }

        public ICollection<Trail> GetTrails()
        {
            return this._db.Trails.Include(c => c.NationalPark).OrderBy(a => a.Name).ToList();
        }

        public ICollection<Trail> GetTrailsInNationalPark(int nationalParkId)
        {
            return this._db.Trails.Include(c => c.NationalPark).Where(c => c.NationalParkId == nationalParkId).ToList();
        }

        public bool TrailExists(string name)
        {
            return this._db.Trails.Any(a => a.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public bool TrailExists(int id)
        {
            return this._db.Trails.Any(a => a.Id == id);
        }

        public bool Save()
        {
            return this._db.SaveChanges() >= 0 ? true : false;
        }

        public bool UpdateTrail(Trail trail)
        {
            this._db.Trails.Update(trail);
            return this.Save();
        }

    }
}
