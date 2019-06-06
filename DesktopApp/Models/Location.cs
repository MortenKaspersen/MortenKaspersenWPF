using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class Location
    {
        //string id = Guid.NewGuid().ToString();
        string id;
        string name;
        string adresse;
        string måletræer;

        public Location()
        {
        }

        public Location(string aId, string aName, string aAdresse, string aMåletræer)
        {
            id = aId;
            name = aName;
            adresse = aAdresse;
            måletræer = aMåletræer;
        }


        /// <summary>
        /// Performs only a shallow copy
        /// </summary>
        /// <returns></returns>
        public Location Clone()
        {
            return this.MemberwiseClone() as Location;
        }


        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string Adresse
        {
            get
            {
                return adresse;
            }
            set
            {
                adresse = value;
            }
        }

        public string Måletræer
        {
            get
            {
                return måletræer;
            }
            set
            {
                måletræer = value;
            }
        }
    }
}
