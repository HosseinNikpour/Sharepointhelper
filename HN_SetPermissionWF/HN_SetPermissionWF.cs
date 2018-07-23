using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Microsoft.SharePoint.Utilities;

namespace sharepointHelper.HN_SetPermissionWF
{
    public sealed partial class HN_SetPermissionWF : SequentialWorkflowActivity
    {
        public HN_SetPermissionWF()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        private void MoveItemToFolder_ExecuteCode(object sender, EventArgs e)
        {

            string siteURL = workflowProperties.WebUrl;
            Guid listId = workflowProperties.ListId;
            int id = workflowProperties.ItemId;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList list = web.Lists[listId];
                        SPListItem item = list.GetItemById(id);
                        string folderName = createFolder(web, list, item);
                        string destination = list.RootFolder.Name + "/" + folderName + "/" + item.File.Name;
                        item.File.MoveTo(destination, true);
                    }
                }
            });
        }

        private SPListItem getSettingItem(SPWeb web)
        {
            SPList settinglist = web.GetList("/Lists/ListSetting");
            SPQuery query = new SPQuery() { Query = string.Format(@"<Where><Eq><FieldRef Name='ListId' /><Value Type='Text'>{0}</Value></Eq></Where>", workflowProperties.ListId.ToString().ToUpper()) };
            SPListItemCollection items = settinglist.GetItems(query);
            if (items.Count == 1)
            {
                return items[0];
            }
            else
                return null;
        }
        private string createFolder(SPWeb web, SPList list, SPListItem item)
        {

            SPListItem sItem = getSettingItem(web);
            if (sItem != null) //Item exists in setting list
            {
                if (sItem["PermissionField"] != null)//(create Folder) - set permission of folder - move Item To folder 
                {
                    if (list.EnableFolderCreation != true)
                    {
                        list.EnableFolderCreation = true;
                        web.AllowUnsafeUpdates = true;
                        list.Update();
                    }

                    //Todo : handel look up ex: .lookupId 
                    //Todo : handel PermissionFieldDynamic
                   // string FolderNameField = sItem["PermissionFieldDynamic"] == null ? sItem["PermissionField"].ToString() : sItem["PermissionFieldDynamic"].ToString();
                    string FolderNameField = sItem["PermissionField"].ToString();
                    if (item[FolderNameField] != null)
                    {
                        string FolderName = item[FolderNameField].ToString();

                        if (!list.ParentWeb.GetFolder(list.RootFolder.ServerRelativeUrl + "/" + FolderName).Exists)
                        {  //create folder  and set permission 
                            SPListItem folder = list.Items.Add(list.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, FolderName);
                            folder["Title"] = FolderName;
                            web.AllowUnsafeUpdates = true;
                            folder.Update();
                            setPermission(sItem, folder);
                            return FolderName;
                        }
                        else //folder Exist
                        {
                            return FolderName;
                        }
                    }
                    else
                    {   // Error
                        // permission field is epmty in current item
                        return "-1";
                    }
                }
                else //no need folder - set permission of Item
                {
                    setPermission(sItem, item);
                }
            }
            // there is no item in setting list
            return "0";
        }

        private void setPermission(SPListItem sListItem, SPListItem Item)
        {
            Utility util = new Utility();
            SPFieldLookupValueCollection Editors = (sListItem["Editors"] != null) ? new SPFieldLookupValueCollection(sListItem["Editors"].ToString()) : null;
            SPFieldLookupValueCollection Viewers = (sListItem["Viewers"] != null) ? new SPFieldLookupValueCollection(sListItem["Viewers"].ToString()) : null;
            Item.BreakRoleInheritance(false, true);
            foreach (SPFieldLookupValue lookup in Viewers)
            {
                util.SetListItemPermission(Item, lookup.LookupId, 1073741826, false);
            }
            foreach (SPFieldLookupValue lookup in Editors)
            {
                util.SetListItemPermission(Item, lookup.LookupId, 1073741827, false);
            }
        }
    }
}
