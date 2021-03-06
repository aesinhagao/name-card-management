using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ExcelDataReader;
using NCMSystem.Filter;
using NCMSystem.Models;
using NCMSystem.Models.CallAPI;
using NCMSystem.Models.CallAPI.Admin;
using NCMSystem.Models.CallAPI.Team;
using Newtonsoft.Json;
using Serilog;

namespace NCMSystem.Controllers
{
    [JwtAuthorizeFilter(NcmRoles = new[] { NcmRole.Admin })]
    public class AdminController : ApiController
    {
        private NCMSystemEntities db = new NCMSystemEntities(Environment.GetEnvironmentVariable("NCMSystemEntities"));
        private LogException _log = new LogException();

        [HttpGet]
        [Route("api/admin/team")]
        public ResponseMessageResult GetListMember(int id = 0)
        {
            if (id < 0) id = 0;
            List<TeamResponse> listMember = new List<TeamResponse>();
            try
            {
                var user = id == 0
                    ? db.users.FirstOrDefault(x => x.role_id == 3 && x.isActive == true)
                    : db.users.FirstOrDefault(x => x.id == id);
                int userId = user?.id ?? 0;
                var mem = id == 0
                    ? db.Database.SqlQuery<MemberTeam>("exec user_recurse @SuperBoss_id = " + userId).ToList()
                    : db.Database.SqlQuery<MemberTeam>("exec user_recurse @SuperBoss_id = " + id).ToList();

                listMember.Add(new TeamResponse()
                {
                    Id = user?.id ?? 0,
                    Name = user?.name ?? "",
                    Email = user?.email ?? "",
                    IsActive = user?.isActive ?? false,
                    Role = user?.role_id ?? 0,
                    Children = new List<TeamResponse>()
                });
                foreach (var a in listMember)
                {
                    a.Children = MemberRecursive(mem, userId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listMember
                }), Encoding.UTF8, "application/json")
            });
        }

        public List<TeamResponse> MemberRecursive(List<MemberTeam> mem, int id)
        {
            List<TeamResponse> arrayChild = new List<TeamResponse>();

            foreach (var a in mem)
            {
                if (a.ManagerId == id)
                {
                    arrayChild.Add(new TeamResponse()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Email = a.Email,
                        Role = a.RoleId,
                        IsActive = a.IsActive,
                        Children = new List<TeamResponse>()
                    });
                }
            }

            if (arrayChild.Count != 0)
            {
                foreach (var a in arrayChild)
                {
                    a.Children = MemberRecursive(mem, a.Id);
                }
            }

            return arrayChild;
        }

        [HttpPost]
        [Route("api/admin/import")]
        public ResponseMessageResult ImportData()
        {
            try
            {
                db.import_user.RemoveRange(db.import_user);
                var temp = HttpContext.Current.Request.Files[0];
                if (temp == null || temp.ContentLength == 0)
                {
                    return Common.ResponseMessage.BadRequest("A0001");
                }


                long timeStart = DateTime.Now.Ticks;
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "Files\\staff_" + timeStart + ".xlsx";
                temp.SaveAs(fileName);

                var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
                var reader = ExcelReaderFactory.CreateReader(stream);

                if (reader == null)
                {
                    return Common.ResponseMessage.BadRequest("A0002");
                }

                var boss = db.users.FirstOrDefault(x => x.role_id == 3);

                reader.Read();
                reader.Read();

                while (reader.Read())
                {
                    if (reader.GetValue(1) == null)
                        break;

                    string name = "";
                    string email = "";
                    int role = 0;
                    for (int column = 1; column < 4; column++)
                    {
                        switch (column)
                        {
                            case 1:
                                name = reader.GetString(column);
                                break;
                            case 2:
                                email = reader.GetString(column);
                                break;
                            case 3:
                                switch (reader.GetValue(column))
                                {
                                    case "Staff":
                                        role = 1;
                                        break;
                                    case "Manager":
                                        role = 2;
                                        break;
                                    case "Sale Director":
                                        if (boss != null)
                                        {
                                            return Common.ResponseMessage.BadRequest("A0002");
                                        }

                                        role = 3;
                                        break;
                                }

                                break;
                        }
                    }

                    db.import_user.Add(new import_user()
                    {
                        name = name,
                        email = email,
                        role_id = role,
                        status = 1
                    });
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
                return Common.ResponseMessage.BadRequest("Failed");
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/list-email-manager")]
        public ResponseMessageResult GetListEmailManager()
        {
            var listEmail = new List<EmailManagerResponse>();

            try
            {
                db.users.Where(x => x.role_id == 2).ToList().ForEach(x =>
                {
                    listEmail.Add(new EmailManagerResponse()
                    {
                        Id = x.id,
                        Email = x.email,
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listEmail
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/list-email-user")]
        public ResponseMessageResult GetListEmailActiveUser()
        {
            var listEmail = new List<EmailManagerResponse>();

            try
            {
                db.users.Where(u => u.isActive == true && (u.role_id == 1 || u.role_id == 2)).ToList().ForEach(x =>
                {
                    listEmail.Add(new EmailManagerResponse()
                    {
                        Email = x.email,
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listEmail
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/list-user-da")]
        public ResponseMessageResult GetListDaUser()
        {
            var listUser = new List<UserInformationImportedResponse>();
            try
            {
                var user = db.users.Where(x => x.isActive == false);
                foreach (var u in user)
                {
                    listUser.Add(new UserInformationImportedResponse()
                    {
                        Id = u.id,
                        Name = u.name,
                        Email = u.email,
                        RoleId = u.role_id,
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listUser
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/contacts/list-user-da/{id}")]
        public ResponseMessageResult GetListContactDaUser(int id)
        {
            var listCt = new List<ContactOfDaUserResponse>();
            var user = db.users.FirstOrDefault(x => x.id == id && x.isActive == false);
            if (user == null)
            {
                return Common.ResponseMessage.BadRequest("C0018");
            }

            try
            {
                var ct = db.contacts.Where(x => x.owner_id == user.id && x.createdBy == user.id);
                foreach (var u in ct)
                {
                    listCt.Add(new ContactOfDaUserResponse
                    {
                        Id = u.id,
                        Name = u.name,
                        Company = u.company,
                        IsActive = u.isActive
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listCt
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/user/{id}")]
        public ResponseMessageResult GetUserInformation(int id)
        {
            UserInformationResponse response = new UserInformationResponse();

            try
            {
                var user = db.users.FirstOrDefault(x => x.id == id);
                if (user == null)
                    return Common.ResponseMessage.BadRequest("C0018");

                if (user.role_id == 4)
                    return Common.ResponseMessage.BadRequest("C0021");

                response.UserId = user.id;
                response.Name = user.name;
                response.Email = user.email;
                response.RoleId = user.role_id;
                response.IsActive = user.isActive;
                response.IdManager = user.user2?.id;
                response.EmailManager = user.user2?.name;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = response
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/user-imported")]
        public ResponseMessageResult GetListUserImported()
        {
            var listUser = new List<UserInformationImportedResponse>();

            try
            {
                var list = db.import_user.ToList();
                foreach (var user in list)
                {
                    listUser.Add(new UserInformationImportedResponse()
                    {
                        Id = user.id,
                        Email = user.email,
                        Name = user.name,
                        RoleId = user.role_id,
                        Status = user.status,
                        EmailManager = user.manager,
                        CheckEmail = db.users.FirstOrDefault(x => x.email == user.email) != null,
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = listUser
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpGet]
        [Route("api/admin/user-imported/{id}")]
        public ResponseMessageResult GetUserImportedDetail(int id)
        {
            var selectUser = new UserInformationImportedResponse();

            try
            {
                var user = db.import_user.FirstOrDefault(x => x.id == id);

                if (user == null)
                    return Common.ResponseMessage.BadRequest("C0018");

                selectUser.Id = user.id;
                selectUser.Name = user.name;
                selectUser.Email = user.email;
                selectUser.RoleId = user.role_id;
                selectUser.EmailManager = user.manager;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                    Data = selectUser
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpPut]
        [Route("api/admin/user-imported/{id}")]
        public ResponseMessageResult ChangeUserImported([FromBody] ChangeUserImportedRequest request, int id)
        {
            try
            {
                var boss = db.users.FirstOrDefault(x => x.role_id == 3);
                if (request == null)
                    return Common.ResponseMessage.BadRequest("A0004");

                if (request.Name == null || request.Email == null || request.Manager == null)
                    return Common.ResponseMessage.BadRequest("A0004");

                if (request.RoleId == 3 && boss != null)
                    return Common.ResponseMessage.Good("A0007");

                if (request.Name.Trim() == "" || request.Email.Trim() == "" || request.Manager.Trim() == "")
                    return Common.ResponseMessage.BadRequest("A0004");

                if (!Validator.Validator.CheckName(request.Name.Trim()) ||
                    !Validator.Validator.CheckEmail(request.Email.Trim()) ||
                    !Validator.Validator.CheckEmail(request.Manager.Trim()))
                    return Common.ResponseMessage.BadRequest("A0004");

                var selectUserByEmail = db.users.FirstOrDefault(x => x.email == request.Email);
                if (selectUserByEmail != null)
                    return Common.ResponseMessage.BadRequest("A0005");

                var selectUserByEmailManager = db.users.FirstOrDefault(x => x.email == request.Manager);
                if (selectUserByEmailManager == null)
                    return Common.ResponseMessage.BadRequest("A0005");

                var selectUserImported = db.import_user.FirstOrDefault(x => x.id == id);
                if (selectUserImported == null)
                    return Common.ResponseMessage.BadRequest("A0006");

                selectUserImported.name = request.Name;
                selectUserImported.email = request.Email;
                selectUserImported.role_id = request.RoleId;
                selectUserImported.manager = request.Manager;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                }), Encoding.UTF8, "application/json")
            });
        }

        [HttpPost]
        [Route("api/admin/user-imported/{id}")]
        public ResponseMessageResult AddUser(int id)
        {
            try
            {
                var selectUserImported = db.import_user.FirstOrDefault(x => x.id == id);
                if (selectUserImported == null)
                    return Common.ResponseMessage.BadRequest("A0006");

                if (selectUserImported.manager == null && selectUserImported.role_id < 3)
                    return Common.ResponseMessage.BadRequest("A0005");

                var selectUser = db.users.FirstOrDefault(x => x.email == selectUserImported.email);
                if (selectUser != null)
                    return Common.ResponseMessage.BadRequest("A0008");

                var selectUserManager = db.users.FirstOrDefault(x => x.email == selectUserImported.manager);
                if (selectUserManager == null && (selectUserImported.role_id == 1 || selectUserImported.role_id == 2))
                    return Common.ResponseMessage.BadRequest("A0005");

                var boss = db.users.FirstOrDefault(x => x.role_id == 3);
                if (boss != null && (selectUserImported.manager == null || selectUserImported.manager.Trim() == ""))
                {
                    var user = db.users.Add(new user()
                    {
                        name = selectUserImported.name,
                        password = PasswordGenerator.Generate(),
                        email = selectUserImported.email,
                        role_id = selectUserImported.role_id ?? 1,
                        isActive = true,
                        manager_id = null
                    });

                    var listUser = db.users.Where(x => x.manager_id == boss.id).ToList();
                    if (listUser.Count > 0)
                    {
                        foreach (var u in listUser)
                        {
                            u.manager_id = user.id;
                        }
                    }

                    boss.isActive = false;
                    selectUserImported.status = 2;
                    db.SaveChanges();
                    return new ResponseMessageResult(new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                        {
                            Message = "Success",
                        }), Encoding.UTF8, "application/json")
                    });
                }

                if (selectUserImported.role_id > 3)
                    return Common.ResponseMessage.BadRequest("A0009");

                db.users.Add(new user()
                {
                    name = selectUserImported.name,
                    password = PasswordGenerator.Generate(),
                    email = selectUserImported.email,
                    role_id = selectUserImported.role_id ?? 1,
                    isActive = true,
                    manager_id = selectUserManager?.id
                });
                
                selectUserImported.status = 2;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "C0001");
                Log.CloseAndFlush();
                return Common.ResponseMessage.BadRequest("C0001");
            }

            return new ResponseMessageResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CommonResponse()
                {
                    Message = "Success",
                }), Encoding.UTF8, "application/json")
            });
        }

        private static class PasswordGenerator
        {
            private static readonly Random Rand = new Random();

            public static string Generate()
            {
                string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string lowerChars = "abcdefghijklmnopqrstuvwxyz";
                string specialChars = "!@#$%^&*+?~";
                string numbers = "0123456789";
                string password = "";
                //random 1 char in upperChars
                password += upperChars[Rand.Next(0, upperChars.Length)];
                password += specialChars[Rand.Next(0, specialChars.Length)];
                password += numbers[Rand.Next(0, numbers.Length)];
                for (int i = 0; i < 5; i++)
                {
                    password += lowerChars[Rand.Next(0, lowerChars.Length)];
                }

                return password;
            }
        }
    }
}