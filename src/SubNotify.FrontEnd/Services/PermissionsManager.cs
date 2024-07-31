using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using SubNotify.Core;

namespace SubNotify.FrontEnd.Services
{
    public class PermissionsManager
    {            
        private readonly TimeSpan _cacheExpiry = new TimeSpan(0,30,0);
        private readonly SchoolService _schoolRepo;
        private readonly GroupPermissionService _permissionRepository;
        private readonly Dictionary<Guid,School> allSchools = new Dictionary<Guid,School>();
        private readonly Dictionary<string, GroupPermission> _cachedUserPermissions = new Dictionary<string, GroupPermission>();
        private readonly Dictionary<string, DateTime> _cachedUserPermissions_LastUpdate = new Dictionary<string, DateTime>();

        public PermissionsManager(GroupPermissionService groupPermissionService, SchoolService schoolService)        
        {            
            this._schoolRepo = schoolService;
            this._permissionRepository = groupPermissionService; 

            List<School> _schools = _schoolRepo?.GetAll().OrderBy(x => x.Name).ToList<School>() ?? new List<School>();
            allSchools = _schools.ToDictionary(x => x.Id);

            this.FlushPermissions();            
        }
        
        private List<GroupPermission> _cachedPermissions = new List<GroupPermission>();

        public void FlushPermissions()
        {
            // Clear the cache
            _cachedPermissions.Clear();
            _cachedPermissions = _permissionRepository?.GetAll().ToList() ?? new List<GroupPermission>();

            _cachedUserPermissions.Clear();
            _cachedUserPermissions_LastUpdate.Clear();
        }

        public List<GroupPermission> GetGranularPermissionsFor(ClaimsPrincipal user)
        {
            return GetPermissionsForClaims(user.Claims);
        }

        private List<GroupPermission> GetPermissionsForClaims(IEnumerable<Claim> userClaims)
        {
            // Convert claims to just a string list
            List<string> groupClaims = userClaims.Where(x => x.Type == "groups").Select(x => x.Value.ToString()).ToList();
            return GetPermissionsForClaims(groupClaims);
        }

        private List<GroupPermission> GetPermissionsForClaims(List<string> userClaims)
        {
            List<GroupPermission> returnMe = new List<GroupPermission>();            

            foreach(GroupPermission perm in _cachedPermissions.Where(x => x.IsEnabled))
            {
                if (userClaims.Contains(perm.GroupClaim))
                {
                    returnMe.Add(perm);
                }
            }

            return returnMe;
        }


        public GroupPermission GetCombinedPermissions(ClaimsPrincipal? user)
        {
            // If the user object was null coming in, don't waste any time figuring anything out
            if (user == null)
            {
                return new GroupPermission();
            }

            // Check for invalid stuff
            string username = user?.Identity?.Name ?? string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                return new GroupPermission();
            }

            // Check for cached stuff
            if (_cachedUserPermissions.ContainsKey(username))
            {
                if (_cachedUserPermissions_LastUpdate.ContainsKey(username))
                {
                    TimeSpan timeSinceLastCacheUpdate = DateTime.Now - _cachedUserPermissions_LastUpdate[username];

                    if (timeSinceLastCacheUpdate >= _cacheExpiry)
                    {
                        _cachedUserPermissions.Remove(username);
                        _cachedUserPermissions_LastUpdate.Remove(username);
                        // Don't return anything, so we fall through to generating fresh data and caching it
                    } else {
                        return _cachedUserPermissions[username];
                    }
                }
            }       

            // Generate fresh permissions object
            IEnumerable<Claim> userclaims = user?.Claims ?? new List<Claim>();
            GroupPermission thisUserCombinedPermissions = GetCombinedPermissions(userclaims);

            // Update the cache
            Console.WriteLine("Updating permissions cache for user " + username);
            if (_cachedUserPermissions.ContainsKey(username))
            {
                _cachedUserPermissions.Remove(username);
            }
            if (_cachedUserPermissions_LastUpdate.ContainsKey(username))
            {
                _cachedUserPermissions_LastUpdate.Remove(username);
            }
            
            _cachedUserPermissions.Add(username, thisUserCombinedPermissions);
            _cachedUserPermissions_LastUpdate.Add(username, DateTime.Now);

            // Return              
            return _cachedUserPermissions[username];            
        }
        

        // Creates a new temporary GroupPermission object that's the combination of all others this user has
        private GroupPermission GetCombinedPermissions(IEnumerable<Claim> userClaims)
        {
            GroupPermission combinedPermission = new GroupPermission()
            {
                Name = "-",
                Description = "-",
                GroupClaim = "n/a"
            };

            foreach(GroupPermission perm in GetPermissionsForClaims(userClaims))
            {
                if (perm.CanManageSubList)                
                {
                    combinedPermission.CanManageSubList = true;
                }

                if (perm.CanManagePermissions)                
                {
                    combinedPermission.CanManagePermissions = true;
                }

                if (perm.CanManageSchoolList)                
                {
                    combinedPermission.CanManageSchoolList = true;
                }

                if (perm.CanSeeAllSchools)                
                {
                    combinedPermission.CanSeeAllSchools = true;
                }

                // Add all schools, if any perm has that set
                if (perm.CanSeeAllSchools) {
                    combinedPermission.SchoolGUIDs = allSchools.Values.Select(x => x.Id).ToList();
                }

                // Add specific schools attached to this perm (if they arent already added)
                foreach(Guid schoolid in perm.SchoolGUIDs)
                {
                    if (!combinedPermission.SchoolGUIDs.Contains(schoolid))
                    {
                        combinedPermission.SchoolGUIDs.Add(schoolid);
                    }
                }
            }

            return combinedPermission;
        }

        public bool CanManageSubList(ClaimsPrincipal? user) 
        {            
            if (user == null) { return false; }
            GroupPermission userCombinedPermissions = this.GetCombinedPermissions(user);
            return userCombinedPermissions.CanManageSubList;
        }

        public bool CanManagePermissions(ClaimsPrincipal? user) 
        {            
            if (user == null) { return false; }
            GroupPermission userCombinedPermissions = this.GetCombinedPermissions(user);
            return userCombinedPermissions.CanManagePermissions;
        }

        public bool CanManageSchoolList(ClaimsPrincipal? user) 
        {            
            if (user == null) { return false; }
            GroupPermission userCombinedPermissions = this.GetCombinedPermissions(user);
            return userCombinedPermissions.CanManageSchoolList;
        }

        public bool CanSeeAllSchools(ClaimsPrincipal? user) 
        {         
            if (user == null) { return false; }   
            GroupPermission userCombinedPermissions = this.GetCombinedPermissions(user);
            return userCombinedPermissions.CanSeeAllSchools;
        }

        public List<School> GetSchoolsForUser(ClaimsPrincipal? user)
        {            
            GroupPermission userCombinedPermissions = this.GetCombinedPermissions(user);
            
            List<School> returnMe = new List<School>();

            foreach(Guid schoolGUID in userCombinedPermissions.SchoolGUIDs) 
            {
                if (allSchools.ContainsKey(schoolGUID))
                {
                    returnMe.Add(allSchools[schoolGUID]);
                }
            }

            return returnMe;
        }


    }
}