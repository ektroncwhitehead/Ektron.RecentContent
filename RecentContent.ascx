<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecentContent.ascx.cs" Inherits="RecentContentWidget" %>

<div style="overflow: hidden;">
    <asp:MultiView ID="ViewSet" runat="server" ActiveViewIndex="0">

        <asp:View ID="View" runat="server">

            <asp:Label ToolTip="No Records" ID="NoRecordsLabel" Visible="false" runat="server">No Records Found</asp:Label>
                
            <div class="ektronPageGrid" style="overflow:auto;">
                <asp:DataGrid ID="DataGrid" runat="server" Width="100%" Visible="false" AutoGenerateColumns="False" EnableViewState="False" GridLines="None" CssClass="ektronGrid ektronBorder" style="display: table">
                    <HeaderStyle CssClass="title-header" />
                    <Columns>
                        <asp:BoundColumn DataField="Title" HeaderText="Title (click to view content)" />
                        <asp:BoundColumn DataField="ID" HeaderText="ID" />
                        <asp:BoundColumn DataField="Status" HeaderText="Status" />
                        <asp:BoundColumn DataField="Modified" HeaderText="Date Modified" />
                        <asp:BoundColumn DataField="Path" HeaderText="Path (click to view folder contents)" />
                    </Columns>
                </asp:DataGrid>
            </div>
        </asp:View>

        <asp:View ID="uxEdit" runat="server">
            
            <div id="<%=ClientID%>_edit">
                <br />

                <strong>How many days prior to today?</strong>
                <asp:TextBox ToolTip="Days Limit" ID="uxDaysLimit" runat="server" Style="width: 40%"></asp:TextBox>
                <br />(0 for all time)
                
                <br />                
                <br />
                
                <strong>Maximum number of items to return?</strong>
                <asp:TextBox ToolTip="Item Limit" ID="uxItemLimit" runat="server" Style="width: 40%"> </asp:TextBox>
                <br />(0 to remove the maximum)
                
                <br />
                <br />

                <strong>Only view published content?</strong>
                <asp:RadioButtonList ID="uxPublishedRadio" runat="server">
                    <asp:ListItem Text="Yes" Value="true" />
                    <asp:ListItem Text="No" Value="false" Selected="true" />
                </asp:RadioButtonList>

                <br />

                <asp:Button  ID="uxCancelButton" runat="server" Text="Cancel" ToolTip="Cancel" OnClick="CancelButton_Click" />
                &nbsp;&nbsp; 
                <asp:Button  ID="uxSaveButton" runat="server" Text="Save" ToolTip="Save" OnClick="SaveButton_Click" />                
            </div>

        </asp:View>

    </asp:MultiView>
</div>