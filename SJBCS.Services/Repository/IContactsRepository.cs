﻿using SJBCS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJBCS.Services.Repository
{
    public interface IContactsRepository
    {
        List<Contact> GetContacts();
        Contact GetContact(Guid id);
        Contact AddContact(Contact Contact);
        Contact UpdateContact(Contact Contact);
        void DeleteContact(Guid id);
    }
}
