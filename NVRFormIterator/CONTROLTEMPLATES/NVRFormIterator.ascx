<%@ Control Language="C#" AutoEventWireup="false" %>
<%@Assembly Name="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@Register TagPrefix="SharePoint" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" namespace="Microsoft.SharePoint.WebControls"%>
<%@Register TagPrefix="ApplicationPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" namespace="Microsoft.SharePoint.ApplicationPages.WebControls"%>
<%@Register TagPrefix="SPHttpUtility" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" namespace="Microsoft.SharePoint.Utilities"%>
<%@ Register TagPrefix="wssuc" TagName="ToolBar" src="~/_controltemplates/15/ToolBar.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ToolBarButton" src="~/_controltemplates/15/ToolBarButton.ascx" %>
<%@ Register TagPrefix="SPTools2013" Assembly="$SharePoint.Project.AssemblyFullName$" namespace="Navertica.SharePoint.WebControls"%>

<!-- NVR FORMITERATOR page additions start -->
<link href="/_layouts/SPTools2013/styles.css" rel="Stylesheet" type="text/css" />
<asp:Literal runat="server" ID="FormIteratorLiteral" Text="TEST FORMITERATOR JFL"></asp:Literal>

<!-- NVR FORMITERATOR END -->

<SharePoint:RenderingTemplate id="GenericForm" runat="server">
	<Template>
		<table class="ms-formtable" border="0" cellpadding="2">
			<SPTools2013:FormIterator2013 runat="server"/>
		</table>
		<SharePoint:RequiredFieldMessage runat="server"/>
	</Template>
</SharePoint:RenderingTemplate>

<SharePoint:RenderingTemplate ID="GenericLibraryForm" runat="server">
	<Template>
			<SPTools2013:FormIterator2013 runat="server"/>
	</Template>
</SharePoint:RenderingTemplate>

<SharePoint:RenderingTemplate id="TaskForm" runat="server">
	<Template>
		<table>
			<tr>
				<td style="height:350px; vertical-align:top">
					<span id='part1'>
						<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
							<SharePoint:InformationBar runat="server"/>
							<div id="listFormToolBarTop">
								<wssuc:ToolBar CssClass="ms-formtoolbar" id="toolBarTbltop" RightButtonSeparator="&amp;#160;" runat="server">
										<Template_RightButtons>
											<SharePoint:NextPageButton runat="server"/>
											<SharePoint:SaveButton runat="server"/>
											<SharePoint:GoBackButton runat="server"/>
										</Template_RightButtons>
								</wssuc:ToolBar>
							</div>
							<SharePoint:FormToolBar runat="server"/>
						</SharePoint:EditDatesSelector>
						<SharePoint:ItemValidationFailedMessage runat="server"/>
						<SharePoint:EditDatesSelector RenderInEditDatesMode="true" runat="server">
							<div><SharePoint:EncodedLiteral runat="server" text="<%$Resources:wss,BeautifulTimeline_HelperText%>" EncodeMethod='HtmlEncode'/></div>
						</SharePoint:EditDatesSelector>
						<table class="ms-formtable" style="margin-top: 8px;" border="0" cellpadding="0" cellspacing="0" width="100%">
							<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
								<SharePoint:ChangeContentType runat="server"/>
								<SharePoint:FolderFormFields runat="server"/>
								<SPTools2013:FormIterator2013 runat="server"/>
								<SharePoint:ApprovalStatus runat="server"/>
								<SharePoint:FormComponent TemplateName="AttachmentRows" ComponentRequiresPostback="false" runat="server"/>
							</SharePoint:EditDatesSelector>
							<SharePoint:EditDatesSelector RenderInEditDatesMode="true" runat="server">
								<SharePoint:SpecifiedListFieldIterator ShownFields="StartDate;#DueDate" runat="server"/>
							</SharePoint:EditDatesSelector>
						</table>
						<table cellpadding="0" cellspacing="0" width="100%" style="padding-top: 7px"><tr><td width="100%">
							<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
								<SharePoint:ItemHiddenVersion runat="server"/>
								<SharePoint:ParentInformationField runat="server"/>
								<SharePoint:InitContentType runat="server"/>
							</SharePoint:EditDatesSelector>
							<wssuc:ToolBar CssClass="ms-formtoolbar" id="toolBarTbl" RightButtonSeparator="&amp;#160;" runat="server">
								<Template_Buttons>
									<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
										<SharePoint:CreatedModifiedInfo runat="server"/>
									</SharePoint:EditDatesSelector>
								</Template_Buttons>
								<Template_RightButtons>
									<SharePoint:SaveButton runat="server"/>
									<SharePoint:GoBackButton runat="server"/>
								</Template_RightButtons>
						</wssuc:ToolBar>
						</td></tr></table>
					</span>
				</td>
				<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
					<td valign="top">
						<SharePoint:DelegateControl runat="server" ControlId="RelatedItemsPlaceHolder"/>
					</td>
				</SharePoint:EditDatesSelector>
			</tr>
		</table>
		<SharePoint:EditDatesSelector RenderInEditDatesMode="false" runat="server">
			<SharePoint:AttachmentUpload runat="server"/>
		</SharePoint:EditDatesSelector>
	</Template>
</SharePoint:RenderingTemplate>

<SharePoint:RenderingTemplate id="ListForm" runat="server">
	<Template>
		<table>
			<tr>
				<td>
					<span id='part1'>
						<SharePoint:InformationBar runat="server"/>
						<div id="listFormToolBarTop">
						<wssuc:ToolBar CssClass="ms-formtoolbar" id="toolBarTbltop" RightButtonSeparator="&amp;#160;" runat="server">
								<Template_RightButtons>
									<SharePoint:NextPageButton runat="server"/>
									<SharePoint:SaveButton runat="server"/>
									<SharePoint:GoBackButton runat="server"/>
								</Template_RightButtons>
						</wssuc:ToolBar>
						</div>
						<SharePoint:FormToolBar runat="server"/>
						<SharePoint:ItemValidationFailedMessage runat="server"/>
						<table class="ms-formtable" style="margin-top: 8px;" border="0" cellpadding="0" cellspacing="0" width="100%">
						<SharePoint:ChangeContentType runat="server"/>
						<SharePoint:FolderFormFields runat="server"/>
                        <SPTools2013:FormIterator2013 runat="server"/>
						<SharePoint:ApprovalStatus runat="server"/>
						<SharePoint:FormComponent TemplateName="AttachmentRows" ComponentRequiresPostback="false" runat="server"/>
						</table>
						<table cellpadding="0" cellspacing="0" width="100%" style="padding-top: 7px"><tr><td width="100%">
						<SharePoint:ItemHiddenVersion runat="server"/>
						<SharePoint:ParentInformationField runat="server"/>
						<SharePoint:InitContentType runat="server"/>
						<wssuc:ToolBar CssClass="ms-formtoolbar" id="toolBarTbl" RightButtonSeparator="&amp;#160;" runat="server">
								<Template_Buttons>
									<SharePoint:CreatedModifiedInfo runat="server"/>
								</Template_Buttons>
								<Template_RightButtons>
									<SharePoint:SaveButton runat="server"/>
									<SharePoint:GoBackButton runat="server"/>
								</Template_RightButtons>
						</wssuc:ToolBar>
						</td></tr></table>
					</span>
				</td>
				<td valign="top">
					<SharePoint:DelegateControl runat="server" ControlId="RelatedItemsPlaceHolder"/>
				</td>
			</tr>
		</table>
		<SharePoint:AttachmentUpload runat="server"/>
	</Template>
</SharePoint:RenderingTemplate>