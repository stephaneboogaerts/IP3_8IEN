﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IP_8IEN.DAL;

namespace IP_8IEN.BL
{
    public class UnitOfWorkManager
    {
        private UnitOfWork uof;

        internal UnitOfWork UnitOfWork
        {
            get
            {   //Om via buitenaf te verzekeren dat er géén onnodige nieuwe
                //instanaties van UnitOfWork geïnstantieerd worden...
                if (uof == null) uof = new UnitOfWork();
                return uof;
            }
        }

        public void Save()
        {
            UnitOfWork.CommitChanges();
        }
    }
}
