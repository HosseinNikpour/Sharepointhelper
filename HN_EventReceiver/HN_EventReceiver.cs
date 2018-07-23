using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace sharepointHelper.HN_EventReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class HN_EventReceiver : SPItemEventReceiver
    {
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            //test {0} and test {1} 
            //base.ItemAdded(properties);
            string webUrl = properties.WebUrl;
            SPUserToken sysAdminToken = properties.Site.SystemAccount.UserToken;
            using (SPSite site = new SPSite(webUrl, sysAdminToken))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.GetList("/Lists/eventListTest");
                    SPQuery query = new SPQuery() { Query = string.Format(@"<Where><Eq><FieldRef Name='ListId' /><Value Type='Text'>{0}</Value></Eq></Where>", properties.ListId.ToString().ToUpper()) };
                    SPListItemCollection items = list.GetItems(query);

                    foreach (SPListItem item in items)
                    {

                        if (item["Params"].ToString().Length > 0)
                        {
                            string[] pNames = item["Params"].ToString().Split(',');
                            object[] param = new object[pNames.Length];

                            for (int i = 0; i < pNames.Length; i++)
                            {
                                if (pNames[i].ToUpper().Contains(".LOOKUPID"))
                                {
                                    string[] s = pNames[i].Split('.');
                                    param[i] = new SPFieldLookupValue(properties.ListItem[s[0]].ToString()).LookupId;
                                }
                                else if (pNames[i].ToUpper().Contains(".LOOKUPVALUE"))
                                {
                                    string[] s = pNames[i].Split('.');
                                    param[i] = new SPFieldLookupValue(properties.ListItem[s[0]].ToString()).LookupValue;
                                }
                                else
                                    param[i] = properties.ListItem[pNames[i]].ToString();
                            }
                            properties.ListItem[item["Field"].ToString()] = string.Format(item["Value"].ToString(), param);
                        }
                        else
                        {
                            properties.ListItem[item["Field"].ToString()] = string.Format(item["Value"].ToString());
                        }
                       
                    }
                    if (items.Count > 0)
                        properties.ListItem.SystemUpdate(false);
                }
            }

        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
        }


    }
}