// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Contoso.FraudProtection.ApplicationCore.Entities.FraudProtectionApiModels;
using Contoso.FraudProtection.ApplicationCore.Interfaces;
using Contoso.FraudProtection.Infrastructure.Identity;
using Contoso.FraudProtection.Web.Extensions;
using Contoso.FraudProtection.Web.ViewModels;
using Contoso.FraudProtection.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Dynamics.FraudProtection.Models;
using Microsoft.Dynamics.FraudProtection.Models.SignupEvent;
using Microsoft.Dynamics.FraudProtection.Models.SignupStatusEvent;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Contoso.FraudProtection.Web.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBasketService _basketService;
        private readonly IFraudProtectionService _fraudProtectionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IBasketService basketService,
            IFraudProtectionService fraudProtectionService,
            IHttpContextAccessor contextAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _basketService = basketService;
            _fraudProtectionService = fraudProtectionService;
            _contextAccessor = contextAccessor;
            _passwordHasher = passwordHasher;

        }

        // GET: /Account/SignIn 
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var model = new LoginViewModel
            {
                DeviceFingerPrinting = new DeviceFingerPrintingModel
                {
                    SessionId = _contextAccessor.GetSessionId()
                }
            };
            ViewData["ReturnUrl"] = returnUrl;
            if (!String.IsNullOrEmpty(returnUrl) &&
                returnUrl.IndexOf("checkout", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ViewData["ReturnUrl"] = "/Basket/Index";
            }

            return View(model);
        }

        // POST: /Account/SignIn
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ViewData["ReturnUrl"] = returnUrl;

            ApplicationUser applicationUser = new ApplicationUser()
            {
                UserName = model.Email
            };
            var passwordHash = _passwordHasher.HashPassword(applicationUser, model.Password);
            SignInRequest req = new SignInRequest()
            {
                SignInId = model.Email,
                PasswordHash = passwordHash,
                MerchantLocalDate = new DateTime(),
                UserId = model.Email

            };

            var signInAssessmentResponse = await _fraudProtectionService.PostSignIn(req, _fraudProtectionService.NewCorrelationId);
            if (signInAssessmentResponse != null && signInAssessmentResponse.ResultDetails["MerchantRuleDecision"].Equals("Approve"))
            { 
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await TransferBasketToEmailAsync(model.Email);
                    return RedirectToLocal(returnUrl);
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(CatalogController.Index), "Catalog");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                DeviceFingerPrinting = new DeviceFingerPrintingModel
                {
                    SessionId = _contextAccessor.GetSessionId()
                }
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_contextAccessor.HttpContext.Connection == null)
                throw new Exception(nameof(_contextAccessor.HttpContext.Connection));

            #region Fraud Protection Service
            // Ask Fraud Protection to assess this signup/registration before registering the user in our database, etc.
            var signupAddress = new AddressDetails
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.Phone,
                Street1 = model.Address1,
                Street2 = model.Address2,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Country = model.CountryRegion
            };

            var signupUser = new SignupUser
            {
                CreationDate = DateTimeOffset.Now,
                UpdateDate = DateTimeOffset.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Country = model.CountryRegion,
                ZipCode = model.ZipCode,
                TimeZone = new TimeSpan(0, 0, -model.ClientTimeZone, 0).ToString(),
                Language = "EN-US",
                PhoneNumber = model.Phone,
                Email = model.Email,
                ProfileType = UserProfileType.Consumer.ToString(),
                Address = signupAddress
            };

            var deviceContext = new DeviceContext
            {
                DeviceContextId = _contextAccessor.GetSessionId(),
                IPAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                DeviceContextDC = model.DeviceFingerPrinting.FingerPrintingDC,
                Provider = DeviceContextProvider.DFPFingerPrinting.ToString(),
            };

            var marketingContext = new MarketingContext
            {
                Type = MarketingType.Direct.ToString(),
                IncentiveType = MarketingIncentiveType.None.ToString(),
                IncentiveOffer = "Integrate with Fraud Protection"
            };

            var storefrontContext = new StoreFrontContext
            {
                StoreName = "Fraud Protection Sample Site",
                Type = StorefrontType.Web.ToString(),
                Market = "US"
            };

            var correlationId = _fraudProtectionService.NewCorrelationId;

            var signupEvent = new SignUp
            {
                SignUpId = Guid.NewGuid().ToString(),
                AssessmentType = AssessmentType.Protect.ToString(),
                User = signupUser,
                MerchantLocalDate = DateTimeOffset.Now,
                CustomerLocalDate = model.ClientDate,
                MarketingContext = marketingContext,
                StoreFrontContext = storefrontContext,
                DeviceContext = deviceContext,
            };

            var signupAssessment = await _fraudProtectionService.PostSignup(signupEvent, correlationId);

            //Track Fraud Protection request/response for display only
            var fraudProtectionIO = new FraudProtectionIOModel(signupEvent, signupAssessment, "Signup");

            //2 out of 3 signups will succeed on average. Adjust if you want more or less signups blocked for tesing purposes.
            var random = new Random();
            var rejectSignup = random.NextDouble() >= 2.0 / 3;
            var signupStatusType = rejectSignup ? SignupStatusType.Rejected.ToString() : SignupStatusType.Approved.ToString();

            var signupStatus = new SignupStatusEvent
            {
                SignUpId = signupEvent.SignUpId,
                StatusType = signupStatusType,
                StatusDate = DateTimeOffset.Now,
                Reason = "User is " + signupStatusType
            };

            if (!rejectSignup)
            {
                signupStatus.User = new SignupStatusUser { UserId = model.Email };
            }

            var signupStatusResponse = await _fraudProtectionService.PostSignupStatus(signupStatus, correlationId);

            fraudProtectionIO.Add(signupStatus, signupStatusResponse, "Signup Status");

            TempData.Put(FraudProtectionIOModel.TempDataKey, fraudProtectionIO);
            #endregion

            if (rejectSignup)
            {
                ModelState.AddModelError("", "Signup rejected by Fraud Protection. You can try again as it has a random likelyhood of happening in this sample site.");
                return View(model);
            }

            //Only create the user in the sample site if the Fraud Protection merchant decision is APPROVE
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.Phone,
                Address1 = model.Address1,
                Address2 = model.Address2,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                CountryRegion = model.CountryRegion
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            if (user == null)
                throw new Exception(nameof(user));

            await _signInManager.SignInAsync(user, isPersistent: false);

            await TransferBasketToEmailAsync(user.Email);

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(CatalogController.Index), "Catalog");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl)
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(CatalogController.Index), "Catalog");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private async Task TransferBasketToEmailAsync(string email)
        {
            string anonymousBasketId = Request.Cookies[Constants.BASKET_COOKIENAME];
            if (!string.IsNullOrEmpty(anonymousBasketId))
            {
                await _basketService.TransferBasketAsync(anonymousBasketId, email);
                Response.Cookies.Delete(Constants.BASKET_COOKIENAME);
            }
        }
    }
}
