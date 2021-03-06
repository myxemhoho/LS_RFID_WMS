﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Invengo.RiceManangeServices.Utility;
using Invengo.RiceManangeServices.Model;

public partial class UserManage_User_UserList : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["IsFirstSearch"] = true;
            LoadData(1);
            ViewState["IsFirstSearch"] = false;
        }
    }

    private void LoadData(int page)
    {
        int recordNum = 100;
        int pageSize = Variable.DefaultPageSize;
        string retValue = string.Empty;
       
        bool isFirstSearch = false;
        if (ViewState["IsFirstSearch"] != null)
            isFirstSearch = bool.Parse(ViewState["IsFirstSearch"].ToString());

        UserRoleMenu.QueryUserList(GetQueryUserInfo(), pageSize, page, null, isFirstSearch, out recordNum, out retValue);
        GridView1.DataSource = Helper.ConvertXmlStrToDataTable(retValue);
        GridView1.DataBind();
        //因为查询之后第二点击下一页不需要统计总数，只需要取之前的就可以
        if (bool.Parse(ViewState["IsFirstSearch"].ToString()) == false)
        {
            recordNum = int.Parse(ViewState["RecordNum"].ToString());
        }
        else
        {
            ViewState["RecordNum"] = recordNum;
        }

        lblRecordNum.Text = recordNum.ToString();
        lblPresentPageNum.Text = page.ToString();
        lblTotalPageNum.Text = (Math.Ceiling(Convert.ToDouble(recordNum) / pageSize)).ToString();

        WSDataLayer.EnablePageControl(lbtFirstPage, lbtPreviousPage, lbtNextPage, lbtLastPage, recordNum, pageSize, Convert.ToInt32(lblTotalPageNum.Text), page);
    }

    private UserInfo GetQueryUserInfo()
    {
        UserInfo qrUserInfo = new UserInfo();

        if (!string.IsNullOrEmpty(this.txtJobNo.Text))
        {
            qrUserInfo.JobNo = txtJobNo.Text;
        }
        return qrUserInfo;
    }

    protected void ChangePage_Click(object sender, CommandEventArgs e)
    {
        string arg = e.CommandArgument.ToString();
        int page = (Convert.ToInt32(lblPresentPageNum.Text));
        switch (arg)
        {
            case "first":
                page = 1;
                break;
            case "previous":
                page -= 1;
                break;
            case "next":
                page += 1;
                break;
            case "last":
                page = (Convert.ToInt32(lblTotalPageNum.Text));
                break;
            default:
                page = 1;
                break;
        }
        LoadData(page);
    }

    protected void btJump_Click(object sender, EventArgs e)
    {
        string page = txtPageNum.Text.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(page, "^\\d+$"))
        {
            ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "DataError", "alert('输入页数必须为正整数！')", true);
            return;
        }
        if (Convert.ToInt32(page) < 1 || Convert.ToInt32(page) > Convert.ToInt32(lblTotalPageNum.Text.Trim()))
        {
            ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "DataError", "alert('输入页数不在范围之内！')", true);
            return;
        }
        LoadData(Convert.ToInt32(page));
        txtPageNum.Text = string.Empty;
    }

    protected void GridView1_OnRowCommand(object sender, GridViewCommandEventArgs e)
    {
        GridViewRow gRow = (GridViewRow)((Control)e.CommandSource).Parent.Parent;
        string userId = GridView1.DataKeys[gRow.RowIndex].Value.ToString();        
        if (e.CommandName == "Edit")
        {
            Response.Redirect("UserEdit.aspx?UserId=" + userId + "&Type=1");
        }
        if (e.CommandName == "Del")
        {
            if (UserManage.DeleteUser(int.Parse(userId)))
            {
                ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "DeleteSucess", "alert('删除成功！');", true);
                LoadData(1);
            }
            else
            {
                ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "DeleteError", "alert('删除失败！');", true);
            }
        }
        if (e.CommandName == "ResumePassword")
        {
            UserInfo userInfo = new UserInfo();
            string beginPassword = GridView1.Rows[gRow.RowIndex].Cells[1].Text;
            userInfo.JobNo=GridView1.Rows[gRow.RowIndex].Cells[1].Text;
            userInfo.Password = SHA1.GetSHA1Password(beginPassword);
            if (UserManage.ChangePassword(userInfo))
            {
                ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "ResumeSucess", "alert('恢复密码成功！');", true);
                LoadData(1);
            }
            else
            {
                ScriptManager.RegisterStartupScript(UpdatePanel1, this.GetType(), "ResumeError", "alert('恢复密码失败！');", true);
            }
        }
    }

    protected void GridView1_OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Attributes.Add("onmouseover", "e=this.style.backgroundColor;this.style.backgroundColor='" + OnMouseBackColor + "';");
            e.Row.Attributes.Add("onmouseout", "this.style.backgroundColor=e;");
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        ViewState["IsFirstSearch"] = true;
        LoadData(1);
        ViewState["IsFirstSearch"] = false;
    }
}
