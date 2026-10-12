using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Up01_Noviokov_Koskina_Donation
{
    public partial class Entities
    {
        private static Entities _context;

        public static Entities GetCotext()
        {
            if (_context == null)
                _context = new Entities();
            return _context;
        }
    }
}
