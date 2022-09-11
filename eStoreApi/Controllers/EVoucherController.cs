using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eStoreApi.Data;
using eStoreApi.Model;
using eStoreApi.Util;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using RestSharp.Authenticators;

namespace eStoreApi.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class EVoucherController : ControllerBase
    {
        private readonly eVoucherContext _context;
        private static IConfiguration _configuration;

        public EVoucherController(eVoucherContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TblEvoucher>>> GetAllEVoucher()
        {
            if (_context.TblEvouchers == null)
            {
                return NotFound();
            }
            return await _context.TblEvouchers.ToListAsync();
        }

        [Authorize]
        [HttpGet("GetEVoucher")]
        public async Task<ActionResult<TblEvoucher>> GetEVoucher(eVoucherGetModel eVoucher)
        {
            if (_context.TblEvouchers == null)
            {
                return NotFound();
            }
            var tblEvoucher = await _context.TblEvouchers.FindAsync(eVoucher.Id);

            if (tblEvoucher == null)
            {
                return NotFound();
            }

            return tblEvoucher;
        }

        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken(string userName, string password)
        {
            var tokenUser = (from o in _context.TblTokenusers
                             where
                             o.User == userName && o.Password == password
                             select o).FirstOrDefault();
            if (tokenUser != null)
            {
                var issuer = _configuration.GetSection("Jwt").GetValue<string>("Issuer");
                var audience = _configuration.GetSection("Jwt").GetValue<string>("Audience");
                var key = Encoding.ASCII.GetBytes
                (_configuration.GetSection("Jwt").GetValue<string>("Key"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {

                    Expires = DateTime.UtcNow.AddDays(_configuration.GetSection("Jwt").GetValue<int>("ExpireDay")),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return CreatedAtAction("GetToken", new { }, stringToken);
            }
            return NoContent();
        }

        [Authorize]
        [HttpPost("CheckOut")]
        public async Task<ActionResult<TblEvoucher>> CheckOutEVoucher(eVoucherCheckOutModel eVoucher)
        {
            //Check credit card
            var card = CreditCardInfo.GetCardType(eVoucher.CreditCard);
            if (card == 0)
            {
                return Problem("Invalid Credit Card! Please Try Again!");
            }
            else if (card != CreditCardInfo.CardType.VISA && card != CreditCardInfo.CardType.MasterCard)
            {
                return Problem("Payment Method Not Supported! Please Use VISA or MasterCard");
            }

            if (_context.TblEvouchers == null)
            {
                return Problem("There is no avaliable vouchers.");
            }

            // get unused vouchers purchased by user
            TblEvoucher voucher;
            var purchasedVouchers = (from o in _context.TblEvouchers
                                     where o.Phone == eVoucher.Phone && o.IsUsed == false && o.Active == true
                                     select o).Count();
            var voucherCount = 0;
            voucherCount = eVoucher.IsGift ? _configuration.GetSection("VoucherSetting").GetValue<int>("MaximumGift") : _configuration.GetSection("VoucherSetting").GetValue<int>("MaximumVoucher");

            if (purchasedVouchers < voucherCount) //check maximun count
            {
                try
                {
                    voucher = await _context.TblEvouchers.FindAsync(eVoucher.Id);
                    if (voucher != null)
                    {
                        //check voucher Exp date
                        if (voucher.ExpiryDate > DateTime.Now)
                        {
                            return Problem("E-Voucher has expired!");
                        }
                        //calculate discount if get payment method discount
                        double discount = 0.0;
                        if (voucher.PaymentType == card.ToString())
                        {
                            discount = (double)(voucher.Price * voucher.PaymentDiscount * 0.01);
                        }
                        //check promo code
                        TblPurchasehistory promoResult = (from o in _context.TblPurchasehistories
                                                          where
                                                          o.PromoCodes == eVoucher.PromoCode && o.IsUsed == false
                                                          select o).FirstOrDefault();
                        if (promoResult != null)
                        {
                            promoResult.IsUsed = true;
                            discount = discount + (voucher.Price * promoResult.PromoAmount * 0.01);
                            _context.Entry(promoResult).State = EntityState.Modified;
                        }
                        //price after discounts
                        double finalPrice = voucher.Price - (discount);

                        //add user data to voucher
                        voucher.Name = eVoucher.UserName;
                        voucher.Phone = eVoucher.Phone;
                        voucher.MidifiedDate = DateTime.Now;
                        _context.Entry(voucher).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        //update purchase histroy
                        TblPurchasehistory history = new TblPurchasehistory()
                        {
                            PurchaseId = Guid.NewGuid().ToString(),
                            UserName = voucher.Name,
                            Phone = voucher.Phone,
                            CreatedDate = DateTime.Now,
                            EvoucherId = voucher.Id,

                        };
                        _context.TblPurchasehistories.Add(history);
                        await _context.SaveChangesAsync();
                        GeneratePromoCodes(new GeneratePromoModel() { Id = history.PurchaseId, Phone = history.Phone, PurchaseCount = purchasedVouchers + 1 });
                    }
                    else
                    {
                        return Problem("E-Voucher not found!");
                    }

                }
                catch (Exception ex)
                {
                    return Problem("Error Occured " + ex.Message);
                }
            }
            else
            {
                return Problem("Customer has reached voucher limit!");
            }
            return CreatedAtAction("CheckOutEVoucher", new { phone = eVoucher.Phone }, Convert.ToBase64String(voucher.Qrimage));

        }

        public static async void GeneratePromoCodes(GeneratePromoModel data)
        {
            var client = new RestClient($"https://localhost:7207/api/Promocodes");
            var request = new RestRequest("https://localhost:7207/api/Promocodes", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(JsonConvert.SerializeObject(data));
            string token = GetJwTTokenFromPromoManagementSystem();
            client.Authenticator = new JwtAuthenticator(token.Replace("\"",""));
            RestResponse response = await client.ExecuteAsync(request);
            var output = response.Content;
        }
        public static  string GetJwTTokenFromPromoManagementSystem()
        {
            var client = new RestClient($"https://localhost:7207/api/Promocodes/GetToken");
            var request = new RestRequest("https://localhost:7207/api/Promocodes/GetToken", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("userName", _configuration.GetSection("JwT").GetValue<string>("User"));
            request.AddQueryParameter("password", _configuration.GetSection("JwT").GetValue<string>("Password"));
            RestResponse response =  client.Execute(request);
            var output = response.Content;
            return output;
        }

        

        private bool EVoucherExists(string id)
        {
            return (_context.TblEvouchers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
