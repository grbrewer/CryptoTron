using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq;
using System.Security.Principal;

namespace CryptoManager
{
    class CustomPrincipal: IPrincipal
    {
        private CustomIdentity _identity;
 
        public CustomIdentity Identity
        {
            get { return _identity ?? new AnonymousIdentity(); }
            set { _identity = value; }
        }
     
        #region IPrincipal Members
        IIdentity IPrincipal.Identity
        {
            get { return this.Identity; } 
        }
 
        public bool IsInRole(string role)
        {
            return _identity.Roles.Contains(role);
        }
        #endregion
    }
    
}
