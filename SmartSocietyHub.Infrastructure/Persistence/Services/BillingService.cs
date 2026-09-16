using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartSocietyHub.Application.Features.Billing.DTOs;
using SmartSocietyHub.Application.Features.Billing.Interfaces;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // BILLING CHARGES / SETTINGS - ADMIN
        // ============================================================

        public async Task<BillingChargesResponse?>
            GetBillingChargesAsync()
        {
            var settings = await _context.BillingSettings
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                return null;
            }

            return MapBillingChargesToResponse(settings);
        }

        public async Task<BillingChargesResponse>
            UpdateBillingChargesAsync(
                UpdateBillingChargesRequest request)
        {
            var settings = await _context.BillingSettings
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new BillingSettings
                {
                    Id = Guid.NewGuid(),
                    MaintenanceAmount = request.MaintenanceAmount,
                    SecurityAmount = request.SecurityAmount,
                    WaterAmount = request.WaterAmount,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BillingSettings.Add(settings);
            }
            else
            {
                settings.MaintenanceAmount =
                    request.MaintenanceAmount;

                settings.SecurityAmount =
                    request.SecurityAmount;

                settings.WaterAmount =
                    request.WaterAmount;

                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return MapBillingChargesToResponse(settings);
        }

        // ============================================================
        // BILL MANAGEMENT - ADMIN
        // ============================================================

        public async Task<IEnumerable<BillResponse>> GenerateBillsAsync(
            GenerateBillsRequest request)
        {
            var billingMonth = new DateTime(
                request.BillingMonth.Year,
                request.BillingMonth.Month,
                1);

            if (request.DueDate.Date < billingMonth.Date)
            {
                throw new ArgumentException(
                    "Due date cannot be earlier than the billing month.");
            }

            var billingSettings = await _context.BillingSettings
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            if (billingSettings == null)
            {
                throw new InvalidOperationException(
                    "Billing charges have not been configured. Please set the standard maintenance, security, and water charges before generating bills.");
            }

            var issueDate = DateTime.UtcNow.Date;

            var activeResidents = await _context.Residents
                .Include(r => r.Property)
                .Where(r => r.Status == "Active")
                .OrderBy(r => r.FullName)
                .ToListAsync();

            if (!activeResidents.Any())
            {
                return new List<BillResponse>();
            }

            var existingResidentIds = await _context.Bills
                .Where(b =>
                    b.BillingMonth.Year == billingMonth.Year &&
                    b.BillingMonth.Month == billingMonth.Month)
                .Select(b => b.ResidentId)
                .ToListAsync();

            var existingResidentIdSet =
                existingResidentIds.ToHashSet();

            var residentsToBill = activeResidents
                .Where(r => !existingResidentIdSet.Contains(r.Id))
                .ToList();

            if (!residentsToBill.Any())
            {
                throw new InvalidOperationException(
                    "Bills have already been generated for all active residents for this billing month.");
            }

            var totalAmount =
                billingSettings.MaintenanceAmount +
                billingSettings.SecurityAmount +
                billingSettings.WaterAmount;

            var nextInvoiceNumber =
                await GetNextInvoiceNumberAsync(billingMonth.Year);

            var generatedBills = new List<Bill>();

            foreach (var resident in residentsToBill)
            {
                var bill = new Bill
                {
                    Id = Guid.NewGuid(),

                    InvoiceNumber =
                        $"INV-{billingMonth.Year}-{nextInvoiceNumber:D4}",

                    ResidentId = resident.Id,

                    BillingMonth = billingMonth,

                    IssueDate = issueDate,

                    DueDate = request.DueDate.Date,

                    MaintenanceAmount =
                        billingSettings.MaintenanceAmount,

                    SecurityAmount =
                        billingSettings.SecurityAmount,

                    WaterAmount =
                        billingSettings.WaterAmount,

                    TotalAmount = totalAmount,

                    Status = request.DueDate.Date < issueDate
                        ? "Overdue"
                        : "Unpaid",

                    CreatedAt = DateTime.UtcNow
                };

                generatedBills.Add(bill);

                nextInvoiceNumber++;
            }

            _context.Bills.AddRange(generatedBills);

            await _context.SaveChangesAsync();

            var generatedBillIds =
                generatedBills
                    .Select(b => b.Id)
                    .ToList();

            var savedBills = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .Where(b => generatedBillIds.Contains(b.Id))
                .OrderBy(b => b.Resident.FullName)
                .ToListAsync();

            return savedBills
                .Select(MapBillToResponse)
                .ToList();
        }

        public async Task<IEnumerable<BillResponse>> GetAllBillsAsync()
        {
            await UpdateOverdueBillStatusesAsync();

            var bills = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .OrderByDescending(b => b.BillingMonth)
                .ThenBy(b => b.Resident.FullName)
                .ToListAsync();

            return bills
                .Select(MapBillToResponse)
                .ToList();
        }

        public async Task<BillResponse?> GetBillByIdAsync(
            Guid billId)
        {
            await UpdateOverdueBillStatusesAsync();

            return await GetBillResponseAsync(billId);
        }

        public async Task<BillResponse?> UpdateBillAsync(
            Guid billId,
            UpdateBillRequest request)
        {
            var billingMonth = new DateTime(
                request.BillingMonth.Year,
                request.BillingMonth.Month,
                1);

            if (request.DueDate.Date < billingMonth.Date)
            {
                throw new ArgumentException(
                    "Due date cannot be earlier than the billing month.");
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.Id == billId);

            if (bill == null)
            {
                return null;
            }

            if (bill.Status == "Paid")
            {
                throw new InvalidOperationException(
                    "Paid bills cannot be updated.");
            }

            if (bill.Status == "PendingVerification")
            {
                throw new InvalidOperationException(
                    "A bill with a payment pending verification cannot be updated.");
            }

            var duplicateBill = await _context.Bills
                .AnyAsync(b =>
                    b.Id != billId &&
                    b.ResidentId == bill.ResidentId &&
                    b.BillingMonth.Year == billingMonth.Year &&
                    b.BillingMonth.Month == billingMonth.Month);

            if (duplicateBill)
            {
                throw new InvalidOperationException(
                    "Another bill already exists for this resident and billing month.");
            }

            bill.BillingMonth = billingMonth;

            bill.DueDate = request.DueDate.Date;

            bill.MaintenanceAmount =
                request.MaintenanceAmount;

            bill.SecurityAmount =
                request.SecurityAmount;

            bill.WaterAmount =
                request.WaterAmount;

            bill.TotalAmount =
                request.MaintenanceAmount +
                request.SecurityAmount +
                request.WaterAmount;

            bill.Status =
                request.DueDate.Date < DateTime.UtcNow.Date
                    ? "Overdue"
                    : "Unpaid";

            bill.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetBillResponseAsync(bill.Id);
        }

        // ============================================================
        // INVOICE - ADMIN
        // ============================================================

        public async Task<byte[]?> GenerateInvoicePdfAsync(
            Guid billId)
        {
            var bill = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .FirstOrDefaultAsync(b => b.Id == billId);

            if (bill == null)
            {
                return null;
            }

            return GenerateInvoicePdf(bill);
        }

        // ============================================================
        // INVOICE - RESIDENT
        // ============================================================

        public async Task<byte[]?> GenerateMyInvoicePdfAsync(
            Guid applicationUserId,
            Guid billId)
        {
            var resident = await GetActiveResidentAsync(
                applicationUserId);

            var bill = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == billId &&
                    b.ResidentId == resident.Id);

            if (bill == null)
            {
                return null;
            }

            return GenerateInvoicePdf(bill);
        }

        // ============================================================
        // PDF INVOICE GENERATION
        // ============================================================

        private static byte[] GenerateInvoicePdf(Bill bill)
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            var residentName =
                bill.Resident?.FullName ?? "Resident";

            var houseNumber =
                bill.Resident?.Property?.HouseNumber ?? "N/A";

            var block =
                bill.Resident?.Property?.Block ?? "N/A";

            var billingMonth =
                bill.BillingMonth.ToString("MMMM yyyy");

            var issueDate =
                bill.IssueDate.ToString("dd MMM yyyy");

            var dueDate =
                bill.DueDate.ToString("dd MMM yyyy");

            var invoiceNumber =
                bill.InvoiceNumber;

            var status =
                bill.Status;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(text =>
                        text.FontFamily("Arial")
                            .FontSize(10));

                    // ====================================================
                    // HEADER
                    // ====================================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(left =>
                                        {
                                            left.Item()
                                                .Text("SMART SOCIETY HUB")
                                                .FontSize(22)
                                                .Bold()
                                                .FontColor(
                                                    Colors.Blue.Darken2);

                                            left.Item()
                                                .PaddingTop(3)
                                                .Text(
                                                    "Society Management System")
                                                .FontSize(10)
                                                .FontColor(
                                                    Colors.Grey.Darken1);
                                        });

                                    row.ConstantItem(180)
                                        .AlignRight()
                                        .Column(right =>
                                        {
                                            right.Item()
                                                .Text("INVOICE")
                                                .FontSize(24)
                                                .Bold()
                                                .FontColor(
                                                    Colors.Blue.Darken2);

                                            right.Item()
                                                .PaddingTop(3)
                                                .Text(invoiceNumber)
                                                .FontSize(11)
                                                .SemiBold();
                                        });
                                });

                            column.Item()
                                .PaddingTop(12)
                                .LineHorizontal(1)
                                .LineColor(
                                    Colors.Blue.Darken2);
                        });

                    // ====================================================
                    // CONTENT
                    // ====================================================

                    page.Content()
                        .PaddingTop(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // =================================================
                            // BILL TO + INVOICE DETAILS
                            // =================================================

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(left =>
                                        {
                                            left.Item()
                                                .Text("BILL TO")
                                                .FontSize(9)
                                                .Bold()
                                                .FontColor(
                                                    Colors.Grey.Darken1);

                                            left.Item()
                                                .PaddingTop(4)
                                                .Text(residentName)
                                                .FontSize(13)
                                                .Bold();

                                            left.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"House: {houseNumber}");

                                            left.Item()
                                                .Text(
                                                    $"Block: {block}");
                                        });

                                    row.ConstantItem(220)
                                        .Column(right =>
                                        {
                                            right.Item()
                                                .Row(info =>
                                                {
                                                    info.RelativeItem()
                                                        .Text(
                                                            "Invoice Number")
                                                        .SemiBold();

                                                    info.ConstantItem(110)
                                                        .AlignRight()
                                                        .Text(
                                                            invoiceNumber);
                                                });

                                            right.Item()
                                                .PaddingTop(4)
                                                .Row(info =>
                                                {
                                                    info.RelativeItem()
                                                        .Text(
                                                            "Billing Month")
                                                        .SemiBold();

                                                    info.ConstantItem(110)
                                                        .AlignRight()
                                                        .Text(
                                                            billingMonth);
                                                });

                                            right.Item()
                                                .PaddingTop(4)
                                                .Row(info =>
                                                {
                                                    info.RelativeItem()
                                                        .Text(
                                                            "Issue Date")
                                                        .SemiBold();

                                                    info.ConstantItem(110)
                                                        .AlignRight()
                                                        .Text(
                                                            issueDate);
                                                });

                                            right.Item()
                                                .PaddingTop(4)
                                                .Row(info =>
                                                {
                                                    info.RelativeItem()
                                                        .Text(
                                                            "Due Date")
                                                        .SemiBold();

                                                    info.ConstantItem(110)
                                                        .AlignRight()
                                                        .Text(
                                                            dueDate);
                                                });
                                        });
                                });

                            // =================================================
                            // PAYMENT STATUS
                            // =================================================

                            column.Item()
                                .Background(
                                    GetStatusBackgroundColor(status))
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("PAYMENT STATUS")
                                        .Bold();

                                    row.ConstantItem(150)
                                        .AlignRight()
                                        .Text(status)
                                        .Bold();
                                });

                            // =================================================
                            // CHARGES TABLE
                            // =================================================

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(
                                                Colors.Blue.Darken2)
                                            .Padding(9)
                                            .Text("Description")
                                            .FontColor(Colors.White)
                                            .Bold();

                                        header.Cell()
                                            .Background(
                                                Colors.Blue.Darken2)
                                            .Padding(9)
                                            .AlignRight()
                                            .Text("Amount")
                                            .FontColor(Colors.White)
                                            .Bold();
                                    });

                                    AddInvoiceRow(
                                        table,
                                        "Maintenance Charges",
                                        bill.MaintenanceAmount);

                                    AddInvoiceRow(
                                        table,
                                        "Security Charges",
                                        bill.SecurityAmount);

                                    AddInvoiceRow(
                                        table,
                                        "Water Charges",
                                        bill.WaterAmount);

                                    table.Cell()
                                        .ColumnSpan(2)
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            Colors.Grey.Lighten1);

                                    table.Cell()
                                        .Padding(10)
                                        .Text("TOTAL")
                                        .Bold()
                                        .FontSize(12);

                                    table.Cell()
                                        .Padding(10)
                                        .AlignRight()
                                        .Text(
                                            $"Rs. {bill.TotalAmount:N2}")
                                        .Bold()
                                        .FontSize(12);
                                });

                            // =================================================
                            // PAYMENT INSTRUCTIONS
                            // =================================================

                            column.Item()
                                .PaddingTop(10)
                                .Text("PAYMENT INSTRUCTIONS")
                                .FontSize(12)
                                .Bold()
                                .FontColor(
                                    Colors.Blue.Darken2);

                            column.Item()
                                .Background(
                                    Colors.Grey.Lighten4)
                                .Padding(12)
                                .Column(payment =>
                                {
                                    payment.Spacing(5);

                                    payment.Item()
                                        .Text(
                                            "Please pay the exact invoice amount through the society's designated bank account.");

                                    payment.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Bank Name: ")
                                                .Bold();

                                            text.Span("Bank");
                                        });

                                    payment.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Account Name: ")
                                                .Bold();

                                            text.Span("Tariq");
                                        });

                                    payment.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Account Number: ")
                                                .Bold();

                                            text.Span("************");
                                        });

                                    payment.Item()
                                        .PaddingTop(5)
                                        .Text(
                                            "After making the payment, upload your payment receipt or proof through the Smart Society Hub portal.");
                                });

                            // =================================================
                            // NOTE
                            // =================================================

                            column.Item()
                                .PaddingTop(10)
                                .Text(
                                    "This is a system-generated invoice. Please retain this invoice for your records.")
                                .FontSize(9)
                                .FontColor(
                                    Colors.Grey.Darken1);
                        });

                    // ====================================================
                    // FOOTER
                    // ====================================================

                    page.Footer()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .LineHorizontal(1)
                                .LineColor(
                                    Colors.Grey.Lighten1);

                            column.Item()
                                .PaddingTop(6)
                                .Text(text =>
                                {
                                    text.Span(
                                        "Smart Society Hub • Invoice ")
                                        .FontSize(8)
                                        .FontColor(
                                            Colors.Grey.Darken1);

                                    text.Span(invoiceNumber)
                                        .FontSize(8)
                                        .SemiBold();

                                    text.Span(" • Page ")
                                        .FontSize(8)
                                        .FontColor(
                                            Colors.Grey.Darken1);

                                    text.CurrentPageNumber()
                                        .FontSize(8);
                                });
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static void AddInvoiceRow(
            TableDescriptor table,
            string description,
            decimal amount)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(
                    Colors.Grey.Lighten2)
                .Padding(9)
                .Text(description);

            table.Cell()
                .BorderBottom(1)
                .BorderColor(
                    Colors.Grey.Lighten2)
                .Padding(9)
                .AlignRight()
                .Text(
                    $"Rs. {amount:N2}");
        }

        private static string GetStatusBackgroundColor(
            string status)
        {
            return status switch
            {
                "Paid" =>
                    Colors.Green.Lighten4,

                "PendingVerification" =>
                    Colors.Orange.Lighten4,

                "Overdue" =>
                    Colors.Red.Lighten4,

                _ =>
                    Colors.Grey.Lighten4
            };
        }

        // ============================================================
        // RESIDENT - BILL MANAGEMENT
        // ============================================================

        public async Task<IEnumerable<BillResponse>> GetMyBillsAsync(
            Guid applicationUserId)
        {
            var resident = await GetActiveResidentAsync(
                applicationUserId);

            await UpdateOverdueBillStatusesAsync(
                resident.Id);

            var bills = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .Where(b =>
                    b.ResidentId == resident.Id)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();

            return bills
                .Select(MapBillToResponse)
                .ToList();
        }

        public async Task<BillResponse?> GetMyBillByIdAsync(
            Guid applicationUserId,
            Guid billId)
        {
            var resident = await GetActiveResidentAsync(
                applicationUserId);

            await UpdateOverdueBillStatusesAsync(
                resident.Id);

            var bill = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == billId &&
                    b.ResidentId == resident.Id);

            if (bill == null)
            {
                return null;
            }

            return MapBillToResponse(bill);
        }

        // ============================================================
        // PAYMENT - RESIDENT
        // ============================================================

        public async Task<PaymentResponse> SubmitPaymentAsync(
            Guid applicationUserId,
            CreatePaymentRequest request)
        {
            var resident = await GetActiveResidentAsync(
                applicationUserId);

            await UpdateOverdueBillStatusesAsync(
                resident.Id);

            var bill = await _context.Bills
                .Include(b => b.Resident)
                .FirstOrDefaultAsync(b =>
                    b.Id == request.BillId &&
                    b.ResidentId == resident.Id);

            if (bill == null)
            {
                throw new KeyNotFoundException(
                    "Bill was not found.");
            }

            if (bill.Status == "Paid")
            {
                throw new InvalidOperationException(
                    "This bill has already been paid.");
            }

            if (bill.Status == "PendingVerification")
            {
                throw new InvalidOperationException(
                    "A payment for this bill is already pending verification.");
            }

            if (request.Amount != bill.TotalAmount)
            {
                throw new InvalidOperationException(
                    $"Payment amount must be exactly Rs. {bill.TotalAmount:N2}.");
            }

            if (string.IsNullOrWhiteSpace(
                request.PaymentProofPath))
            {
                throw new ArgumentException(
                    "Payment proof is required.");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),

                BillId = bill.Id,

                Amount = request.Amount,

                PaymentMethod = "Offline",

                PaymentProofPath =
                    request.PaymentProofPath,

                Status = "Pending",

                SubmittedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            bill.Status =
                "PendingVerification";

            bill.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPaymentResponseAsync(
                payment.Id)
                ?? throw new InvalidOperationException(
                    "Payment could not be submitted.");
        }

        public async Task<IEnumerable<PaymentResponse>>
            GetMyPaymentsAsync(
                Guid applicationUserId)
        {
            var resident = await GetActiveResidentAsync(
                applicationUserId);

            var payments = await _context.Payments
                .Include(p => p.Bill)
                    .ThenInclude(b => b.Resident)
                .Where(p =>
                    p.Bill.ResidentId == resident.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();

            return payments
                .Select(MapPaymentToResponse)
                .ToList();
        }

        // ============================================================
        // PAYMENT VERIFICATION - ADMIN
        // ============================================================

        public async Task<IEnumerable<PaymentResponse>>
            GetPendingPaymentsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Bill)
                    .ThenInclude(b => b.Resident)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.SubmittedAt)
                .ToListAsync();

            return payments
                .Select(MapPaymentToResponse)
                .ToList();
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(
            Guid paymentId)
        {
            return await GetPaymentResponseAsync(
                paymentId);
        }

        public async Task<PaymentResponse?> VerifyPaymentAsync(
            Guid paymentId,
            VerifyPaymentRequest request)
        {
            var payment = await _context.Payments
                .Include(p => p.Bill)
                .FirstOrDefaultAsync(p =>
                    p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            if (payment.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "Only pending payments can be verified or rejected.");
            }

            if (request.IsApproved)
            {
                if (payment.Amount !=
                    payment.Bill.TotalAmount)
                {
                    throw new InvalidOperationException(
                        "Payment amount does not match the bill total.");
                }

                payment.Status =
                    "Verified";

                payment.VerifiedAt =
                    DateTime.UtcNow;

                payment.RejectionReason =
                    null;

                payment.Bill.Status =
                    "Paid";

                payment.Bill.UpdatedAt =
                    DateTime.UtcNow;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(
                    request.RejectionReason))
                {
                    throw new ArgumentException(
                        "A rejection reason is required when rejecting a payment.");
                }

                payment.Status =
                    "Rejected";

                payment.VerifiedAt =
                    DateTime.UtcNow;

                payment.RejectionReason =
                    request.RejectionReason.Trim();

                payment.Bill.Status =
                    payment.Bill.DueDate.Date <
                    DateTime.UtcNow.Date
                        ? "Overdue"
                        : "Unpaid";

                payment.Bill.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetPaymentResponseAsync(
                payment.Id);
        }

        // ============================================================
        // ADMIN - PAYMENT PROOF
        // ============================================================

        public async Task<string?> GetPaymentProofPathAsync(
            Guid paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(
                payment.PaymentProofPath))
            {
                return null;
            }

            return payment.PaymentProofPath;
        }

        // ============================================================
        // RESIDENT LOOKUP
        // ============================================================

        private async Task<Resident> GetActiveResidentAsync(
            Guid applicationUserId)
        {
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            return resident;
        }

        // ============================================================
        // INVOICE NUMBER
        // ============================================================

        private async Task<int> GetNextInvoiceNumberAsync(
            int invoiceYear)
        {
            var prefix =
                $"INV-{invoiceYear}-";

            var lastInvoiceNumber =
                await _context.Bills
                    .Where(b =>
                        b.InvoiceNumber.StartsWith(prefix))
                    .OrderByDescending(b =>
                        b.InvoiceNumber)
                    .Select(b =>
                        b.InvoiceNumber)
                    .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(
                lastInvoiceNumber))
            {
                return 1;
            }

            var numberPart =
                lastInvoiceNumber.Substring(
                    prefix.Length);

            if (int.TryParse(
                numberPart,
                out var lastNumber))
            {
                return lastNumber + 1;
            }

            return 1;
        }

        // ============================================================
        // OVERDUE BILL STATUS
        // ============================================================

        private async Task UpdateOverdueBillStatusesAsync()
        {
            var today =
                DateTime.UtcNow.Date;

            var billsToUpdate =
                await _context.Bills
                    .Where(b =>
                        b.Status == "Unpaid" &&
                        b.DueDate.Date < today)
                    .ToListAsync();

            if (!billsToUpdate.Any())
            {
                return;
            }

            foreach (var bill in billsToUpdate)
            {
                bill.Status =
                    "Overdue";

                bill.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateOverdueBillStatusesAsync(
            Guid residentId)
        {
            var today =
                DateTime.UtcNow.Date;

            var billsToUpdate =
                await _context.Bills
                    .Where(b =>
                        b.ResidentId == residentId &&
                        b.Status == "Unpaid" &&
                        b.DueDate.Date < today)
                    .ToListAsync();

            if (!billsToUpdate.Any())
            {
                return;
            }

            foreach (var bill in billsToUpdate)
            {
                bill.Status =
                    "Overdue";

                bill.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // BILL RESPONSE
        // ============================================================

        private async Task<BillResponse?> GetBillResponseAsync(
            Guid billId)
        {
            var bill = await _context.Bills
                .Include(b => b.Resident)
                    .ThenInclude(r => r.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == billId);

            if (bill == null)
            {
                return null;
            }

            return MapBillToResponse(bill);
        }

        private static BillResponse MapBillToResponse(
            Bill bill)
        {
            return new BillResponse
            {
                Id = bill.Id,

                InvoiceNumber =
                    bill.InvoiceNumber,

                ResidentId =
                    bill.ResidentId,

                ResidentName =
                    bill.Resident?.FullName
                    ?? string.Empty,

                HouseNumber =
                    bill.Resident?.Property?.HouseNumber
                    ?? string.Empty,

                Block =
                    bill.Resident?.Property?.Block
                    ?? string.Empty,

                BillingMonth =
                    bill.BillingMonth,

                IssueDate =
                    bill.IssueDate,

                DueDate =
                    bill.DueDate,

                MaintenanceAmount =
                    bill.MaintenanceAmount,

                SecurityAmount =
                    bill.SecurityAmount,

                WaterAmount =
                    bill.WaterAmount,

                TotalAmount =
                    bill.TotalAmount,

                Status =
                    bill.Status,

                CreatedAt =
                    bill.CreatedAt,

                UpdatedAt =
                    bill.UpdatedAt
            };
        }

        // ============================================================
        // BILLING CHARGES RESPONSE
        // ============================================================

        private static BillingChargesResponse
            MapBillingChargesToResponse(
                BillingSettings settings)
        {
            return new BillingChargesResponse
            {
                MaintenanceAmount =
                    settings.MaintenanceAmount,

                SecurityAmount =
                    settings.SecurityAmount,

                WaterAmount =
                    settings.WaterAmount,

                TotalAmount =
                    settings.MaintenanceAmount +
                    settings.SecurityAmount +
                    settings.WaterAmount,

                UpdatedAt =
                    settings.UpdatedAt
            };
        }

        // ============================================================
        // PAYMENT RESPONSE
        // ============================================================

        private async Task<PaymentResponse?>
            GetPaymentResponseAsync(
                Guid paymentId)
        {
            var payment =
                await _context.Payments
                    .Include(p => p.Bill)
                        .ThenInclude(b => b.Resident)
                    .FirstOrDefaultAsync(p =>
                        p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            return MapPaymentToResponse(payment);
        }

        private static PaymentResponse
            MapPaymentToResponse(
                Payment payment)
        {
            return new PaymentResponse
            {
                Id =
                    payment.Id,

                BillId =
                    payment.BillId,

                InvoiceNumber =
                    payment.Bill?.InvoiceNumber
                    ?? string.Empty,

                ResidentId =
                    payment.Bill?.ResidentId
                    ?? Guid.Empty,

                ResidentName =
                    payment.Bill?.Resident?.FullName
                    ?? string.Empty,

                Amount =
                    payment.Amount,

                PaymentMethod =
                    payment.PaymentMethod,

                PaymentProofPath =
                    payment.PaymentProofPath,

                Status =
                    payment.Status,

                SubmittedAt =
                    payment.SubmittedAt,

                VerifiedAt =
                    payment.VerifiedAt,

                RejectionReason =
                    payment.RejectionReason
            };
        }
    }
}