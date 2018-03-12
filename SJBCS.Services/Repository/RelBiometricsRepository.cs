﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJBCS.Data;

namespace SJBCS.Services.Repository
{
    public class RelBiometricsRepository : IRelBiometricsRepository
    {
        AmsDbContext _context = new AmsDbContext();

        public RelBiometric AddRelBiometric(RelBiometric relBiometric)
        {
            _context.RelBiometrics.Add(relBiometric);
            _context.SaveChanges();
            return relBiometric;
        }

        public  void DeleteRelBiometric(Guid id)
        {
            var relBiometric = _context.RelBiometrics.FirstOrDefault(r => r.RelBiometricID == id);
            if (relBiometric != null)
            {
                _context.RelBiometrics.Remove(relBiometric);
            }
            _context.SaveChanges();
        }

        public  RelBiometric GetRelBiometric(Guid id)
        {
            return _context.RelBiometrics.FirstOrDefault(r => r.FingerID == id);
        }

        public  List<RelBiometric> GetRelBiometrics()
        {
            return _context.RelBiometrics.ToList();
        }

        public  RelBiometric UpdateRelBiometric(RelBiometric relBiometric)
        {
            if (!_context.RelBiometrics.Local.Any(r => r.RelBiometricID == relBiometric.RelBiometricID))
            {
                _context.RelBiometrics.Attach(relBiometric);
            }
            _context.Entry(relBiometric).State = EntityState.Modified;
            _context.SaveChanges();
            return relBiometric;
        }
    }
}
