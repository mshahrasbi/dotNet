using Microsoft.EntityFrameworkCore.Internal;
using ParkyAPI.Data;
using ParkyAPI.Models;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.Repository
{
    public class NationalParkRepository : INationalParkRepository
    {

        private ApplicationDbContext _db;

        public NationalParkRepository(ApplicationDbContext db)
        {
            this._db = db;
        }

        public bool CreateNationalPark(NationalPark nationalPark)
        {
            this._db.NationalParks.Add(nationalPark);
            return this.Save();
        }

        public bool DeleteNationalPark(NationalPark nationalPark)
        {
            this._db.NationalParks.Remove(nationalPark);
            return this.Save();
        }

        public NationalPark GetNationalPark(int nationalParkId)
        {
            return this._db.NationalParks.FirstOrDefault(i => i.Id == nationalParkId);
        }

        public ICollection<NationalPark> GetNationalParks()
        {
            return this._db.NationalParks.OrderBy(a => a.Name).ToList();
        }

        public bool NationalParkExists(string name)
        {
            return this._db.NationalParks.Any(a => a.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public bool NationalParkExists(int id)
        {
            return this._db.NationalParks.Any(a => a.Id == id);
        }

        public bool Save()
        {
            return this._db.SaveChanges() >= 0 ? true : false;
        }

        public bool UpdateNationalPark(NationalPark nationalPark)
        {
            this._db.NationalParks.Update(nationalPark);
            return this.Save();
        }
    }
}
