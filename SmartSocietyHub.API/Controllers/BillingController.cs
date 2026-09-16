using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSocietyHub.Application.Features.Billing.DTOs;
using SmartSocietyHub.Application.Features.Billing.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly IWebHostEnvironment _environment;

        public BillingController(
            IBillingService billingService,
            IWebHostEnvironment environment)
        {
            _billingService = billingService;
            _environment = environment;
        }

        // ============================================================
        // ADMIN - BILLING CHARGES / SETTINGS
        // ============================================================

        // GET: api/Billing/charges
        //
        // Returns the currently saved standard monthly charges.
        [HttpGet("charges")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBillingCharges()
        {
            var charges =
                await _billingService.GetBillingChargesAsync();

            if (charges == null)
            {
                return NotFound(new
                {
                    message =
                        "Billing charges have not been configured yet."
                });
            }

            return Ok(charges);
        }

        // PUT: api/Billing/charges
        //
        // Creates or updates the standard monthly charges.
        [HttpPut("charges")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBillingCharges(
            [FromBody] UpdateBillingChargesRequest request)
        {
            try
            {
                var charges =
                    await _billingService.UpdateBillingChargesAsync(
                        request);

                return Ok(charges);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // ADMIN - BILL MANAGEMENT
        // ============================================================

        // POST: api/Billing/bills
        //
        // Generates bills automatically for all active residents
        // who do not already have a bill for the selected month.
        [HttpPost("bills")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateBills(
            [FromBody] GenerateBillsRequest request)
        {
            try
            {
                var bills =
                    await _billingService.GenerateBillsAsync(request);

                return Ok(bills);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/Billing/bills
        [HttpGet("bills")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBills()
        {
            var bills =
                await _billingService.GetAllBillsAsync();

            return Ok(bills);
        }

        // GET: api/Billing/bills/{billId}
        [HttpGet("bills/{billId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBillById(
            Guid billId)
        {
            var bill =
                await _billingService.GetBillByIdAsync(billId);

            if (bill == null)
            {
                return NotFound(new
                {
                    message = "Bill not found."
                });
            }

            return Ok(bill);
        }

        // PUT: api/Billing/bills/{billId}
        [HttpPut("bills/{billId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBill(
            Guid billId,
            [FromBody] UpdateBillRequest request)
        {
            try
            {
                var bill =
                    await _billingService.UpdateBillAsync(
                        billId,
                        request);

                if (bill == null)
                {
                    return NotFound(new
                    {
                        message = "Bill not found."
                    });
                }

                return Ok(bill);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // ADMIN - INVOICE
        // ============================================================

        // GET: api/Billing/bills/{billId}/invoice
        //
        // Generates and downloads the professional PDF invoice
        // for any bill. Admin only.
        [HttpGet("bills/{billId:guid}/invoice")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadInvoice(
            Guid billId)
        {
            var pdf =
                await _billingService.GenerateInvoicePdfAsync(
                    billId);

            if (pdf == null)
            {
                return NotFound(new
                {
                    message = "Bill not found."
                });
            }

            var bill =
                await _billingService.GetBillByIdAsync(
                    billId);

            var fileName =
                bill == null
                    ? $"invoice-{billId}.pdf"
                    : $"{bill.InvoiceNumber}.pdf";

            return File(
                pdf,
                "application/pdf",
                fileName);
        }

        // ============================================================
        // RESIDENT - BILL MANAGEMENT
        // ============================================================

        // GET: api/Billing/my-bills
        [HttpGet("my-bills")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyBills()
        {
            try
            {
                var applicationUserId =
                    GetCurrentUserId();

                var bills =
                    await _billingService.GetMyBillsAsync(
                        applicationUserId);

                return Ok(bills);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/Billing/my-bills/{billId}
        [HttpGet("my-bills/{billId:guid}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyBillById(
            Guid billId)
        {
            try
            {
                var applicationUserId =
                    GetCurrentUserId();

                var bill =
                    await _billingService.GetMyBillByIdAsync(
                        applicationUserId,
                        billId);

                if (bill == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Bill not found or does not belong to you."
                    });
                }

                return Ok(bill);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // RESIDENT - INVOICE
        // ============================================================

        // GET: api/Billing/my-bills/{billId}/invoice
        //
        // Generates and downloads the professional PDF invoice
        // for the logged-in resident's own bill.
        [HttpGet("my-bills/{billId:guid}/invoice")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> DownloadMyInvoice(
            Guid billId)
        {
            try
            {
                var applicationUserId =
                    GetCurrentUserId();

                var pdf =
                    await _billingService.GenerateMyInvoicePdfAsync(
                        applicationUserId,
                        billId);

                if (pdf == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Bill not found or does not belong to you."
                    });
                }

                var bill =
                    await _billingService.GetMyBillByIdAsync(
                        applicationUserId,
                        billId);

                var fileName =
                    bill == null
                        ? $"invoice-{billId}.pdf"
                        : $"{bill.InvoiceNumber}.pdf";

                return File(
                    pdf,
                    "application/pdf",
                    fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // RESIDENT - PAYMENT
        // ============================================================

        // POST: api/Billing/payments
        //
        // Content-Type: multipart/form-data
        //
        // Form fields:
        // BillId
        // Amount
        // PaymentProof
        [HttpPost("payments")]
        [Authorize(Roles = "Resident")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitPayment(
            [FromForm] Guid billId,
            [FromForm] decimal amount,
            IFormFile paymentProof)
        {
            try
            {
                if (paymentProof == null ||
                    paymentProof.Length == 0)
                {
                    return BadRequest(new
                    {
                        message =
                            "Payment proof image is required."
                    });
                }

                // Only image files are allowed.
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension =
                    Path.GetExtension(
                        paymentProof.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message =
                            "Only JPG, JPEG, and PNG images are allowed."
                    });
                }

                // Maximum file size: 5 MB
                const long maxFileSize =
                    5 * 1024 * 1024;

                if (paymentProof.Length > maxFileSize)
                {
                    return BadRequest(new
                    {
                        message =
                            "Payment proof image cannot exceed 5 MB."
                    });
                }

                var uploadFolder =
                    Path.Combine(
                        _environment.WebRootPath
                        ?? Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot"),
                        "uploads",
                        "payment-proofs");

                Directory.CreateDirectory(uploadFolder);

                var uniqueFileName =
                    $"{Guid.NewGuid()}{extension}";

                var filePath =
                    Path.Combine(
                        uploadFolder,
                        uniqueFileName);

                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await paymentProof.CopyToAsync(stream);
                }

                // Store a relative path in the database.
                var paymentProofPath =
                    $"/uploads/payment-proofs/{uniqueFileName}";

                var request =
                    new CreatePaymentRequest
                    {
                        BillId = billId,
                        Amount = amount,
                        PaymentProofPath =
                            paymentProofPath
                    };

                var applicationUserId =
                    GetCurrentUserId();

                var payment =
                    await _billingService.SubmitPaymentAsync(
                        applicationUserId,
                        request);

                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/Billing/my-payments
        [HttpGet("my-payments")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var applicationUserId =
                    GetCurrentUserId();

                var payments =
                    await _billingService.GetMyPaymentsAsync(
                        applicationUserId);

                return Ok(payments);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // ADMIN - PAYMENT VERIFICATION
        // ============================================================

        // GET: api/Billing/payments/pending
        [HttpGet("payments/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingPayments()
        {
            var payments =
                await _billingService.GetPendingPaymentsAsync();

            return Ok(payments);
        }

        // GET: api/Billing/payments/{paymentId}
        [HttpGet("payments/{paymentId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentById(
            Guid paymentId)
        {
            var payment =
                await _billingService.GetPaymentByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Payment not found."
                });
            }

            return Ok(payment);
        }

        // PUT: api/Billing/payments/{paymentId}/verify
        [HttpPut("payments/{paymentId:guid}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyPayment(
            Guid paymentId,
            [FromBody] VerifyPaymentRequest request)
        {
            try
            {
                var payment =
                    await _billingService.VerifyPaymentAsync(
                        paymentId,
                        request);

                if (payment == null)
                {
                    return NotFound(new
                    {
                        message = "Payment not found."
                    });
                }

                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // ADMIN - PAYMENT PROOF
        // ============================================================

        // GET: api/Billing/payments/{paymentId}/proof
        //
        // Returns the actual uploaded payment proof image.
        // Admin only.
        [HttpGet("payments/{paymentId:guid}/proof")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentProof(
            Guid paymentId)
        {
            var paymentProofPath =
                await _billingService.GetPaymentProofPathAsync(
                    paymentId);

            if (string.IsNullOrWhiteSpace(paymentProofPath))
            {
                return NotFound(new
                {
                    message =
                        "Payment proof was not found."
                });
            }

            // Convert the stored relative path into a safe
            // physical path inside wwwroot.
            var relativePath =
                paymentProofPath
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            var webRootPath =
                _environment.WebRootPath
                ?? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

            var fullPath =
                Path.GetFullPath(
                    Path.Combine(
                        webRootPath,
                        relativePath));

            var normalizedWebRoot =
                Path.GetFullPath(
                    webRootPath)
                .TrimEnd(
                    Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            // Prevent path traversal outside wwwroot.
            if (!fullPath.StartsWith(
                    normalizedWebRoot,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Invalid payment proof path."
                });
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new
                {
                    message =
                        "Payment proof file could not be found on the server."
                });
            }

            var extension =
                Path.GetExtension(fullPath)
                    .ToLowerInvariant();

            var contentType =
                extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };

            // Return the actual image instead of only returning
            // the database path.
            return PhysicalFile(
                fullPath,
                contentType);
        }

        // ============================================================
        // CURRENT USER
        // ============================================================

        private Guid GetCurrentUserId()
        {
            var userId =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;

            if (!Guid.TryParse(
                    userId,
                    out var applicationUserId))
            {
                throw new InvalidOperationException(
                    "Unable to identify the logged-in user.");
            }

            return applicationUserId;
        }
    }
}