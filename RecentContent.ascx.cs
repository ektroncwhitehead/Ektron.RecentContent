using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Ektron.Cms;
using Ektron.Cms.Common;
using Ektron.Cms.Content;
using Ektron.Cms.Framework.Content;
using Ektron.Cms.Framework.User;
using Ektron.Cms.Widget;

/// <summary>
/// An Ektron workarea widget to display content recently edited by the current logged in user.
/// </summary>
public partial class RecentContentWidget : WorkareaWidgetBaseControl, IWidget
{
    /// <summary>
    /// Models a recently edited content item
    /// </summary>
    private class RecentContentRecord
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string Editor { get; set; }
        public DateTime Modified { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }
    }

    //defaults
    const int DAYS = 7;
    const int ITEMS = 10;
    const bool PUBLISHED = false;

    /// <summary>
    /// The number of days worth of history to show
    /// </summary>
    [WidgetDataMember(DAYS)]
    public int DaysLimit { get; set; }

    /// <summary>
    /// The maximum number of items to show
    /// </summary>
    [WidgetDataMember(ITEMS)]
    public int ItemLimit { get; set; }

    /// <summary>
    /// If true, only display published content items. Otherwise, display all recently edited content items, regardless of state.
    /// </summary>
    [WidgetDataMember(PUBLISHED)]
    public bool PublishedOnly { get; set; }

    /// <summary>
    /// Boilerplate widget code
    /// </summary>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        base.Host.Edit += new EditDelegate(EditEvent);
        base.Host.Maximize += new MaximizeDelegate(delegate() { Visible = true; });
        base.Host.Minimize += new MinimizeDelegate(delegate() { Visible = false; });
        base.Host.Create += new CreateDelegate(delegate() { EditEvent(""); });
        ViewSet.SetActiveView(View);
    }

    /// <summary>
    /// Event handler for the widget's pre-render phase
    /// </summary>
    protected void Page_PreRender(object sender, EventArgs e)
    {
        //if the user is viewing their list of recently edited content
        if (this.ViewSet.ActiveViewIndex == 0)
        {
            //load this user's data
            var contentData = this.loadContentData();
            var dataSource = this.loadWidgetData(contentData);
            this.bindWidget(dataSource);
        }

        //set the title bar of the widget

        StringBuilder title = new StringBuilder("<strong>Recent Content");

        if (this.PublishedOnly)
            title.Append(" (Published Only)");

        if (this.DaysLimit > 0)
            title.AppendFormat(" - Since {0}", DateTime.Now.AddDays(-DaysLimit).ToString("MM/dd/yyyy"));

        title.Append("</strong> (click the check at the far right to modify your Recent Content settings)");

        base.SetTitle(title.ToString());
    }

    /// <summary>
    /// Event handler called as the editor view is about to be displayed to the user.
    /// </summary>
    protected void EditEvent(string settings = "")
    {
        //populate the controls with the user's current settings

        uxDaysLimit.Text = this.DaysLimit.ToString();
        uxItemLimit.Text = this.ItemLimit.ToString();

        uxPublishedRadio.SelectedIndex = this.PublishedOnly ? 0 : 1;

        ViewSet.SetActiveView(uxEdit);
    }

    /// <summary>
    /// Event handler called in response to the user clicking the Save button
    /// </summary>
    protected void SaveButton_Click(object sender, EventArgs e)
    {
        //attempt to read values provided by the user
        //fall back to defaults in case of invalid input

        int days = DAYS;
        int items = ITEMS;
        bool published = PUBLISHED;

        int.TryParse(uxDaysLimit.Text, out days);
        int.TryParse(uxItemLimit.Text, out items);
        bool.TryParse(uxPublishedRadio.SelectedValue, out published);

        this.DaysLimit = days;
        this.ItemLimit = items;
        this.PublishedOnly = published;

        Host.SaveWidgetDataMembers();
        ViewSet.SetActiveView(View);
    }

    /// <summary>
    /// Event handler called in response to the user clicking the Cancel button
    /// </summary>
    protected void CancelButton_Click(object sender, EventArgs e)
    {
        //just redisplay the widget view
        ViewSet.SetActiveView(View);
    }

    /// <summary>
    /// Gets a collection of the current user's recently edited content items
    /// </summary>
    private IEnumerable<ContentData> loadContentData()
    {
        ContentManager cAPI = new ContentManager();
        UserManager uAPI = new UserManager();

        var me = uAPI.GetItem(cAPI.UserId, true);

        ContentCriteria criteria = new ContentCriteria();
        criteria.OrderByField = ContentProperty.DateModified;
        criteria.AddFilter(ContentProperty.UserId, CriteriaFilterOperator.EqualTo, me.Id);
        criteria.AddFilter(ContentProperty.FolderName, CriteriaFilterOperator.NotEqualTo, "Workspace");
        criteria.AddFilter(ContentProperty.FolderName, CriteriaFilterOperator.NotEqualTo, "_meta_");

        if (this.DaysLimit > 0)
            criteria.AddFilter(ContentProperty.DateModified, CriteriaFilterOperator.GreaterThanOrEqualTo, DateTime.Now.AddDays(-DaysLimit));

        if (this.PublishedOnly)
            criteria.AddFilter(ContentProperty.Status, CriteriaFilterOperator.EqualTo, "A");

        var list = cAPI.GetList(criteria);

        if (this.ItemLimit > 0)
            list = list.Take(this.ItemLimit).ToList();

        return list;
    }

    /// <summary>
    /// Convert a collection of Ektron ContentData into a collection of RecentContentRecord
    /// </summary>
    private IEnumerable<RecentContentRecord> loadWidgetData(IEnumerable<ContentData> contentList)
    {
        var records = new List<RecentContentRecord>();

        string icon;
        string titlePattern = @"{0} <a href=""#"" onclick=""top.showContentInWorkarea('content.aspx?action=ViewStaged&id={1}&callerpage=dashboard.aspx&LangType={2}', 'Content', '{3}')"" title=""View {4}"">{4}</a>";
        string pathPattern = @"<a href=""#"" onclick=""top.showContentInWorkarea('content.aspx?action=ViewContentByCategory&id={0}&LangType={1}', 'Content', '{2}')"">{3}</a>";

        foreach (ContentData item in contentList)
        {
            icon = "";

            try
            {
                icon = item.AssetData.ImageUrl;
            }
            catch (Exception) { }

            string folderCSVPath = EkContentRef.GetFolderParentFolderIdRecursive(item.FolderId);

            var record = new RecentContentRecord() {
                ID = item.Id,
                Title = String.Format(titlePattern, base.GetContentImage(item.ContType, icon), item.Id, item.LanguageId, folderCSVPath, item.Title),
                Editor = String.Format("{0}, {1}", item.EditorLastName, item.EditorFirstName),
                Modified = item.DateModified,
                Path = String.Format(pathPattern, item.FolderId, item.LanguageId, folderCSVPath, item.Path.StartsWith("/") ? item.Path : String.Format("/{0}", item.Path)),
                Status = item.Status
            };

            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// Binds a collection of RecentContentRecord to the UI, allowing the user to navigate to the recently edited content items.
    /// </summary>
    private void bindWidget(IEnumerable<RecentContentRecord> dataSource)
    {
        if (dataSource.Count() > 0)
        {
            DataGrid.DataSource = dataSource;
            DataGrid.DataBind();
            DataGrid.Visible = true;
        }
        else
        {
            NoRecordsLabel.Visible = true;
        }
    }
}