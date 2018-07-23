using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharepointHelper
{
   public class Utility
    {
        //کنترول کامل     1073741829
        //طراحی     1073741828
        //ویرایش     1073741830
        //سهم گرفتن     1073741827
        //بخوانید     1073741826
        //فقط ببینید     1073741824
       public string SetListItemPermission(SPListItem Item, int userId, int PermissionID, bool ClearPreviousPermissions)
        {
            string strError = "";
            string siteURL = Item.ParentList.ParentWeb.Url;
            Guid listId = Item.ParentList.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPPrincipal byID;
                        Exception exception;
                        web.AllowUnsafeUpdates = true;
                        SPListItem itemById = web.Lists[listId].GetItemById(Item.ID);
                        if (!itemById.HasUniqueRoleAssignments)
                        {
                            itemById.BreakRoleInheritance(!ClearPreviousPermissions);
                        }
                        try
                        {
                            byID = web.SiteUsers.GetByID(userId);
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            byID = web.SiteGroups.GetByID(userId);
                        }
                        SPRoleAssignment roleAssignment = new SPRoleAssignment(byID);
                        SPRoleDefinition roleDefinition = web.RoleDefinitions.GetById(PermissionID);
                        roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                        itemById.RoleAssignments.Remove(byID);
                        itemById.RoleAssignments.Add(roleAssignment);
                        try
                        {
                            itemById.SystemUpdate(false);
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                            strError = exception.Message;
                        }
                    }
                }
            });
            return strError;
        }
    }
}
