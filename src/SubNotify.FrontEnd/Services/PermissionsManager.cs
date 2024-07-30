using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using SubNotify.Core;

namespace SubNotify.FrontEnd.Services
{
    public class PermissionsManager
    {            
        private readonly SchoolService _schoolRepo;
        private readonly GroupPermissionService _permissionRepository;
        private readonly List<School> allSchools = new List<School>();

        public PermissionsManager(GroupPermissionService groupPermissionService, SchoolService schoolService)        
        {            
            this._schoolRepo = schoolService;
            this._permissionRepository = groupPermissionService; 

            allSchools = _schoolRepo?.GetAll().OrderBy(x => x.Name).ToList() ?? new List<School>();
            this.FlushPermissions();            
        }
        
        private List<GroupPermission> _cachedPermissions = new List<GroupPermission>();

        public void FlushPermissions()
        {
            // Clear the cache
            _cachedPermissions.Clear();
            _cachedPermissions = _permissionRepository?.GetAll().ToList() ?? new List<GroupPermission>();
        }

        public List<GroupPermission> GetPermissionsForClaims(IEnumerable<Claim> userClaims)
        {
            // Convert claims to just a string list
            List<string> groupClaims = userClaims.Where(x => x.Type == "groups").Select(x => x.Value.ToString()).ToList();
            return GetPermissionsForClaims(groupClaims);
        }

        public List<GroupPermission> GetPermissionsForClaims(List<string> userClaims)
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

        // Creates a new temporary GroupPermission object that's the combination of all others this user has
        public GroupPermission GetCombinedPermissions(IEnumerable<Claim> userClaims)
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
                    combinedPermission.SchoolGUIDs = allSchools.Select(x => x.Id).ToList();
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

    }
}