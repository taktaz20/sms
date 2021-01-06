using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kavenegar;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TezAl.Data.Repozitory;
using TezAl.Entities;

namespace TezAl.Controllers
{
	[ApiExplorerSettings(IgnoreApi = true)]
    public class LoginController : Controller
    {

        private readonly IUnitOfWork db;
        public LoginController(IUnitOfWork _db)
        {
            db = _db;
        }



        [HttpGet]
        public bool Register(string Mobile)
        {
            if (Mobile != null)
            {
                Random rnd = new Random();
                string code = rnd.Next(1000, 9999).ToString();

                var query = db.Rep_Phone.FindSingleAsync(a => a.Phone_Mobile == Mobile).Result;
                if (query == null)
                {
                    Tbl_Phone t = new Tbl_Phone
                    {
                        Phone_Mobile = Mobile,
                        Phone_Code = code, //code
                    };
                    db.Rep_Phone.CreateAsync(t);
                    db.Save();
                }
                else
                {
                    query.Phone_Code = code;
                    db.Rep_Phone.Update(query);
                    db.Save();
                }

                // Kavenegar

                var api = new KavenegarApi("3871353043697339486A70384F544A4A574C74612B51432F4C7A4B305076645457396F5267456F7A5A34383D");
                api.VerifyLookup(Mobile, code, "tezalmarket");
                return true;

                // Kavenegar
            }
            else
                return false;
        }




        [HttpPost]
        public async Task<bool> verification(string code, string Mobile)
        {
            var qcheck = db.Rep_Phone.FindByConditionAsync(a => a.Phone_Mobile == Mobile && a.Phone_Code == code).Result.Any();
            if (qcheck)
            {
                var qcustomer = await db.Rep_Customer.FindSingleAsync(a => a.Customer_Phone == Mobile);
                if (qcustomer != null)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim (ClaimTypes.Name, qcustomer.Customer_Name + " " + qcustomer.Customer_Family),
                        new Claim (ClaimTypes.MobilePhone, qcustomer.Customer_Phone),
                        new Claim (ClaimTypes.NameIdentifier, qcustomer.Customer_ID),
                        new Claim (ClaimTypes.StreetAddress, qcustomer.Customer_Address),
                        new Claim (ClaimTypes.Role, qcustomer.Customer_RolID.ToString()),
                    };
                    var identity1 = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var prinpal1 = new ClaimsPrincipal(identity1);
                    var properties1 = new AuthenticationProperties
                    {
                        
                      IsPersistent = true,
                    };
                    await HttpContext.SignInAsync(prinpal1, properties1);
                }
                else
                {
                    var claim = new List<Claim>()
                    {
                        new Claim (ClaimTypes.MobilePhone, Mobile),
                    };

                    var identity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
                    var prinpal = new ClaimsPrincipal(identity);
                    var properties = new AuthenticationProperties();
                    await HttpContext.SignInAsync(prinpal, properties);
                }
                return true;
            }
            else
                return false;
        }


        //var sender = "1000596446";
        //var receptor = Mobile;
        //var message = $"کد امنیتی شما:{code} ";
        //var api = new KavenegarApi("456371744964687A625572714C4E5233444A2F386C676E3266624A4C594C323739653563324743303057453D");
        //api.Send(sender, receptor, message);




    }//
}