﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Security;

namespace AbidWebApplication1.AbidUsers
{
    public partial class abidChangePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;
                userName.Text = username;
            } 

            if (Request.QueryString["uid"] == null && User.Identity.Name == "")
            {
                Response.Redirect("../AbidUsers/MyAccount.aspx");
            }
            if (!IsPostBack)
            {
                if (Request.QueryString["uid"] != null)
                {
                    if (!IsPasswordResetLinkValid())
                    {
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Text = "Password Reset link has expired or is invalid";
                    }
                    trCurrentPassword.Visible = false;
                }
                else if (User.Identity.Name != "")
                {
                    trCurrentPassword.Visible = true;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if ((Request.QueryString["uid"] != null && ChangeUserPassword()) ||
                    (User.Identity.Name != "" && ChangeUserPasswordUsingCurrentPassword()))
            {
                lblMessage.Text = "Password Changed Successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                if (trCurrentPassword.Visible)
                {
                    lblMessage.Text = "Invalid Current Password!";
                }
                else
                {
                    lblMessage.Text = "Password Reset link has expired or is invalid";
                }
            }
        }


        private bool ChangeUserPassword()
        {
            List<SqlParameter> paramList = new List<SqlParameter>()
            {
                new SqlParameter()
                {
                    ParameterName = "@GUID",
                    Value = Request.QueryString["uid"]
                },
                new SqlParameter()
                {
                    ParameterName = "@Password",
                    Value = FormsAuthentication.HashPasswordForStoringInConfigFile(txtNewPassword.Text, "SHA1")
                }
            };

            return ExecuteSP("spChangePassword", paramList);
        }



        private bool IsPasswordResetLinkValid()
        {
            List<SqlParameter> paramList = new List<SqlParameter>()
                {
                    new SqlParameter()
                    {
                        ParameterName = "@GUID",
                        Value = Request.QueryString["uid"]
                    }
                };

            return ExecuteSP("spIsPasswordResetLinkValid", paramList);
        }


        private bool ExecuteSP(string SPName, List<SqlParameter> SPParameters)
        {
            string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(SPName, con);
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter parameter in SPParameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                con.Open();
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }


        private bool ChangeUserPasswordUsingCurrentPassword()
        {
            List<SqlParameter> paramList = new List<SqlParameter>()
            {
                new SqlParameter()
                {
                    ParameterName = "@UserName",
                    Value = User.Identity.Name
                },
                new SqlParameter()
                {
                    ParameterName = "@CurrentPassword",
                    Value = FormsAuthentication.HashPasswordForStoringInConfigFile(txtCurrentPassword.Text, "SHA1")
                },
                new SqlParameter()
                {
                    ParameterName = "@NewPassword",
                    Value = FormsAuthentication.HashPasswordForStoringInConfigFile(txtNewPassword.Text, "SHA1")
                }
            };

            return ExecuteSP("spChangePasswordUsingCurrentPassword", paramList);
        } 
    }
}