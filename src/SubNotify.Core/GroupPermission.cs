
namespace SubNotify.Core;

public class GroupPermission : IGUIDable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GroupClaim { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    
    // Administrative permissions
    public bool CanManageSubList { get; set; } = false;
    public bool CanManagePermissions { get; set; } = false;
    public bool CanManageSchoolList { get; set; } = false;
    public bool CanSeeAllSchools { get; set; } = false;

    // School permissions
    // Note: The permissions system should do a sanity check on the schools here, in case data is mangled or malicious
    public List<Guid> SchoolGUIDs { get; set; } = new List<Guid>();
}
