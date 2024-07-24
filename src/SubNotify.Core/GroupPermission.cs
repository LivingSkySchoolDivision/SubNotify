
namespace SubNotify.Core;

public class GroupPermission : IGUIDable
{
    public Guid Id { get; set; }
    public string GroupOID { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool CanRequestSubs { get; set; }
    public bool CanManageAvailableSubs { get; set; }
    public bool CanManagePermissions { get; set; }
    public bool CanSeeAllSchools { get; set; }
    public List<Guid> SchoolGUIDs { get; set; } = new List<Guid>();
    
}
