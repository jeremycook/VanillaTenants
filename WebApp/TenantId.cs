using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    public class TenantId
    {
        public TenantId(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
